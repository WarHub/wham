using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using WarHub.ArmouryModel.Source;
using ProtocolRosterState = BattleScribeSpec.RosterState;
using WhamRosterState = WarHub.ArmouryModel.EditorServices.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Adapts the ISymbol-based <see cref="WhamRosterEngine"/> to the BattleScribeSpec
/// <see cref="IRosterEngine"/> interface. Thin translation layer:
/// <list type="bullet">
///   <item>Protocol ID → symbol resolution</item>
///   <item>Delegation to engine's ID-based API</item>
///   <item>ISymbol roster tree → Protocol state mapping via <see cref="StateMapper"/></item>
/// </list>
/// </summary>
public sealed class SpecRosterEngineAdapter : IRosterEngine
{
    private WhamRosterEngine? _coreEngine;
    private WhamRosterState? _state;
    private WhamCompilation? _catalogCompilation;
    private readonly EntryResolver _resolver = new();

    // Force ID → catalogue mapping: tracks which catalogue each force was added with.
    private readonly Dictionary<string, ICatalogueSymbol> _forceCatalogues = new(StringComparer.Ordinal);

    // Tracks forces that have received explicit user selections (SelectEntry/SelectChildEntry).
    // Forces with only auto-selections (from AddForce) are not included.
    private readonly HashSet<string> _forcesWithExplicitSelections = new(StringComparer.Ordinal);

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        var compilation = ProtocolConverter.CreateCompilation(gameSystem, catalogues);
        _catalogCompilation = compilation;
        _coreEngine = new WhamRosterEngine();
        _state = _coreEngine.CreateRoster(compilation);
        _forceCatalogues.Clear();
        _forcesWithExplicitSelections.Clear();
        return [];
    }

    public ActionOutputs AddForce(string forceEntryId, string catalogueId)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var compilation = (WhamCompilation)state.Compilation;

        var catalogue = ResolveCatalogueById(compilation, catalogueId);
        var forceEntry = ResolveForceEntryById(compilation, forceEntryId);

        var result = engine.AddForceById(state, forceEntry, catalogue);
        _state = result.State;

        if (result.ForceId is not null)
            _forceCatalogues[result.ForceId] = catalogue;

        return new ActionOutputs
        {
            ForceId = result.ForceId,
            Selections = result.Selections
        };
    }

    public ActionOutputs AddChildForce(string parentForceId, string forceEntryId, string catalogueId)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var compilation = (WhamCompilation)state.Compilation;

        var catalogue = ResolveCatalogueById(compilation, catalogueId);
        var forceEntry = ResolveForceEntryById(compilation, forceEntryId);

        var result = engine.AddChildForceById(state, parentForceId, forceEntry, catalogue);
        _state = result.State;

        if (result.ForceId is not null)
            _forceCatalogues[result.ForceId] = catalogue;

        return new ActionOutputs { ForceId = result.ForceId };
    }

    public void RemoveForce(string forceId)
    {
        var engine = EnsureEngine();
        _state = engine.RemoveForceById(EnsureState(), forceId);
        _forceCatalogues.Remove(forceId);
        _forcesWithExplicitSelections.Remove(forceId);
    }

    public ActionOutputs SelectEntry(string forceId, string entryId)
    {
        _forcesWithExplicitSelections.Add(forceId);
        var engine = EnsureEngine();
        var state = EnsureState();

        var catalogue = ResolveCatalogueForForce(state, forceId);
        var available = _resolver.GetAvailableEntries(catalogue);
        var avail = FindAvailableEntryById(available, entryId);

        var result = engine.SelectEntryById(state, forceId, avail.Symbol, avail.SourceGroup);
        _state = result.State;

        return new ActionOutputs
        {
            SelectionId = result.SelectionId,
            Selections = result.Selections
        };
    }

    public ActionOutputs SelectChildEntry(string forceId, string parentSelectionId, string entryId)
    {
        _forcesWithExplicitSelections.Add(forceId);
        var engine = EnsureEngine();
        var state = EnsureState();

        var parentEntry = FindEntrySymbolForSelection(state, forceId, parentSelectionId);
        if (parentEntry is null)
        {
            throw new InvalidOperationException(
                $"Could not find entry symbol for selection '{parentSelectionId}' in force '{forceId}'");
        }

        var childEntries = _resolver.GetChildEntries(parentEntry);
        var childAvail = FindAvailableEntryById(childEntries, entryId);

        var result = engine.SelectChildEntryById(state, forceId, parentSelectionId,
            childAvail.Symbol, childAvail.SourceGroup);
        _state = result.State;

        return new ActionOutputs
        {
            SelectionId = result.SelectionId,
            Selections = result.Selections
        };
    }

    public void DeselectSelection(string forceId, string selectionId)
    {
        _state = EnsureEngine().DeselectSelectionById(EnsureState(), forceId, selectionId);
    }

    public void SetSelectionCount(string forceId, string selectionId, int count)
    {
        _state = EnsureEngine().SetSelectionCountById(EnsureState(), forceId, selectionId, count);
    }

    public ActionOutputs DuplicateSelection(string forceId, string selectionId)
    {
        var result = EnsureEngine().DuplicateSelectionById(EnsureState(), forceId, selectionId);
        _state = result.State;
        _forcesWithExplicitSelections.Add(forceId);
        return new ActionOutputs { SelectionId = result.SelectionId };
    }

    public ActionOutputs DuplicateForce(string forceId)
    {
        var result = EnsureEngine().DuplicateForceById(EnsureState(), forceId);
        _state = result.State;
        var newForceId = result.ForceId!;
        if (_forceCatalogues.TryGetValue(forceId, out var catalogue))
            _forceCatalogues[newForceId] = catalogue;
        // Duplicated force inherits explicit status from original
        if (_forcesWithExplicitSelections.Contains(forceId))
            _forcesWithExplicitSelections.Add(newForceId);
        return new ActionOutputs { ForceId = newForceId };
    }

    public void SetCostLimit(string costTypeId, double value)
    {
        _state = EnsureEngine().SetCostLimit(EnsureState(), costTypeId, (decimal)value);
    }

    public void SetCustomization(string forceId, string? selectionId, string? categoryEntryId,
        string? customName, string? customNotes)
    {
        _state = EnsureEngine().SetCustomizationById(
            EnsureState(), forceId, selectionId, categoryEntryId, customName, customNotes);
    }

    public ProtocolRosterState GetRosterState()
    {
        var state = EnsureState();
        var compilation = (WhamCompilation)state.Compilation;

        var rosterSymbol = compilation.SourceGlobalNamespace.Rosters
            .FirstOrDefault(r => r.Declaration == state.RosterRequired)
            ?? compilation.SourceGlobalNamespace.Rosters.FirstOrDefault();

        // Compute per-force available entry counts and referenced cost types
        var counts = new List<int>();
        var referencedCostTypes = new HashSet<string>(StringComparer.Ordinal);
        var roster = state.RosterRequired;
        for (var i = 0; i < roster.Forces.Count; i++)
        {
            var force = roster.Forces[i];
            var catalogue = ResolveCatalogueForForce(state, force.Id!);
            var entries = _resolver.GetAvailableEntries(catalogue);
            counts.Add(_resolver.GetRootEntryCount(catalogue));
            foreach (var entry in entries)
            {
                StateMapper.CollectReferencedCostTypes(entry.Symbol, referencedCostTypes);
            }
        }

        var mapper = new StateMapper(rosterSymbol!, compilation);
        return mapper.MapRosterState(counts, referencedCostTypes);
    }

    public IReadOnlyList<ValidationErrorState> GetValidationErrors()
    {
        var state = GetRosterState();
        return HiddenConstraintFilter.Apply(
            state.ValidationErrors,
            _forcesWithExplicitSelections,
            state.Forces);
    }

    public void Dispose()
    {
        _coreEngine = null;
        _state = null;
        _catalogCompilation = null;
        _forceCatalogues.Clear();
        _forcesWithExplicitSelections.Clear();
    }

    // ──────────────────────────────────────────────────────────────────
    //  Internal helpers
    // ──────────────────────────────────────────────────────────────────

    private WhamRosterEngine EnsureEngine()
        => _coreEngine ?? throw new InvalidOperationException("Engine not set up. Call Setup first.");

    private WhamRosterState EnsureState()
        => _state ?? throw new InvalidOperationException("Engine not set up. Call Setup first.");

    /// <summary>
    /// Resolves a catalogue symbol by its ID from the compilation.
    /// </summary>
    private static ICatalogueSymbol ResolveCatalogueById(WhamCompilation compilation, string catalogueId)
    {
        foreach (var cat in compilation.GlobalNamespace.Catalogues)
        {
            if (cat.Id == catalogueId) return cat;
        }
        throw new ArgumentException($"Catalogue '{catalogueId}' not found.", nameof(catalogueId));
    }

    /// <summary>
    /// Resolves a force entry symbol by its ID from all catalogues in the compilation.
    /// </summary>
    private static IForceEntrySymbol ResolveForceEntryById(WhamCompilation compilation, string forceEntryId)
    {
        foreach (var cat in compilation.GlobalNamespace.Catalogues)
        {
            foreach (var entry in cat.RootContainerEntries)
            {
                if (entry is IForceEntrySymbol fe && fe.Id == forceEntryId)
                    return fe;
                if (entry is IForceEntrySymbol feParent)
                {
                    var nested = FindNestedForceEntry(feParent, forceEntryId);
                    if (nested is not null) return nested;
                }
            }
        }
        throw new ArgumentException($"Force entry '{forceEntryId}' not found.", nameof(forceEntryId));
    }

    private static IForceEntrySymbol? FindNestedForceEntry(IForceEntrySymbol parent, string id)
    {
        foreach (var child in parent.ChildForces)
        {
            if (child.Id == id) return child;
            var nested = FindNestedForceEntry(child, id);
            if (nested is not null) return nested;
        }
        return null;
    }

    /// <summary>
    /// Resolves the catalogue for a force — first checks local tracking, then falls back
    /// to the ForceNode's CatalogueId.
    /// </summary>
    private ICatalogueSymbol ResolveCatalogueForForce(WhamRosterState state, string forceId)
    {
        if (_forceCatalogues.TryGetValue(forceId, out var catalogue))
            return catalogue;

        var roster = state.RosterRequired;
        var force = WhamRosterEngine.FindForceDeep(roster, forceId);
        if (force is not null && force.CatalogueId is not null)
        {
            foreach (var cat in state.Compilation.GlobalNamespace.Catalogues)
            {
                if (cat.Id == force.CatalogueId) return cat;
            }
        }
        return state.Compilation.GlobalNamespace.RootCatalogue;
    }

    /// <summary>
    /// Finds an available entry by matching the definition entry ID.
    /// </summary>
    private static AvailableEntry FindAvailableEntryById(IReadOnlyList<AvailableEntry> available, string entryId)
    {
        foreach (var avail in available)
        {
            if (avail.Symbol.Id == entryId) return avail;
        }
        foreach (var avail in available)
        {
            var resolved = avail.Symbol.ReferencedEntry ?? avail.Symbol;
            if (resolved.Id == entryId) return avail;
        }
        throw new ArgumentException(
            $"Entry '{entryId}' not found among {available.Count} available entries.",
            nameof(entryId));
    }

    /// <summary>
    /// Finds the ISelectionEntryContainerSymbol matching a selection node's entry ID.
    /// </summary>
    private static ISelectionEntryContainerSymbol? FindEntrySymbolForSelection(
        WhamRosterState state, string forceId, string selectionId)
    {
        var roster = state.RosterRequired;
        var force = WhamRosterEngine.FindForceDeep(roster, forceId);
        if (force is null) return null;
        var selNode = WhamRosterEngine.FindSelectionDeep(force, selectionId);
        if (selNode is null) return null;

        var entryId = selNode.EntryId;
        if (string.IsNullOrEmpty(entryId)) return null;

        var targetId = entryId;
        var separatorIndex = entryId.IndexOf(WhamRosterEngine.EntryLinkIdSeparator, StringComparison.Ordinal);
        if (separatorIndex >= 0)
            targetId = entryId[(separatorIndex + WhamRosterEngine.EntryLinkIdSeparator.Length)..];

        foreach (var cat in state.Compilation.GlobalNamespace.Catalogues)
        {
            var found = FindEntryById(cat.RootContainerEntries, targetId)
                     ?? FindEntryById(cat.SharedSelectionEntryContainers, targetId);
            if (found is not null) return found;
        }
        return null;
    }

    private static ISelectionEntryContainerSymbol? FindEntryById(
        ImmutableArray<IContainerEntrySymbol> entries, string id)
    {
        foreach (var entry in entries)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
            {
                if (sec.Id == id) return sec;
                var effective = sec.IsReference ? sec.ReferencedEntry ?? sec : sec;
                if (effective.Id == id) return effective;
                var found = FindEntryInChildren(effective, id);
                if (found is not null) return found;
            }
        }
        return null;
    }

    private static ISelectionEntryContainerSymbol? FindEntryById(
        ImmutableArray<ISelectionEntryContainerSymbol> entries, string id)
    {
        foreach (var entry in entries)
        {
            if (entry.Id == id) return entry;
            var effective = entry.IsReference ? entry.ReferencedEntry ?? entry : entry;
            if (effective.Id == id) return effective;
            var found = FindEntryInChildren(effective, id);
            if (found is not null) return found;
        }
        return null;
    }

    private static ISelectionEntryContainerSymbol? FindEntryInChildren(
        ISelectionEntryContainerSymbol parent, string id)
    {
        foreach (var child in parent.ChildSelectionEntries)
        {
            if (child.Id == id) return child;
            var effective = child.IsReference ? child.ReferencedEntry ?? child : child;
            if (effective.Id == id) return effective;
            var found = FindEntryInChildren(effective, id);
            if (found is not null) return found;
        }
        return null;
    }
}
