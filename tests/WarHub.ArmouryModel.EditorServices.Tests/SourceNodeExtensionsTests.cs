using FluentAssertions;
using WarHub.ArmouryModel.Source;
using Xunit;
using static WarHub.ArmouryModel.Source.NodeFactory;

namespace WarHub.ArmouryModel.EditorServices;

public class SourceNodeExtensionsTests
{
    [Fact]
    public void WithUpdatedNumberAndCosts_UpdatesCountAndScalesCosts()
    {
        var gst = Gamesystem("TestGst")
            .AddCostTypes(CostType("pts"))
            .AddForceEntries(ForceEntry("Det"))
            .AddSelectionEntries(
                SelectionEntry("Unit")
                    .WithId("unit-1")
                    .AddCosts(Cost(CostType("pts"), 10)));
        var state = RosterState.CreateFromNodes(gst);
        state = RosterOperations.CreateRoster().ApplyTo(state);
        state = RosterOperations.AddForce(state.Gamesystem.ForceEntries[0]).ApplyTo(state);
        var force = state.RosterRequired.Forces[0];
        state = RosterOperations.AddSelection(state.Gamesystem.SelectionEntries[0], force).ApplyTo(state);
        var selection = state.RosterRequired.Forces[0].Selections[0];

        var updated = selection.WithUpdatedNumberAndCosts(3);

        updated.Number.Should().Be(3);
        updated.Costs[0].Value.Should().Be(30m);
    }

    [Fact]
    public void WithUpdatedNumberAndCosts_SameCount_ReturnsSameInstance()
    {
        var selection = Selection(SelectionEntry("Unit"), "unit-1")
            .WithNumber(2)
            .AddCosts(Cost(CostType("pts"), 10));

        var result = selection.WithUpdatedNumberAndCosts(2);

        result.Should().BeSameAs(selection);
    }

    [Fact]
    public void WithUpdatedNumberAndCosts_ZeroCount_Throws()
    {
        var selection = Selection(SelectionEntry("Unit"), "unit-1");

        var act = () => selection.WithUpdatedNumberAndCosts(0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WithUpdatedCostTotals_AggregatesCostsFromSelections()
    {
        var costType = CostType("pts");
        var gst = Gamesystem("TestGst")
            .AddCostTypes(costType)
            .AddForceEntries(ForceEntry("Det"))
            .AddSelectionEntries(
                SelectionEntry("Unit")
                    .WithId("unit-1")
                    .AddCosts(Cost(costType, 15)));
        var state = RosterState.CreateFromNodes(gst);
        state = RosterOperations.CreateRoster().ApplyTo(state);
        state = RosterOperations.AddForce(state.Gamesystem.ForceEntries[0]).ApplyTo(state);
        var force = state.RosterRequired.Forces[0];
        state = RosterOperations.AddSelection(state.Gamesystem.SelectionEntries[0], force).ApplyTo(state);

        var roster = state.RosterRequired;
        var updated = roster.WithUpdatedCostTotals();

        updated.Costs.Should().Contain(c => c.Value == 15m);
    }

    [Fact]
    public void Replace_ReplacesSingleNode()
    {
        var costType = CostType("pts");
        var roster = Roster(Gamesystem("G"), "Test")
            .AddCosts(Cost(costType, 5));
        var costNode = roster.Costs[0];

        var result = roster.Replace(costNode, c => c.WithValue(99));

        result.Costs[0].Value.Should().Be(99);
    }

    [Fact]
    public void Remove_RemovesNodeFromTree()
    {
        var costType = CostType("pts");
        var roster = Roster(Gamesystem("G"), "Test")
            .AddCosts(Cost(costType, 5), Cost(costType, 10));

        var result = roster.Remove(roster.Costs[0]);

        result.Costs.Should().HaveCount(1);
        result.Costs[0].Value.Should().Be(10);
    }

    [Fact]
    public void ReplaceFluent_WithSelector_ReplacesMatchedNode()
    {
        var costType = CostType("pts");
        var roster = Roster(Gamesystem("G"), "Test")
            .AddCosts(Cost(costType, 5));

        var result = roster.ReplaceFluent(r => r.Costs[0], c => c.WithValue(42));

        result.Costs[0].Value.Should().Be(42);
    }
}
