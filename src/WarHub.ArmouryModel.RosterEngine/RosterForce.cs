using BattleScribeSpec.Protocol;

namespace WarHub.ArmouryModel.RosterEngine;

/// <summary>
/// Mutable state for a force within the roster.
/// </summary>
internal sealed class RosterForce
{
    public required ProtocolForceEntry ForceEntry { get; init; }
    public required ProtocolCatalogue Catalogue { get; init; }
    public List<RosterSelection> Selections { get; } = [];
}
