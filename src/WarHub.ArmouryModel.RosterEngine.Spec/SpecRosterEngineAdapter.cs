using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using LegacyEngine = WarHub.ArmouryModel.RosterEngine.Spec.Legacy.WhamRosterEngine;

namespace WarHub.ArmouryModel.RosterEngine.Spec;

/// <summary>
/// Adapts the roster engine to the BattleScribeSpec <see cref="IRosterEngine"/> interface.
/// Currently delegates to the legacy Protocol-based engine.
/// Will be switched to the new ISymbol-based engine once it reaches conformance.
/// </summary>
public sealed class SpecRosterEngineAdapter : IRosterEngine
{
    private LegacyEngine? _engine;

    public IReadOnlyList<string> Setup(ProtocolGameSystem gameSystem, ProtocolCatalogue[] catalogues)
    {
        _engine = new LegacyEngine();
        return _engine.Setup(gameSystem, catalogues);
    }

    public void AddForce(int forceEntryIndex, int catalogueIndex = 0)
        => EnsureEngine().AddForce(forceEntryIndex, catalogueIndex);

    public void RemoveForce(int forceIndex)
        => EnsureEngine().RemoveForce(forceIndex);

    public void SelectEntry(int forceIndex, int entryIndex)
        => EnsureEngine().SelectEntry(forceIndex, entryIndex);

    public void SelectChildEntry(int forceIndex, int selectionIndex, int childEntryIndex)
        => EnsureEngine().SelectChildEntry(forceIndex, selectionIndex, childEntryIndex);

    public void DeselectSelection(int forceIndex, int selectionIndex)
        => EnsureEngine().DeselectSelection(forceIndex, selectionIndex);

    public void SetSelectionCount(int forceIndex, int entryIndex, int count)
        => EnsureEngine().SetSelectionCount(forceIndex, entryIndex, count);

    public void DuplicateSelection(int forceIndex, int selectionIndex)
        => EnsureEngine().DuplicateSelection(forceIndex, selectionIndex);

    public void SetCostLimit(string costTypeId, double value)
        => EnsureEngine().SetCostLimit(costTypeId, value);

    public RosterState GetRosterState()
        => EnsureEngine().GetRosterState();

    public IReadOnlyList<ValidationErrorState> GetValidationErrors()
        => EnsureEngine().GetValidationErrors();

    public void Dispose()
    {
        _engine = null;
    }

    private LegacyEngine EnsureEngine()
        => _engine ?? throw new InvalidOperationException("Engine not set up. Call Setup first.");
}
