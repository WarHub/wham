using FluentAssertions;
using WarHub.ArmouryModel.Source;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

public class RosterOperationsTests
{
    [Fact]
    public void Identity_ReturnsUnchangedState()
    {
        var state = TestData.CreateStateWithRoster();

        var result = RosterOperations.Identity.Apply(state);

        result.Should().BeSameAs(state);
    }

    [Fact]
    public void CreateRoster_CreatesRosterWithCostLimitsAndCosts()
    {
        var gst = TestData.CreateGamesystem();
        var state = RosterState.CreateFromNodes(gst);

        var result = RosterOperations.CreateRoster().ApplyTo(state);

        result.Roster.Should().NotBeNull();
        result.RosterRequired.CostLimits.Should().HaveCount(gst.CostTypes.Count);
        result.RosterRequired.Costs.Should().HaveCount(gst.CostTypes.Count);
    }

    [Fact]
    public void CreateRoster_WithName_SetsRosterName()
    {
        var gst = TestData.CreateGamesystem();
        var state = RosterState.CreateFromNodes(gst);

        var op = RosterOperations.CreateRoster() with { Name = "My Roster" };
        var resultState = op.ApplyTo(state);

        resultState.RosterRequired.Name.Should().Be("My Roster");
    }

    [Fact]
    public void ChangeRosterName_UpdatesName()
    {
        var state = TestData.CreateStateWithRoster();

        var result = RosterOperations.ChangeRosterName("Renamed").ApplyTo(state);

        result.RosterRequired.Name.Should().Be("Renamed");
    }

    [Fact]
    public void AddForce_AddsForceToRoster()
    {
        var state = TestData.CreateStateWithRoster();
        var forceEntry = state.Gamesystem.ForceEntries[0];

        var result = RosterOperations.AddForce(forceEntry).ApplyTo(state);

        result.RosterRequired.Forces.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveForce_RemovesForceFromRoster()
    {
        var state = TestData.CreateStateWithForce();
        var force = state.RosterRequired.Forces[0];

        var result = RosterOperations.RemoveForce(force).ApplyTo(state);

        result.RosterRequired.Forces.Should().BeEmpty();
    }

    [Fact]
    public void AddSelection_AddsSelectionToForce()
    {
        var gst = Gamesystem("TestGst")
            .AddCostTypes(CostType("pts"))
            .AddForceEntries(ForceEntry("Det"))
            .AddSelectionEntries(
                SelectionEntry("Unit").WithId("unit-1"));
        var state = RosterState.CreateFromNodes(gst);
        state = RosterOperations.CreateRoster().ApplyTo(state);
        var forceEntry = state.Gamesystem.ForceEntries[0];
        state = RosterOperations.AddForce(forceEntry).ApplyTo(state);
        var force = state.RosterRequired.Forces[0];
        var entry = state.Gamesystem.SelectionEntries[0];

        var result = RosterOperations.AddSelection(entry, force).ApplyTo(state);

        result.RosterRequired.Forces[0].Selections.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveSelection_RemovesSelectionFromForce()
    {
        var gst = Gamesystem("TestGst")
            .AddCostTypes(CostType("pts"))
            .AddForceEntries(ForceEntry("Det"))
            .AddSelectionEntries(
                SelectionEntry("Unit").WithId("unit-1"));
        var state = RosterState.CreateFromNodes(gst);
        state = RosterOperations.CreateRoster().ApplyTo(state);
        var forceEntry = state.Gamesystem.ForceEntries[0];
        state = RosterOperations.AddForce(forceEntry).ApplyTo(state);
        var force = state.RosterRequired.Forces[0];
        var entry = state.Gamesystem.SelectionEntries[0];
        state = RosterOperations.AddSelection(entry, force).ApplyTo(state);
        var selection = state.RosterRequired.Forces[0].Selections[0];

        var result = RosterOperations.RemoveSelection(selection).ApplyTo(state);

        result.RosterRequired.Forces[0].Selections.Should().BeEmpty();
    }

    [Fact]
    public void ChangeCostLimit_UpdatesExistingLimit()
    {
        var state = TestData.CreateStateWithRoster();
        var costLimit = state.RosterRequired.CostLimits[0];

        var result = RosterOperations.ChangeCostLimit(costLimit, 1000m).ApplyTo(state);

        result.RosterRequired.CostLimits[0].Value.Should().Be(1000m);
    }

    [Fact]
    public void ChangeCostLimit_ByTypeId_UpdatesLimit()
    {
        var state = TestData.CreateStateWithRoster();
        var typeId = state.RosterRequired.CostLimits[0].TypeId!;

        var result = RosterOperations.ChangeCostLimit(typeId, 500m).ApplyTo(state);

        result.RosterRequired.CostLimits
            .First(cl => cl.TypeId == typeId)
            .Value.Should().Be(500m);
    }
}
