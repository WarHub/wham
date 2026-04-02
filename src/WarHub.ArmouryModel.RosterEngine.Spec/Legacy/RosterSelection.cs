using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Legacy;

/// <summary>
/// Mutable state for a selection within a force.
/// </summary>
internal sealed class RosterSelection
{
    /// <summary>The entry definition this selection was created from.</summary>
    public required ProtocolSelectionEntry Entry { get; init; }

    /// <summary>For entry links, the link that pointed to this entry.</summary>
    public ProtocolEntryLink? SourceLink { get; init; }

    /// <summary>For entry groups, the group this entry belongs to.</summary>
    public ProtocolSelectionEntryGroup? SourceGroup { get; init; }

    /// <summary>Number of instances of this selection.</summary>
    public int Number { get; set; } = 1;

    /// <summary>Child selections under this selection.</summary>
    public List<RosterSelection> Children { get; } = [];
}
