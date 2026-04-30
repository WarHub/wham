using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
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

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        var compilation = ProtocolConverter.CreateCompilation(gameSystem, catalogues);
        _catalogCompilation = compilation;
        _coreEngine = new WhamRosterEngine();
        _state = _coreEngine.CreateRoster(compilation);
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

        return new ActionOutputs { ForceId = result.ForceId };
    }

    public void RemoveForce(string forceId)
    {
        var engine = EnsureEngine();
        _state = engine.RemoveForceById(EnsureState(), forceId);
    }

    public ActionOutputs SelectEntry(string forceId, string entryId)
    {
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
        return new ActionOutputs { SelectionId = result.SelectionId };
    }

    public ActionOutputs DuplicateForce(string forceId)
    {
        var result = EnsureEngine().DuplicateForceById(EnsureState(), forceId);
        _state = result.State;
        var newForceId = result.ForceId!;
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

        var rosterSymbol = state.RosterSymbol
            ?? throw new InvalidOperationException("No roster symbol found in state.");

        // Compute per-force available entry counts and referenced cost types
        // using the symbol tree (no Node-layer access needed).
        var counts = new List<int>();
        var referencedCostTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var forceSymbol in rosterSymbol.Forces)
        {
            var catalogue = ResolveCatalogueForForce(state, forceSymbol.Id!);
            var entries = _resolver.GetAvailableEntries(catalogue);
            counts.Add(_resolver.GetRootEntryCount(catalogue));
            foreach (var entry in entries)
            {
                StateMapper.CollectReferencedCostTypes(entry.Symbol, referencedCostTypes);
            }
        }

        var mapper = new StateMapper(rosterSymbol, compilation);
        return mapper.MapRosterState(counts, referencedCostTypes);
    }

    public IReadOnlyList<ValidationErrorState> GetValidationErrors()
    {
        var state = GetRosterState();
        return state.ValidationErrors;
    }

    public void Dispose()
    {
        _coreEngine = null;
        _state = null;
        _catalogCompilation = null;
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
    /// Resolves the catalogue for a force via the force symbol's
    /// <see cref="IForceSymbol.CatalogueReference"/>.
    /// </summary>
    private static ICatalogueSymbol ResolveCatalogueForForce(WhamRosterState state, string forceId)
    {
        var rosterSymbol = state.RosterSymbol;
        if (rosterSymbol is not null)
        {
            var forceSymbol = FindForceSymbolDeep(rosterSymbol, forceId);
            if (forceSymbol is not null)
                return forceSymbol.CatalogueReference.Catalogue;
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
    /// Finds the <see cref="ISelectionEntryContainerSymbol"/> for a selection's source entry
    /// by traversing the roster's symbol tree. Uses the selection symbol's
    /// <see cref="ISelectionSymbol.SourceEntry"/> which already resolves entry links
    /// through the binder (no manual ID parsing needed).
    /// </summary>
    private static ISelectionEntryContainerSymbol? FindEntrySymbolForSelection(
        WhamRosterState state, string forceId, string selectionId)
    {
        var rosterSymbol = state.RosterSymbol;
        if (rosterSymbol is null) return null;

        var forceSymbol = FindForceSymbolDeep(rosterSymbol, forceId);
        if (forceSymbol is null) return null;

        var selectionSymbol = FindSelectionSymbolDeep(forceSymbol, selectionId);
        if (selectionSymbol is null) return null;

        return selectionSymbol.SourceEntry;
    }

    // ──────────────────────────────────────────────────────────────────
    //  Symbol-based tree traversal
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds a force symbol by instance ID anywhere in the roster's force tree
    /// (including nested child forces).
    /// </summary>
    private static IForceSymbol? FindForceSymbolDeep(IRosterSymbol roster, string forceId)
    {
        foreach (var force in roster.Forces)
        {
            var found = FindForceSymbolDeep(force, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    private static IForceSymbol? FindForceSymbolDeep(IForceSymbol force, string forceId)
    {
        if (force.Id == forceId) return force;
        foreach (var child in force.Forces)
        {
            var found = FindForceSymbolDeep(child, forceId);
            if (found is not null) return found;
        }
        return null;
    }

    /// <summary>
    /// Recursively searches for a selection symbol by ID within a force's selection tree
    /// (including selections in child forces).
    /// </summary>
    private static ISelectionSymbol? FindSelectionSymbolDeep(IForceSymbol force, string selectionId)
    {
        foreach (var sel in force.Selections)
        {
            var found = FindSelectionSymbolDeep(sel, selectionId);
            if (found is not null) return found;
        }
        foreach (var childForce in force.Forces)
        {
            var found = FindSelectionSymbolDeep(childForce, selectionId);
            if (found is not null) return found;
        }
        return null;
    }

    private static ISelectionSymbol? FindSelectionSymbolDeep(ISelectionSymbol sel, string selectionId)
    {
        if (sel.Id == selectionId) return sel;
        foreach (var child in sel.Selections)
        {
            var found = FindSelectionSymbolDeep(child, selectionId);
            if (found is not null) return found;
        }
        return null;
    }
}
