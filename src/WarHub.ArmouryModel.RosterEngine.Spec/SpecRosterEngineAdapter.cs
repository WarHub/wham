using BattleScribeSpec.Protocol;
using BattleScribeSpec.Roster;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.EditorServices;
using ProtocolRosterState = BattleScribeSpec.Roster.RosterState;
using WhamRosterState = WarHub.ArmouryModel.EditorServices.RosterState;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Adapts the ISymbol-based <see cref="WhamRosterEngine"/> to the BattleScribeSpec
/// <see cref="IRosterEngine"/> interface. Thin translation layer:
/// <list type="bullet">
///   <item>Protocol ID → engine's ID-based API</item>
///   <item>ISymbol roster tree → Protocol state mapping via <see cref="StateMapper"/></item>
/// </list>
/// </summary>
public sealed class SpecRosterEngineAdapter : IRosterEngine
{
    private WhamRosterEngine? _coreEngine;
    private WhamRosterState? _state;
    private WhamCompilation? _catalogCompilation;

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
        _state = EnsureEngine().RemoveForceById(EnsureState(), forceId);
    }

    public ActionOutputs SelectEntry(string forceId, string entryId)
    {
        var result = EnsureEngine().SelectEntryById(EnsureState(), forceId, entryId);
        _state = result.State;

        return new ActionOutputs
        {
            SelectionId = result.SelectionId,
            Selections = result.Selections
        };
    }

    public ActionOutputs SelectChildEntry(string forceId, string parentSelectionId, string entryId)
    {
        var result = EnsureEngine().SelectChildEntryById(
            EnsureState(), forceId, parentSelectionId, entryId);
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
        return new ActionOutputs { ForceId = result.ForceId! };
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

        var mapper = new StateMapper(rosterSymbol, compilation);
        return mapper.MapRosterState();
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

    private static ICatalogueSymbol ResolveCatalogueById(WhamCompilation compilation, string catalogueId)
    {
        foreach (var cat in compilation.GlobalNamespace.Catalogues)
        {
            if (cat.Id == catalogueId) return cat;
        }
        throw new ArgumentException($"Catalogue '{catalogueId}' not found.", nameof(catalogueId));
    }

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
}
