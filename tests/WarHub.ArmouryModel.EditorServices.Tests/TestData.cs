using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

internal static class TestData
{
    public static GamesystemNode CreateGamesystem() =>
        Gamesystem("TestGst")
            .AddCostTypes(CostType("pts"))
            .AddForceEntries(ForceEntry("Detachment"));

    public static RosterState CreateStateWithRoster()
    {
        var gst = CreateGamesystem();
        var state = RosterState.CreateFromNodes(gst);
        return RosterOperations.CreateRoster().ApplyTo(state);
    }

    public static RosterState CreateStateWithForce()
    {
        var state = CreateStateWithRoster();
        var forceEntry = state.Gamesystem.ForceEntries[0];
        return RosterOperations.AddForce(forceEntry).ApplyTo(state);
    }

    /// <summary>
    /// Helper to call explicit interface Apply method.
    /// </summary>
    public static RosterState ApplyTo(this IRosterOperation op, RosterState state) => op.Apply(state);
}
