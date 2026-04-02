using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using ProtocolRosterState = BattleScribeSpec.RosterState;
using WhamRosterState = WarHub.ArmouryModel.EditorServices.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Adapts the ISymbol-based <see cref="WhamRosterEngine"/> to the BattleScribeSpec
/// <see cref="IRosterEngine"/> interface. Handles:
/// <list type="bullet">
///   <item>Protocol → SourceNode conversion via <see cref="ProtocolConverter"/></item>
///   <item>Index → ISymbol mapping for <see cref="IRosterEngine"/> index-based API</item>
///   <item>ISymbol roster tree → Protocol state mapping via <see cref="StateMapper"/></item>
/// </list>
/// </summary>
public sealed class SpecRosterEngineAdapter : IRosterEngine
{
    private WhamRosterEngine? _coreEngine;
    private WhamRosterState? _state;
    private WhamCompilation? _catalogCompilation;
    private readonly EntryResolver _resolver = new();

    // Catalogue mapping: maps catalogue index (from Setup) to ICatalogueSymbol
    private readonly List<ICatalogueSymbol> _catalogueSymbols = [];

    // Force-to-catalogue mapping: tracks which catalogue each force was added with.
    // Parallel to RosterNode.Forces — index i corresponds to force i.
    private readonly List<ICatalogueSymbol> _forceCatalogues = [];

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        var compilation = ProtocolConverter.CreateCompilation(gameSystem, catalogues);
        _catalogCompilation = compilation;
        _coreEngine = new WhamRosterEngine();
        _state = _coreEngine.CreateRoster(compilation);

        // Build catalogue index: first entry is always the gamesystem
        _catalogueSymbols.Clear();
        var ns = compilation.GlobalNamespace;
        foreach (var cat in ns.Catalogues)
        {
            if (!cat.IsGamesystem)
            {
                _catalogueSymbols.Add(cat);
            }
        }

        return [];
    }

    public void AddForce(int forceEntryIndex, int catalogueIndex = 0)
    {
        var engine = EnsureEngine();
        var state = EnsureState();
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;

        // Resolve catalogue: catalogueIndex maps to non-gamesystem catalogues
        var catalogue = catalogueIndex < _catalogueSymbols.Count
            ? _catalogueSymbols[catalogueIndex]
            : gsSym;

        // Find force entry by index: force entries are in RootContainerEntries
        var forceEntries = gsSym.RootContainerEntries
            .OfType<IForceEntrySymbol>()
            .Concat(catalogue.IsGamesystem
                ? Enumerable.Empty<IForceEntrySymbol>()
                : catalogue.RootContainerEntries.OfType<IForceEntrySymbol>())
            .ToList();

        if (forceEntryIndex < 0 || forceEntryIndex >= forceEntries.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(forceEntryIndex),
                $"Force entry index {forceEntryIndex} out of range (total={forceEntries.Count})");
        }

        var forceEntry = forceEntries[forceEntryIndex];
        _state = engine.AddForce(state, forceEntry, catalogue);
        _forceCatalogues.Add(catalogue);

        // Auto-select root entries with min constraints
        AutoSelectRootEntries(_state.RosterRequired.Forces.Count - 1);
    }

    public void RemoveForce(int forceIndex)
    {
        _state = EnsureEngine().RemoveForce(EnsureState(), forceIndex);
        _forceCatalogues.RemoveAt(forceIndex);
    }

    public void SelectEntry(int forceIndex, int entryIndex)
    {
        var engine = EnsureEngine();
        var state = EnsureState();

        // Use tracked catalogue for this force (set during AddForce)
        var catalogue = _forceCatalogues[forceIndex];

        var available = _resolver.GetAvailableEntries(catalogue);
        if (entryIndex < 0 || entryIndex >= available.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(entryIndex),
                $"Entry index {entryIndex} out of range (available={available.Count})");
        }

        var avail = available[entryIndex];
        _state = engine.SelectEntry(state, forceIndex, avail.Symbol, avail.SourceGroup);
    }

    public void SelectChildEntry(int forceIndex, int selectionIndex, int childEntryIndex)
    {
        var engine = EnsureEngine();
        var state = EnsureState();

        // Find the parent selection's entry symbol to get child entries
        var force = state.RosterRequired.Forces[forceIndex];
        var parentSel = force.Selections[selectionIndex];
        var parentEntry = FindEntrySymbolForSelection(parentSel);

        if (parentEntry is null)
        {
            throw new InvalidOperationException(
                $"Could not find entry symbol for selection '{parentSel.Name}' (entryId={parentSel.EntryId})");
        }

        var childEntries = _resolver.GetChildEntries(parentEntry);
        if (childEntryIndex < 0 || childEntryIndex >= childEntries.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(childEntryIndex),
                $"Child entry index {childEntryIndex} out of range (available={childEntries.Count})");
        }

        var childAvail = childEntries[childEntryIndex];
        _state = engine.SelectChildEntry(state, forceIndex, selectionIndex, childAvail.Symbol, childAvail.SourceGroup);
    }

    public void DeselectSelection(int forceIndex, int selectionIndex)
    {
        _state = EnsureEngine().DeselectSelection(EnsureState(), forceIndex, selectionIndex);
    }

    public void SetSelectionCount(int forceIndex, int entryIndex, int count)
    {
        // No-op for root/force-level entries.
        // Root entries create new selections via selectEntry, not via count.
    }

    public void DuplicateSelection(int forceIndex, int selectionIndex)
    {
        _state = EnsureEngine().DuplicateSelection(EnsureState(), forceIndex, selectionIndex);
    }

    public void SetCostLimit(string costTypeId, double value)
    {
        _state = EnsureEngine().SetCostLimit(EnsureState(), costTypeId, (decimal)value);
    }

    public ProtocolRosterState GetRosterState()
    {
        var state = EnsureState();
        var mapper = new StateMapper(state.Compilation, state.RosterRequired, _forceCatalogues);
        return mapper.MapRosterState(state.RosterRequired);
    }

    public IReadOnlyList<ValidationErrorState> GetValidationErrors()
    {
        // TODO Phase 5: constraint validation
        return [];
    }

    public void Dispose()
    {
        _coreEngine = null;
        _state = null;
        _catalogCompilation = null;
        _catalogueSymbols.Clear();
        _forceCatalogues.Clear();
    }

    private WhamRosterEngine EnsureEngine()
        => _coreEngine ?? throw new InvalidOperationException("Engine not set up. Call Setup first.");

    private WhamRosterState EnsureState()
        => _state ?? throw new InvalidOperationException("Engine not set up. Call Setup first.");

    /// <summary>
    /// Auto-selects root entries that have min constraints on force/parent scope.
    /// Called after AddForce to mirror legacy behavior.
    /// </summary>
    private void AutoSelectRootEntries(int forceIndex)
    {
        var engine = EnsureEngine();
        var state = EnsureState();

        // Use tracked catalogue for this force
        var catalogue = _forceCatalogues[forceIndex];
        var available = _resolver.GetAvailableEntries(catalogue);

        foreach (var avail in available)
        {
            var effectiveEntry = avail.Symbol.IsReference
                ? avail.Symbol.ReferencedEntry ?? avail.Symbol
                : avail.Symbol;

            var minCount = GetMinConstraintAutoSelect(effectiveEntry);
            if (minCount < 1) continue;

            for (int i = 0; i < minCount; i++)
            {
                state = engine.SelectEntry(state, forceIndex, avail.Symbol, avail.SourceGroup);
            }
        }

        _state = state;
    }

    /// <summary>
    /// Checks for min constraint on selections scope=parent/force for auto-selection.
    /// </summary>
    private static int GetMinConstraintAutoSelect(ISelectionEntryContainerSymbol entry)
    {
        foreach (var constraint in entry.Constraints)
        {
            var decl = constraint.GetDeclaration();
            if (decl is null) continue;
            if (decl.Type != Source.ConstraintKind.Minimum) continue;
            if (decl.Field is not "selections") continue;
            if (decl.Scope is not ("parent" or "force")) continue;
            if (decl.IsValuePercentage) continue;

            var value = (int)decl.Value;
            if (value >= 1) return value;
        }

        return 0;
    }

    /// <summary>
    /// Finds the ISelectionEntryContainerSymbol matching a selection node's entry ID.
    /// Searches all catalogues in the compilation.
    /// </summary>
    private ISelectionEntryContainerSymbol? FindEntrySymbolForSelection(Source.SelectionNode selNode)
    {
        var entryId = selNode.EntryId;
        if (string.IsNullOrEmpty(entryId)) return null;

        var compilation = _state?.Compilation;
        if (compilation is null) return null;

        // Search through all catalogues
        foreach (var cat in compilation.GlobalNamespace.Catalogues)
        {
            var found = FindEntryById(cat.RootContainerEntries, entryId)
                     ?? FindEntryById(cat.SharedSelectionEntryContainers, entryId);
            if (found is not null) return found;
        }

        return null;
    }

    /// <summary>
    /// Recursively searches for an entry by ID in a collection of container entries.
    /// </summary>
    private static ISelectionEntryContainerSymbol? FindEntryById(
        ImmutableArray<IContainerEntrySymbol> entries, string id)
    {
        foreach (var entry in entries)
        {
            if (entry is ISelectionEntryContainerSymbol sec)
            {
                var effective = sec.IsReference ? sec.ReferencedEntry ?? sec : sec;
                if (effective.Id == id) return effective;

                // Search children
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
            var effective = child.IsReference ? child.ReferencedEntry ?? child : child;
            if (effective.Id == id) return effective;

            var found = FindEntryInChildren(effective, id);
            if (found is not null) return found;
        }
        return null;
    }
}
