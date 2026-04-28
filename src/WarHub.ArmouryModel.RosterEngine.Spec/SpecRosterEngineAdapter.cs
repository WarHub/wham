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
/// <see cref="IRosterEngine"/> interface. Handles:
/// <list type="bullet">
///   <item>Protocol → SourceNode conversion via <see cref="ProtocolConverter"/></item>
///   <item>ID-based addressing → index-based core engine calls (top-level) or tree manipulation (nested)</item>
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

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        var compilation = ProtocolConverter.CreateCompilation(gameSystem, catalogues);
        _catalogCompilation = compilation;
        _coreEngine = new WhamRosterEngine();
        _state = _coreEngine.CreateRoster(compilation);
        _forceCatalogues.Clear();
        return [];
    }

    public ActionOutputs AddForce(string forceEntryId, string catalogueId)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var compilation = (WhamCompilation)state.Compilation;

        var catalogue = ResolveCatalogueById(compilation, catalogueId);
        var forceEntry = ResolveForceEntryById(compilation, forceEntryId);

        // Collect existing selection IDs before the operation
        var existingIds = CollectAllSelectionIds(state.RosterRequired);

        _state = engine.AddForce(state, forceEntry, catalogue);
        var roster = _state.RosterRequired;
        var newForce = roster.Forces[^1];
        var forceId = newForce.Id!;

        _forceCatalogues[forceId] = catalogue;

        // Auto-select root entries with min constraints
        var forceIndex = roster.Forces.Count - 1;
        AutoSelectRootEntries(forceIndex);

        // Build selections map from auto-selected entries
        var outputs = new ActionOutputs { ForceId = forceId };
        outputs.Selections = CollectNewSelections(_state.RosterRequired, existingIds, null);
        return outputs;
    }

    public ActionOutputs AddChildForce(string parentForceId, string forceEntryId, string catalogueId)
    {
        var state = EnsureState();
        var compilation = (WhamCompilation)state.Compilation;
        var roster = state.RosterRequired;

        var catalogue = ResolveCatalogueById(compilation, catalogueId);
        var forceEntry = ResolveForceEntryById(compilation, forceEntryId);
        var forceEntryDecl = forceEntry.GetDeclaration()
            ?? throw new InvalidOperationException($"Force entry '{forceEntryId}' has no declaration.");

        var catalogueDecl = catalogue.GetDeclaration() as CatalogueBaseNode;
        var newChildForce = NodeFactory.Force(forceEntryDecl, catalogueDecl);

        // Resolve categories for the child force
        var categories = BuildForceCategories(forceEntry);
        if (categories.Length > 0)
        {
            newChildForce = newChildForce.AddCategories(categories);
        }

        // Find parent force anywhere in the nested force tree
        var parentForce = FindForceNodeDeep(roster, parentForceId)
            ?? throw new ArgumentException($"Force '{parentForceId}' not found in roster.", nameof(parentForceId));
        var updatedParent = parentForce.AddForces(newChildForce);
        var newRoster = roster.Replace(parentForce, _ => updatedParent).WithUpdatedCostTotals();
        _state = state.ReplaceRoster(newRoster);

        var childForceId = newChildForce.Id!;
        _forceCatalogues[childForceId] = catalogue;

        return new ActionOutputs { ForceId = childForceId };
    }

    public void RemoveForce(string forceId)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Try top-level first (core engine uses index-based removal)
        for (var i = 0; i < roster.Forces.Count; i++)
        {
            if (roster.Forces[i].Id == forceId)
            {
                _state = EnsureEngine().RemoveForce(state, i);
                _forceCatalogues.Remove(forceId);
                return;
            }
        }

        // Nested force: find and remove from parent via tree manipulation
        var forceNode = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found in roster.", nameof(forceId));
        var newRoster = roster.Remove(forceNode).WithUpdatedCostTotals();
        _state = state.ReplaceRoster(newRoster);
        _forceCatalogues.Remove(forceId);
    }

    public ActionOutputs SelectEntry(string forceId, string entryId)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Try top-level force (can use core engine's index-based API)
        int? topLevelIndex = FindTopLevelForceIndex(roster, forceId);

        var catalogue = _forceCatalogues.GetValueOrDefault(forceId);
        if (catalogue is null)
        {
            var forceNode = topLevelIndex.HasValue
                ? roster.Forces[topLevelIndex.Value]
                : FindForceNodeDeep(roster, forceId)
                    ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));
            catalogue = ResolveForceCatalogue(state, forceNode);
        }

        var available = _resolver.GetAvailableEntries(catalogue);
        var avail = FindAvailableEntryById(available, entryId);

        // Collect existing selection IDs before the operation
        var existingIds = CollectAllSelectionIds(roster);

        if (topLevelIndex.HasValue)
        {
            // Top-level force: use core engine
            _state = engine.SelectEntry(state, topLevelIndex.Value, avail.Symbol, avail.SourceGroup);
        }
        else
        {
            // Nested force: manipulate tree directly
            var forceNode = FindForceNodeDeep(roster, forceId)!;
            var selectionNode = engine.CreateSelectionFromEntry(avail.Symbol, avail.SourceGroup);
            var newForce = forceNode.AddSelections(selectionNode);
            var newRoster = roster.Replace(forceNode, _ => newForce).WithUpdatedCostTotals();
            _state = state.ReplaceRoster(newRoster);
        }

        // Find the new primary selection
        var newRosterAfter = _state.RosterRequired;
        var targetForce = FindForceNodeDeep(newRosterAfter, forceId)!;
        var primarySel = targetForce.Selections[^1];

        var outputs = new ActionOutputs { SelectionId = primarySel.Id };
        outputs.Selections = CollectNewSelections(newRosterAfter, existingIds, primarySel.Id);
        return outputs;
    }

    public ActionOutputs SelectChildEntry(string forceId, string parentSelectionId, string entryId)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Find force (may be nested)
        var force = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));
        int? topLevelIndex = FindTopLevelForceIndex(roster, forceId);

        // Find parent selection (may be nested within other selections)
        var parentSelNode = FindSelectionNodeDeep(force, parentSelectionId)
            ?? throw new ArgumentException($"Selection '{parentSelectionId}' not found in force '{forceId}'.", nameof(parentSelectionId));
        var parentEntry = FindEntrySymbolForSelection(parentSelNode);

        if (parentEntry is null)
        {
            throw new InvalidOperationException(
                $"Could not find entry symbol for selection '{parentSelNode.Name}' (entryId={parentSelNode.EntryId})");
        }

        var childEntries = _resolver.GetChildEntries(parentEntry);
        var childAvail = FindAvailableEntryById(childEntries, entryId);

        // Collect existing selection IDs before the operation
        var existingIds = CollectAllSelectionIds(roster);

        // Check if parent selection is a direct child of a top-level force (can use core engine)
        int selectionIndex = -1;
        if (topLevelIndex.HasValue)
        {
            for (var i = 0; i < force.Selections.Count; i++)
            {
                if (force.Selections[i].Id == parentSelectionId)
                {
                    selectionIndex = i;
                    break;
                }
            }
        }

        if (selectionIndex >= 0)
        {
            _state = engine.SelectChildEntry(state, topLevelIndex!.Value, selectionIndex, childAvail.Symbol, childAvail.SourceGroup);
        }
        else
        {
            // Nested parent: use tree manipulation
            var childSelectionNode = engine.CreateSelectionFromEntry(childAvail.Symbol, childAvail.SourceGroup);
            var newParent = parentSelNode.AddSelections(childSelectionNode);
            var newRoster = roster.Replace(parentSelNode, _ => newParent).WithUpdatedCostTotals();
            _state = state.ReplaceRoster(newRoster);
        }

        // Find the new child selection
        var newRosterAfter = _state.RosterRequired;
        var updatedForce = FindForceNodeDeep(newRosterAfter, forceId)!;
        var updatedParent2 = FindSelectionNodeDeep(updatedForce, parentSelectionId)!;
        var primaryChild = updatedParent2.Selections[^1];

        var outputs = new ActionOutputs { SelectionId = primaryChild.Id };
        outputs.Selections = CollectNewSelections(newRosterAfter, existingIds, primaryChild.Id);
        return outputs;
    }

    public void DeselectSelection(string forceId, string selectionId)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Try top-level force with core engine
        int? topLevelIndex = FindTopLevelForceIndex(roster, forceId);
        if (topLevelIndex.HasValue)
        {
            var force = roster.Forces[topLevelIndex.Value];
            // Check if selection is a direct child of the force
            for (var i = 0; i < force.Selections.Count; i++)
            {
                if (force.Selections[i].Id == selectionId)
                {
                    _state = EnsureEngine().DeselectSelection(state, topLevelIndex.Value, i);
                    return;
                }
            }
        }

        // Selection in nested force, or nested selection: remove via tree manipulation
        var forceNode = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));
        var selNode = FindSelectionNodeDeep(forceNode, selectionId)
            ?? throw new ArgumentException($"Selection '{selectionId}' not found in force '{forceId}'.", nameof(selectionId));
        var newRoster = roster.Remove(selNode).WithUpdatedCostTotals();
        _state = state.ReplaceRoster(newRoster);
    }

    public void SetSelectionCount(string forceId, string selectionId, int count)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;

        var force = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));
        var selNode = FindSelectionNodeDeep(force, selectionId)
            ?? throw new InvalidOperationException($"Selection '{selectionId}' not found in force '{forceId}'.");

        var newSelNode = selNode.WithNumber(count);
        var newRoster = roster.Replace(selNode, _ => newSelNode).WithUpdatedCostTotals();
        _state = state.ReplaceRoster(newRoster);
    }

    public ActionOutputs DuplicateSelection(string forceId, string selectionId)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Try top-level force with core engine
        int? topLevelIndex = FindTopLevelForceIndex(roster, forceId);
        if (topLevelIndex.HasValue)
        {
            var force = roster.Forces[topLevelIndex.Value];
            for (var i = 0; i < force.Selections.Count; i++)
            {
                if (force.Selections[i].Id == selectionId)
                {
                    _state = EnsureEngine().DuplicateSelection(state, topLevelIndex.Value, i);
                    var newForce = _state.RosterRequired.Forces[topLevelIndex.Value];
                    return new ActionOutputs { SelectionId = newForce.Selections[^1].Id };
                }
            }
        }

        // Nested: duplicate via tree manipulation
        var forceNode = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));
        var selNode = FindSelectionNodeDeep(forceNode, selectionId)
            ?? throw new ArgumentException($"Selection '{selectionId}' not found in force '{forceId}'.", nameof(selectionId));

        // Find parent and add duplicate
        var newForceNode = forceNode.AddSelections(selNode);
        var newRoster = roster.Replace(forceNode, _ => newForceNode).WithUpdatedCostTotals();
        _state = state.ReplaceRoster(newRoster);

        var updatedForce = FindForceNodeDeep(_state.RosterRequired, forceId)!;
        return new ActionOutputs { SelectionId = updatedForce.Selections[^1].Id };
    }

    public ActionOutputs DuplicateForce(string forceId)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;

        // Try top-level force
        for (var i = 0; i < roster.Forces.Count; i++)
        {
            if (roster.Forces[i].Id == forceId)
            {
                var newRoster = roster.AddForces(roster.Forces[i]).WithUpdatedCostTotals();
                _state = state.ReplaceRoster(newRoster);
                var duplicatedForce = _state.RosterRequired.Forces[^1];
                var newForceId = duplicatedForce.Id!;
                if (_forceCatalogues.TryGetValue(forceId, out var catalogue))
                    _forceCatalogues[newForceId] = catalogue;
                return new ActionOutputs { ForceId = newForceId };
            }
        }

        throw new ArgumentException($"Force '{forceId}' not found at top level for duplication.", nameof(forceId));
    }

    public void SetCostLimit(string costTypeId, double value)
    {
        _state = EnsureEngine().SetCostLimit(EnsureState(), costTypeId, (decimal)value);
    }

    public void SetCustomization(string forceId, string? selectionId, string? categoryEntryId,
        string? customName, string? customNotes)
    {
        var state = EnsureState();
        var roster = state.RosterRequired;
        var force = FindForceNodeDeep(roster, forceId)
            ?? throw new ArgumentException($"Force '{forceId}' not found.", nameof(forceId));

        RosterNode newRoster;
        if (categoryEntryId is not null)
        {
            // Target is a category in the force or selection
            if (selectionId is not null)
            {
                var selNode = FindSelectionNodeDeep(force, selectionId)
                    ?? throw new InvalidOperationException($"Selection '{selectionId}' not found.");
                var catNode = selNode.Categories.FirstOrDefault(c => c.EntryId == categoryEntryId)
                    ?? throw new InvalidOperationException($"Category '{categoryEntryId}' not found.");
                var newCat = catNode;
                if (customNotes is not null) newCat = newCat.WithCustomNotes(customNotes);
                newRoster = roster.Replace(catNode, _ => newCat);
            }
            else
            {
                var catNode = force.Categories.FirstOrDefault(c => c.EntryId == categoryEntryId)
                    ?? throw new InvalidOperationException($"Category '{categoryEntryId}' not found.");
                var newCat = catNode;
                if (customNotes is not null) newCat = newCat.WithCustomNotes(customNotes);
                newRoster = roster.Replace(catNode, _ => newCat);
            }
        }
        else if (selectionId is not null)
        {
            // Target is a selection
            var selNode = FindSelectionNodeDeep(force, selectionId)
                ?? throw new InvalidOperationException($"Selection '{selectionId}' not found.");
            var newSel = selNode;
            if (customName is not null) newSel = newSel.WithCustomName(customName);
            if (customNotes is not null) newSel = newSel.WithCustomNotes(customNotes);
            newRoster = roster.Replace(selNode, _ => newSel);
        }
        else
        {
            // Target is the force
            var newForce = force;
            if (customName is not null) newForce = newForce.WithCustomName(customName);
            if (customNotes is not null) newForce = newForce.WithCustomNotes(customNotes);
            newRoster = roster.Replace(force, _ => newForce);
        }

        _state = state.ReplaceRoster(newRoster);
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
            var catalogue = _forceCatalogues.GetValueOrDefault(force.Id!)
                ?? ResolveForceCatalogue(state, force);
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
        return GetRosterState().ValidationErrors;
    }

    public void Dispose()
    {
        _coreEngine = null;
        _state = null;
        _catalogCompilation = null;
        _forceCatalogues.Clear();
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
                // Search nested force entries
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
    /// Resolves the catalogue for a force using the ForceNode's CatalogueId.
    /// </summary>
    private static ICatalogueSymbol ResolveForceCatalogue(WhamRosterState state, ForceNode force)
    {
        var catId = force.CatalogueId;
        if (catId is not null)
        {
            foreach (var cat in state.Compilation.GlobalNamespace.Catalogues)
            {
                if (cat.Id == catId) return cat;
            }
        }
        return state.Compilation.GlobalNamespace.RootCatalogue;
    }

    /// <summary>
    /// Returns the top-level force index if the force is at the roster root, or null if nested.
    /// </summary>
    private static int? FindTopLevelForceIndex(RosterNode roster, string forceId)
    {
        for (var i = 0; i < roster.Forces.Count; i++)
        {
            if (roster.Forces[i].Id == forceId) return i;
        }
        return null;
    }

    /// <summary>
    /// Finds a force node by its instance ID anywhere in the force tree (including nested child forces).
    /// </summary>
    private static ForceNode? FindForceNodeDeep(RosterNode roster, string forceId)
    {
        foreach (var force in roster.Forces)
        {
            var found = FindForceNodeDeep(force, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    private static ForceNode? FindForceNodeDeep(ForceNode force, string forceId)
    {
        if (force.Id == forceId) return force;
        foreach (var child in force.Forces)
        {
            var found = FindForceNodeDeep(child, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>
    /// Finds a top-level selection by its instance ID in a force.
    /// </summary>
    private static (SelectionNode Selection, int Index) FindTopLevelSelectionById(ForceNode force, string selectionId)
    {
        for (var i = 0; i < force.Selections.Count; i++)
        {
            if (force.Selections[i].Id == selectionId)
                return (force.Selections[i], i);
        }
        throw new ArgumentException($"Selection '{selectionId}' not found in force '{force.Id}'.", nameof(selectionId));
    }

    /// <summary>
    /// Recursively searches for a selection node by ID within a force's selection tree.
    /// </summary>
    private static SelectionNode? FindSelectionNodeDeep(ForceNode force, string selectionId)
    {
        foreach (var sel in force.Selections)
        {
            var found = FindSelectionNodeDeep(sel, selectionId);
            if (found is not null) return found;
        }
        // Also search child forces
        foreach (var childForce in force.Forces)
        {
            var found = FindSelectionNodeDeep(childForce, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    private static SelectionNode? FindSelectionNodeDeep(SelectionNode sel, string selectionId)
    {
        if (sel.Id == selectionId) return sel;
        foreach (var child in sel.Selections)
        {
            var found = FindSelectionNodeDeep(child, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>
    /// Finds an available entry by matching the definition entry ID.
    /// Checks both the direct symbol ID and the resolved (referenced) entry ID.
    /// </summary>
    private static AvailableEntry FindAvailableEntryById(IReadOnlyList<AvailableEntry> available, string entryId)
    {
        // First pass: match by symbol ID (link ID or direct entry ID)
        foreach (var avail in available)
        {
            if (avail.Symbol.Id == entryId) return avail;
        }
        // Second pass: match by resolved target ID
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
    /// Collects all selection IDs currently present in the roster (including nested forces).
    /// </summary>
    private static HashSet<string> CollectAllSelectionIds(RosterNode roster)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var force in roster.Forces)
        {
            CollectAllSelectionIdsFromForce(force, ids);
        }
        return ids;
    }

    private static void CollectAllSelectionIdsFromForce(ForceNode force, HashSet<string> ids)
    {
        CollectSelectionIds(force.Selections, ids);
        foreach (var childForce in force.Forces)
        {
            CollectAllSelectionIdsFromForce(childForce, ids);
        }
    }

    private static void CollectSelectionIds(IEnumerable<SelectionNode> selections, HashSet<string> ids)
    {
        foreach (var sel in selections)
        {
            if (sel.Id is not null) ids.Add(sel.Id);
            CollectSelectionIds(sel.Selections, ids);
        }
    }

    /// <summary>
    /// Builds the Selections map (entryId → selectionId) for new selections
    /// that appeared after a mutation. Uses the SelectionNode.EntryId as key
    /// (which matches the definition entry ID used in spec expressions).
    /// </summary>
    private static Dictionary<string, string>? CollectNewSelections(
        RosterNode newRoster, HashSet<string> existingIds, string? primaryId)
    {
        Dictionary<string, string>? map = null;
        foreach (var force in newRoster.Forces)
        {
            CollectNewSelectionsFromForce(force, existingIds, primaryId, ref map);
        }
        return map;
    }

    private static void CollectNewSelectionsFromForce(
        ForceNode force,
        HashSet<string> existingIds,
        string? primaryId,
        ref Dictionary<string, string>? map)
    {
        CollectNewSelectionsFromTree(force.Selections, existingIds, primaryId, ref map);
        foreach (var childForce in force.Forces)
        {
            CollectNewSelectionsFromForce(childForce, existingIds, primaryId, ref map);
        }
    }

    private static void CollectNewSelectionsFromTree(
        IEnumerable<SelectionNode> selections,
        HashSet<string> existingIds,
        string? primaryId,
        ref Dictionary<string, string>? map)
    {
        foreach (var sel in selections)
        {
            if (sel.Id is not null && sel.Id != primaryId && !existingIds.Contains(sel.Id))
            {
                var key = GetSelectionMapKey(sel);
                if (key is not null)
                {
                    map ??= new(StringComparer.Ordinal);
                    map.TryAdd(key, sel.Id);
                }
            }
            CollectNewSelectionsFromTree(sel.Selections, existingIds, primaryId, ref map);
        }
    }

    /// <summary>
    /// Gets the key for the selections map from a SelectionNode.
    /// For "linkId::targetId" format entryIds, returns the targetId.
    /// Otherwise returns the entryId directly.
    /// </summary>
    private static string? GetSelectionMapKey(SelectionNode sel)
    {
        var entryId = sel.EntryId;
        if (string.IsNullOrEmpty(entryId)) return null;
        // "linkId::targetId" → use targetId as key
        var separatorIndex = entryId.IndexOf("::", StringComparison.Ordinal);
        return separatorIndex >= 0 ? entryId[(separatorIndex + 2)..] : entryId;
    }

    /// <summary>
    /// Auto-selects root entries that have min constraints on force/parent scope.
    /// </summary>
    private void AutoSelectRootEntries(int forceIndex)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var force = state.RosterRequired.Forces[forceIndex];

        var catalogue = _forceCatalogues.GetValueOrDefault(force.Id!)
            ?? ResolveForceCatalogue(state, force);
        var available = _resolver.GetAvailableEntries(catalogue);

        var autoSelectedGroups = new HashSet<string>(StringComparer.Ordinal);

        foreach (var avail in available)
        {
            var effectiveEntry = avail.Symbol.IsReference
                ? avail.Symbol.ReferencedEntry ?? avail.Symbol
                : avail.Symbol;

            var minCount = GetMinConstraintAutoSelect(effectiveEntry);

            // Also check the source group's constraints
            if (minCount < 1 && avail.SourceGroup is { } group)
            {
                var groupId = group.Id ?? "";
                if (!autoSelectedGroups.Contains(groupId))
                {
                    var groupMin = GetGroupMinConstraintAutoSelect(group);
                    if (groupMin >= 1 && IsDefaultEntryForGroup(avail, available, group))
                    {
                        minCount = groupMin;
                        autoSelectedGroups.Add(groupId);
                    }
                }
            }

            if (minCount < 1) continue;

            for (int i = 0; i < minCount; i++)
            {
                state = engine.SelectEntry(state, forceIndex, avail.Symbol, avail.SourceGroup);
            }
        }

        _state = state;
    }

    private static int GetGroupMinConstraintAutoSelect(ISelectionEntryGroupSymbol group)
    {
        foreach (var constraint in group.Constraints)
        {
            var decl = constraint.GetDeclaration();
            if (decl is null) continue;
            if (decl.Type != ConstraintKind.Minimum) continue;
            if (decl.Field is not "selections") continue;
            if (decl.Scope is not ("parent" or "force")) continue;
            if (decl.IsValuePercentage) continue;
            var value = (int)decl.Value;
            if (value >= 1) return value;
        }
        return 0;
    }

    private static bool IsDefaultEntryForGroup(
        AvailableEntry avail,
        IReadOnlyList<AvailableEntry> allEntries,
        ISelectionEntryGroupSymbol group)
    {
        // Check explicit default
        var defaultEntry = group.DefaultSelectionEntry;
        if (defaultEntry is not null)
        {
            var entryId = avail.Symbol.Id;
            var resolvedId = avail.Symbol.ReferencedEntry?.Id;
            var defaultId = defaultEntry.Id;
            var defaultResolvedId = defaultEntry.ReferencedEntry?.Id;

            if (entryId == defaultId || entryId == defaultResolvedId ||
                resolvedId == defaultId || resolvedId == defaultResolvedId)
                return true;
            return false;
        }

        // If no explicit default, check if this is the only entry from this group
        var count = 0;
        foreach (var entry in allEntries)
        {
            if (entry.SourceGroup == group) count++;
            if (count > 1) return false;
        }
        return count == 1;
    }

    private static int GetMinConstraintAutoSelect(ISelectionEntryContainerSymbol entry)
    {
        foreach (var constraint in entry.Constraints)
        {
            var decl = constraint.GetDeclaration();
            if (decl is null) continue;
            if (decl.Type != ConstraintKind.Minimum) continue;
            if (decl.Field is not "selections") continue;
            if (decl.Scope is not ("parent" or "force")) continue;
            if (decl.IsValuePercentage) continue;

            var value = (int)decl.Value;
            if (value >= 1) return value;
        }
        return 0;
    }

    /// <summary>
    /// Builds category nodes for a force entry (mirrors WhamRosterEngine.BuildForceCategories).
    /// </summary>
    private static CategoryNode[] BuildForceCategories(IForceEntrySymbol forceEntry)
    {
        var categories = forceEntry.Categories;
        if (categories.IsEmpty) return [];

        var list = new List<CategoryNode>();
        foreach (var catSym in categories)
        {
            var targetEntry = catSym.ReferencedEntry ?? catSym;
            var catEntryDecl = targetEntry.GetEntryDeclaration();
            if (catEntryDecl is null) continue;

            list.Add(
                NodeFactory.Category(catEntryDecl)
                    .WithPrimary(catSym.IsPrimaryCategory));
        }
        return [.. list];
    }

    /// <summary>
    /// Finds the ISelectionEntryContainerSymbol matching a selection node's entry ID.
    /// </summary>
    private ISelectionEntryContainerSymbol? FindEntrySymbolForSelection(SelectionNode selNode)
    {
        var entryId = selNode.EntryId;
        if (string.IsNullOrEmpty(entryId)) return null;

        // Handle "linkId::targetId" format by using targetId
        var targetId = entryId;
        var separatorIndex = entryId.IndexOf("::", StringComparison.Ordinal);
        if (separatorIndex >= 0)
            targetId = entryId[(separatorIndex + 2)..];

        var compilation = _state?.Compilation;
        if (compilation is null) return null;

        foreach (var cat in compilation.GlobalNamespace.Catalogues)
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
