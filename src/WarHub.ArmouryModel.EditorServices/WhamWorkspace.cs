using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.EditorServices;

/// <summary>
/// Manages the current state of catalogues and multiple open rosters with lazy per-roster
/// compilation tracking, document identity, and change notifications.
/// Analogous to Roslyn's <c>Workspace</c>: mutable entry point that owns all mutations.
/// </summary>
/// <remarks>
/// <para>
/// The workspace owns all roster mutations — <see cref="RosterEditor"/> is not exposed publicly.
/// Consumers must use <see cref="ApplyOperation"/>, <see cref="Undo"/>, and <see cref="Redo"/>
/// to ensure all state changes (including undo/redo) fire workspace events.
/// </para>
/// <para>
/// Catalogue changes rebuild the shared <see cref="CatalogueCompilation"/> and reset all
/// roster editors (undo history is lost). This is acceptable because catalogue changes are rare.
/// </para>
/// </remarks>
public sealed class WhamWorkspace
{
    private readonly object syncRoot = new();
    private long version;
    private WhamCompilation catalogueCompilation;
    private ImmutableDictionary<DocumentId, SourceTree> catalogueTrees;
    private ImmutableDictionary<DocumentId, RosterDocumentState> rosterStates;

    private WhamWorkspace(
        ImmutableDictionary<DocumentId, SourceTree> catalogueTrees,
        WhamCompilation catalogueCompilation)
    {
        this.catalogueTrees = catalogueTrees;
        this.catalogueCompilation = catalogueCompilation;
        rosterStates = ImmutableDictionary<DocumentId, RosterDocumentState>.Empty;
    }

    /// <summary>
    /// Fired when the workspace state changes. Events are fired outside the lock.
    /// The <see cref="WorkspaceChangedEventArgs.Version"/> can be used to detect stale events.
    /// </summary>
    public event EventHandler<WorkspaceChangedEventArgs>? WorkspaceChanged;

    /// <summary>
    /// Monotonically increasing version number, incremented on every state change.
    /// </summary>
    public long Version
    {
        get { lock (syncRoot) { return version; } }
    }

    /// <summary>
    /// The shared catalogue compilation containing all loaded catalogues and the gamesystem.
    /// </summary>
    public WhamCompilation CatalogueCompilation
    {
        get { lock (syncRoot) { return catalogueCompilation; } }
    }

    /// <summary>
    /// Document IDs for all loaded catalogues (including the gamesystem).
    /// </summary>
    public ImmutableArray<DocumentId> CatalogueDocumentIds
    {
        get { lock (syncRoot) { return [.. catalogueTrees.Keys]; } }
    }

    /// <summary>
    /// Document IDs for all open rosters.
    /// </summary>
    public ImmutableArray<DocumentId> RosterDocumentIds
    {
        get { lock (syncRoot) { return [.. rosterStates.Keys]; } }
    }

    #region Construction

    /// <summary>
    /// Creates a workspace with the given catalogue/gamesystem nodes.
    /// </summary>
    public static WhamWorkspace Create(params SourceNode[] catalogueNodes)
        => Create((IEnumerable<SourceNode>)catalogueNodes);

    /// <summary>
    /// Creates a workspace with the given catalogue/gamesystem nodes.
    /// </summary>
    public static WhamWorkspace Create(IEnumerable<SourceNode> catalogueNodes)
    {
        var treesBuilder = ImmutableDictionary.CreateBuilder<DocumentId, SourceTree>();
        foreach (var node in catalogueNodes)
        {
            treesBuilder.Add(DocumentId.CreateNew(), SourceTree.CreateForRoot(node));
        }
        var trees = treesBuilder.ToImmutable();
        var compilation = WhamCompilation.Create([.. trees.Values]);
        return new WhamWorkspace(trees, compilation);
    }

    #endregion

    #region Catalogue Management

    /// <summary>
    /// Gets the source tree for a catalogue document.
    /// </summary>
    public SourceTree GetCatalogueTree(DocumentId id)
    {
        lock (syncRoot)
        {
            return catalogueTrees.TryGetValue(id, out var tree)
                ? tree
                : throw new ArgumentException($"Catalogue document '{id.Id}' not found.", nameof(id));
        }
    }

    /// <summary>
    /// Adds a catalogue (or gamesystem) node and returns its new <see cref="DocumentId"/>.
    /// Rebuilds the catalogue compilation and resets all roster editors.
    /// </summary>
    public DocumentId AddCatalogue(SourceNode catalogueNode)
    {
        var docId = DocumentId.CreateNew();
        var tree = SourceTree.CreateForRoot(catalogueNode);
        List<WorkspaceChangedEventArgs> events;
        lock (syncRoot)
        {
            catalogueTrees = catalogueTrees.Add(docId, tree);
            RebuildCatalogueCompilationLocked();
            version++;
            events = [new(WorkspaceChangeKind.CatalogueAdded, docId, version)];
            events.AddRange(ResetAllRosterEditorsLocked());
        }
        RaiseEvents(events);
        return docId;
    }

    /// <summary>
    /// Removes a catalogue document. Rebuilds the catalogue compilation and resets all roster editors.
    /// </summary>
    public void RemoveCatalogue(DocumentId id)
    {
        List<WorkspaceChangedEventArgs> events;
        lock (syncRoot)
        {
            if (!catalogueTrees.ContainsKey(id))
                throw new ArgumentException($"Catalogue document '{id.Id}' not found.", nameof(id));
            catalogueTrees = catalogueTrees.Remove(id);
            RebuildCatalogueCompilationLocked();
            version++;
            events = [new(WorkspaceChangeKind.CatalogueRemoved, id, version)];
            events.AddRange(ResetAllRosterEditorsLocked());
        }
        RaiseEvents(events);
    }

    /// <summary>
    /// Replaces a catalogue node. Rebuilds the catalogue compilation and resets all roster editors.
    /// </summary>
    public void ReplaceCatalogue(DocumentId id, SourceNode newNode)
    {
        List<WorkspaceChangedEventArgs> events;
        lock (syncRoot)
        {
            if (!catalogueTrees.ContainsKey(id))
                throw new ArgumentException($"Catalogue document '{id.Id}' not found.", nameof(id));
            catalogueTrees = catalogueTrees.SetItem(id, SourceTree.CreateForRoot(newNode));
            RebuildCatalogueCompilationLocked();
            version++;
            events = [new(WorkspaceChangeKind.CatalogueChanged, id, version)];
            events.Add(new(WorkspaceChangeKind.CatalogueCompilationChanged, id, version));
            events.AddRange(ResetAllRosterEditorsLocked());
        }
        RaiseEvents(events);
    }

    #endregion

    #region Roster Management

    /// <summary>
    /// Opens an existing roster node in the workspace and returns its <see cref="DocumentId"/>.
    /// </summary>
    public DocumentId OpenRoster(SourceNode rosterNode)
    {
        var docId = DocumentId.CreateNew();
        var tree = SourceTree.CreateForRoot(rosterNode);
        WorkspaceChangedEventArgs evt;
        lock (syncRoot)
        {
            var catComp = catalogueCompilation;
            var tracker = new CompilationTracker(tree, catComp);
            // Use the tracker's compilation to avoid building two equivalent compilations.
            var rosterComp = tracker.GetCompilation();
            var state = new RosterState(rosterComp);
            var editor = new RosterEditor(state);
            rosterStates = rosterStates.Add(docId, new(docId, editor, tracker));
            version++;
            evt = new(WorkspaceChangeKind.RosterOpened, docId, version);
        }
        RaiseEvents([evt]);
        return docId;
    }

    /// <summary>
    /// Creates a new empty roster in the workspace and returns its <see cref="DocumentId"/>.
    /// Requires at least a gamesystem to be loaded in the catalogues.
    /// </summary>
    public DocumentId OpenRoster()
    {
        var docId = DocumentId.CreateNew();
        WorkspaceChangedEventArgs evt;
        lock (syncRoot)
        {
            var catComp = catalogueCompilation;
            // Create a catalogue-only RosterState, then apply CreateRoster
            var catState = new RosterState(catComp);
            var createOp = RosterOperations.CreateRoster();
            var rosterState = ((IRosterOperation)createOp).Apply(catState);
            var rosterTree = rosterState.RosterRequired.GetSourceTree(rosterState.Compilation);
            var tracker = new CompilationTracker(rosterTree, catComp);
            var editor = new RosterEditor(rosterState);
            rosterStates = rosterStates.Add(docId, new(docId, editor, tracker));
            version++;
            evt = new(WorkspaceChangeKind.RosterOpened, docId, version);
        }
        RaiseEvents([evt]);
        return docId;
    }

    /// <summary>
    /// Closes a roster, removing it from the workspace.
    /// </summary>
    public void CloseRoster(DocumentId id)
    {
        WorkspaceChangedEventArgs evt;
        lock (syncRoot)
        {
            if (!rosterStates.ContainsKey(id))
                throw new ArgumentException($"Roster document '{id.Id}' not found.", nameof(id));
            rosterStates = rosterStates.Remove(id);
            version++;
            evt = new(WorkspaceChangeKind.RosterClosed, id, version);
        }
        RaiseEvents([evt]);
    }

    #endregion

    #region Roster Mutations

    /// <summary>
    /// Applies a roster operation and returns the new <see cref="RosterState"/>.
    /// </summary>
    public RosterState ApplyOperation(DocumentId rosterId, IRosterOperation operation)
    {
        RosterState newState;
        WorkspaceChangedEventArgs evt;
        lock (syncRoot)
        {
            var doc = GetRosterDocumentLocked(rosterId);
            doc.Editor.ApplyOperation(operation);
            newState = doc.Editor.State;
            UpdateTrackerFromEditorLocked(rosterId, doc);
            version++;
            evt = new(WorkspaceChangeKind.RosterStateChanged, rosterId, version);
        }
        RaiseEvents([evt]);
        return newState;
    }

    /// <summary>
    /// Undoes the last roster operation. Returns false if at initial state.
    /// </summary>
    public bool Undo(DocumentId rosterId)
    {
        bool result;
        WorkspaceChangedEventArgs? evt = null;
        lock (syncRoot)
        {
            var doc = GetRosterDocumentLocked(rosterId);
            result = doc.Editor.Undo();
            if (result)
            {
                UpdateTrackerFromEditorLocked(rosterId, doc);
                version++;
                evt = new(WorkspaceChangeKind.RosterStateChanged, rosterId, version);
            }
        }
        if (evt is not null)
            RaiseEvents([evt]);
        return result;
    }

    /// <summary>
    /// Redoes the last undone roster operation. Returns false if nothing to redo.
    /// </summary>
    public bool Redo(DocumentId rosterId)
    {
        bool result;
        WorkspaceChangedEventArgs? evt = null;
        lock (syncRoot)
        {
            var doc = GetRosterDocumentLocked(rosterId);
            result = doc.Editor.Redo();
            if (result)
            {
                UpdateTrackerFromEditorLocked(rosterId, doc);
                version++;
                evt = new(WorkspaceChangeKind.RosterStateChanged, rosterId, version);
            }
        }
        if (evt is not null)
            RaiseEvents([evt]);
        return result;
    }

    /// <summary>
    /// Returns true if the roster has operations that can be undone.
    /// </summary>
    public bool CanUndo(DocumentId rosterId)
    {
        lock (syncRoot)
        {
            return GetRosterDocumentLocked(rosterId).Editor.CanUndo;
        }
    }

    /// <summary>
    /// Returns true if the roster has undone operations that can be redone.
    /// </summary>
    public bool CanRedo(DocumentId rosterId)
    {
        lock (syncRoot)
        {
            return GetRosterDocumentLocked(rosterId).Editor.CanRedo;
        }
    }

    #endregion

    #region Read Access

    /// <summary>
    /// Gets the current <see cref="RosterState"/> for the given roster.
    /// </summary>
    public RosterState GetRosterState(DocumentId rosterId)
    {
        lock (syncRoot)
        {
            return GetRosterDocumentLocked(rosterId).Editor.State;
        }
    }

    /// <summary>
    /// Gets the lazily-computed roster compilation for the given roster.
    /// Returns a point-in-time snapshot: the compilation is built from the catalogue
    /// compilation and roster tree that were current when the tracker was last updated.
    /// Concurrent edits may have occurred after the snapshot was captured.
    /// </summary>
    public WhamCompilation GetRosterCompilation(DocumentId rosterId)
    {
        CompilationTracker tracker;
        lock (syncRoot)
        {
            tracker = GetRosterDocumentLocked(rosterId).Tracker;
        }
        return tracker.GetCompilation();
    }

    /// <summary>
    /// Finds a document ID by root node ID and source kind.
    /// Returns null if not found or if <paramref name="rootNodeId"/> is null.
    /// </summary>
    public DocumentId? TryFindDocumentId(SourceKind kind, string? rootNodeId)
    {
        if (rootNodeId is null)
            return null;

        lock (syncRoot)
        {
            if (kind is SourceKind.Roster)
            {
                foreach (var (docId, doc) in rosterStates)
                {
                    if (doc.Tracker.RosterTree.GetRoot() is IIdentifiableNode idNode && idNode.Id == rootNodeId)
                        return docId;
                }
            }
            else
            {
                foreach (var (docId, tree) in catalogueTrees)
                {
                    var root = tree.GetRoot();
                    if (root.Kind == kind && root is IIdentifiableNode idNode && idNode.Id == rootNodeId)
                        return docId;
                }
            }
        }
        return null;
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Gets diagnostics for the given roster synchronously.
    /// </summary>
    public ImmutableArray<Diagnostic> GetDiagnostics(DocumentId rosterId)
    {
        var compilation = GetRosterCompilation(rosterId);
        return compilation.GetDiagnostics();
    }

    /// <summary>
    /// Gets diagnostics for the given roster on a background thread.
    /// Captures a compilation snapshot first to avoid races with concurrent edits.
    /// </summary>
    public Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(
        DocumentId rosterId, CancellationToken cancellationToken = default)
    {
        var compilation = GetRosterCompilation(rosterId);
        return Task.Run(() => compilation.GetDiagnostics(cancellationToken), cancellationToken);
    }

    #endregion

    #region Internal Helpers

    private RosterDocumentState GetRosterDocumentLocked(DocumentId id)
    {
        return rosterStates.TryGetValue(id, out var doc)
            ? doc
            : throw new ArgumentException($"Roster document '{id.Id}' not found.", nameof(id));
    }

    private void RebuildCatalogueCompilationLocked()
    {
        catalogueCompilation = WhamCompilation.Create([.. catalogueTrees.Values]);
    }

    private void UpdateTrackerFromEditorLocked(DocumentId id, RosterDocumentState doc)
    {
        var rosterState = doc.Editor.State;
        var rosterTree = rosterState.RosterRequired.GetSourceTree(rosterState.Compilation);
        var newTracker = doc.Tracker.WithRosterTree(rosterTree);
        rosterStates = rosterStates.SetItem(id, doc with { Tracker = newTracker });
    }

    private List<WorkspaceChangedEventArgs> ResetAllRosterEditorsLocked()
    {
        var events = new List<WorkspaceChangedEventArgs>();
        var catComp = catalogueCompilation;
        var updatedRosters = rosterStates.ToBuilder();
        foreach (var (docId, doc) in rosterStates)
        {
            var rosterTree = doc.Tracker.RosterTree;
            var rosterComp = WhamCompilation.CreateRosterCompilation([rosterTree], catComp);
            var newState = new RosterState(rosterComp);
            var newEditor = new RosterEditor(newState);
            var newTracker = doc.Tracker.WithCatalogueCompilation(catComp);
            updatedRosters[docId] = new(docId, newEditor, newTracker);
            events.Add(new(WorkspaceChangeKind.RosterStateChanged, docId, version));
        }
        rosterStates = updatedRosters.ToImmutable();
        return events;
    }

    private void RaiseEvents(IReadOnlyList<WorkspaceChangedEventArgs> events)
    {
        var handler = WorkspaceChanged;
        if (handler is null)
            return;
        foreach (var evt in events)
        {
            handler(this, evt);
        }
    }

    #endregion

    /// <summary>
    /// Internal per-roster state: editor for undo/redo, tracker for lazy compilation.
    /// </summary>
    internal sealed record RosterDocumentState(
        DocumentId Id,
        RosterEditor Editor,
        CompilationTracker Tracker);
}
