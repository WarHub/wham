using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Tests;

public class WhamRosterEngineTests
{
    private static (ProtocolGameSystem gs, ProtocolCatalogue[] cats) CreateBasicSetup()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            CostTypes = [new ProtocolCostType { Id = "pts", Name = "Points" }],
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Detachment" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Cat",
            GameSystemId = "test-gs",
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-a",
                    Name = "Unit A",
                    Type = "unit",
                    Costs = [new ProtocolCostValue { Name = "Points", TypeId = "pts", Value = 50 }],
                },
                new ProtocolSelectionEntry
                {
                    Id = "se-b",
                    Name = "Unit B",
                    Type = "unit",
                    Costs = [new ProtocolCostValue { Name = "Points", TypeId = "pts", Value = 100 }],
                },
            ],
        };
        return (gs, [cat]);
    }

    [Fact]
    public void Setup_ReturnsNoErrors()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        var errors = engine.Setup(gs, cats);
        Assert.Empty(errors);
    }

    [Fact]
    public void AddForce_CreatesEmptyForce()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        var state = engine.GetRosterState();
        Assert.Single(state.Forces);
        Assert.Equal("Detachment", state.Forces[0].Name);
        Assert.Empty(state.Forces[0].Selections);
    }

    [Fact]
    public void SelectEntry_AddsSelection()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.SelectEntry(0, 0);
        var state = engine.GetRosterState();
        Assert.Single(state.Forces[0].Selections);
        Assert.Equal("Unit A", state.Forces[0].Selections[0].Name);
        Assert.Equal(1, state.Forces[0].Selections[0].Number);
    }

    [Fact]
    public void Costs_CalculatedCorrectly()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.SelectEntry(0, 0); // 50 pts
        engine.SelectEntry(0, 1); // 100 pts
        var state = engine.GetRosterState();
        var ptsCost = state.Costs.First(c => c.TypeId == "pts");
        Assert.Equal(150, ptsCost.Value);
    }

    [Fact]
    public void AutoSelect_MinConstraint()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Det" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Cat",
            GameSystemId = "test-gs",
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-mandatory",
                    Name = "Mandatory Unit",
                    Type = "unit",
                    Constraints = [new ProtocolConstraint
                    {
                        Id = "con-min", Type = "min", Value = 1,
                        Field = "selections", Scope = "parent"
                    }],
                },
                new ProtocolSelectionEntry { Id = "se-optional", Name = "Optional", Type = "unit" },
            ],
        };

        using var engine = new WhamRosterEngine();
        engine.Setup(gs, [cat]);
        engine.AddForce(0);
        var state = engine.GetRosterState();
        Assert.Single(state.Forces[0].Selections);
        Assert.Equal("Mandatory Unit", state.Forces[0].Selections[0].Name);
    }

    [Fact]
    public void DeselectSelection_RemovesSelection()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.SelectEntry(0, 0);
        engine.DeselectSelection(0, 0);
        var state = engine.GetRosterState();
        Assert.Empty(state.Forces[0].Selections);
    }

    [Fact]
    public void DuplicateSelection_CreatesClone()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.SelectEntry(0, 0);
        engine.DuplicateSelection(0, 0);
        var state = engine.GetRosterState();
        Assert.Equal(2, state.Forces[0].Selections.Count);
        Assert.Equal("Unit A", state.Forces[0].Selections[1].Name);
    }

    [Fact]
    public void SetSelectionCount_IsNoOpForRootEntries()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.SelectEntry(0, 0);
        engine.SetSelectionCount(0, 0, 3);
        var state = engine.GetRosterState();
        // SetSelectionCount is a no-op for root entries
        Assert.Equal(1, state.Forces[0].Selections[0].Number);
        var ptsCost = state.Costs.First(c => c.TypeId == "pts");
        Assert.Equal(50, ptsCost.Value);
    }

    [Fact]
    public void RemoveForce_RemovesForce()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.AddForce(0);
        engine.RemoveForce(0);
        var state = engine.GetRosterState();
        Assert.Empty(state.Forces);
    }

    [Fact]
    public void CostLimit_GeneratesError()
    {
        using var engine = new WhamRosterEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.SetCostLimit("pts", 75);
        engine.AddForce(0);
        engine.SelectEntry(0, 1); // 100 pts, limit is 75
        var errors = engine.GetValidationErrors();
        Assert.Contains(errors, e => e.EntryId == "costLimits" && e.ConstraintId == "pts");
    }
}
