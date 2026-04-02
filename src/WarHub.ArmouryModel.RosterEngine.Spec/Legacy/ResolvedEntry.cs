using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Legacy;

/// <summary>
/// An entry with link overrides applied but before modifier evaluation.
/// This is the "base" definition that modifiers operate on.
/// </summary>
internal sealed class ResolvedEntry
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Type { get; init; } = "unit";
    public bool Hidden { get; init; }
    public bool Collective { get; init; }
    public string? Page { get; init; }
    public string? PublicationId { get; init; }
    public List<ProtocolCostValue> Costs { get; init; } = [];
    public List<ProtocolConstraint> Constraints { get; init; } = [];
    public List<ProtocolModifier> Modifiers { get; init; } = [];
    public List<ProtocolModifierGroup> ModifierGroups { get; init; } = [];
    public List<ProtocolSelectionEntry> ChildSelectionEntries { get; init; } = [];
    public List<ProtocolEntryLink> ChildEntryLinks { get; init; } = [];
    public List<ProtocolSelectionEntryGroup> ChildSelectionEntryGroups { get; init; } = [];
    public List<ProtocolCategoryLink> CategoryLinks { get; init; } = [];
    public List<ProtocolProfile> Profiles { get; init; } = [];
    public List<ProtocolRule> Rules { get; init; } = [];
    public List<ProtocolInfoGroup> InfoGroups { get; init; } = [];
    public List<ProtocolInfoLink> InfoLinks { get; init; } = [];
}
