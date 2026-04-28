using BattleScribeSpec;
using BattleScribeSpec.Protocol;
using WarHub.ArmouryModel;
using WarHub.ArmouryModel.RosterEngine.Spec;
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

    private static SpecRosterEngineAdapter CreateEngine()
        => new();

    [Fact]
    public void Setup_ReturnsNoErrors()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        var errors = engine.Setup(gs, cats);
        Assert.Empty(errors);
    }

    [Fact]
    public void AddForce_CreatesEmptyForce()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var outputs = engine.AddForce("fe-1", "cat-1");
        Assert.NotNull(outputs.ForceId);
        var state = engine.GetRosterState();
        Assert.Single(state.Forces);
        Assert.Equal("Detachment", state.Forces[0].Name);
        Assert.Empty(state.Forces[0].Selections);
    }

    [Fact]
    public void SelectEntry_AddsSelection()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        var selOutputs = engine.SelectEntry(forceOutputs.ForceId!, "se-a");
        Assert.NotNull(selOutputs.SelectionId);
        var state = engine.GetRosterState();
        Assert.Single(state.Forces[0].Selections);
        Assert.Equal("Unit A", state.Forces[0].Selections[0].Name);
        Assert.Equal(1, state.Forces[0].Selections[0].Number);
    }

    [Fact]
    public void Costs_CalculatedCorrectly()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        engine.SelectEntry(forceOutputs.ForceId!, "se-a"); // 50 pts
        engine.SelectEntry(forceOutputs.ForceId!, "se-b"); // 100 pts
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

        using var engine = CreateEngine();
        engine.Setup(gs, [cat]);
        engine.AddForce("fe-1", "cat-1");
        var state = engine.GetRosterState();
        Assert.Single(state.Forces[0].Selections);
        Assert.Equal("Mandatory Unit", state.Forces[0].Selections[0].Name);
    }

    [Fact]
    public void DeselectSelection_RemovesSelection()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        var selOutputs = engine.SelectEntry(forceOutputs.ForceId!, "se-a");
        engine.DeselectSelection(forceOutputs.ForceId!, selOutputs.SelectionId!);
        var state = engine.GetRosterState();
        Assert.Empty(state.Forces[0].Selections);
    }

    [Fact]
    public void DuplicateSelection_CreatesClone()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        var selOutputs = engine.SelectEntry(forceOutputs.ForceId!, "se-a");
        engine.DuplicateSelection(forceOutputs.ForceId!, selOutputs.SelectionId!);
        var state = engine.GetRosterState();
        Assert.Equal(2, state.Forces[0].Selections.Count);
        Assert.Equal("Unit A", state.Forces[0].Selections[1].Name);
    }

    [Fact]
    public void SetSelectionCount_UpdatesNumber()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        var selOutputs = engine.SelectEntry(forceOutputs.ForceId!, "se-a");
        engine.SetSelectionCount(forceOutputs.ForceId!, selOutputs.SelectionId!, 3);
        var state = engine.GetRosterState();
        Assert.Equal(3, state.Forces[0].Selections[0].Number);
        var ptsCost = state.Costs.First(c => c.TypeId == "pts");
        Assert.Equal(150, ptsCost.Value);
    }

    [Fact]
    public void RemoveForce_RemovesForce()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        engine.RemoveForce(forceOutputs.ForceId!);
        var state = engine.GetRosterState();
        Assert.Empty(state.Forces);
    }

    [Fact]
    public void CostLimit_GeneratesError()
    {
        using var engine = CreateEngine();
        var (gs, cats) = CreateBasicSetup();
        engine.Setup(gs, cats);
        engine.SetCostLimit("pts", 75);
        var forceOutputs = engine.AddForce("fe-1", "cat-1");
        engine.SelectEntry(forceOutputs.ForceId!, "se-b"); // 100 pts, limit is 75
        var errors = engine.GetValidationErrors();
        Assert.Contains(errors, e => e.EntryId == "costLimits" && e.ConstraintId == "pts");
    }

    [Fact]
    public void Modifier_SetCost_DiagnosticTest()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            CostTypes = [new ProtocolCostType { Id = "pts", Name = "Points" }],
            ForceEntries = [new ProtocolForceEntry { Id = "fe-patrol", Name = "Patrol" }],
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
                    Id = "se-unit-a",
                    Name = "Unit A",
                    Type = "unit",
                    Costs = [new ProtocolCostValue { Name = "Points", TypeId = "pts", Value = 100 }],
                    Modifiers =
                    [
                        new ProtocolModifier { Type = "set", Field = "pts", Value = "50" }
                    ],
                },
            ],
        };

        // Convert to compilation and check symbols directly
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        
        // Find the entry symbol
        var gsSym = compilation.GlobalNamespace.RootCatalogue;
        var allCats = compilation.GlobalNamespace.Catalogues;
        
        // Look through all catalogues for entries
        var allSymbols = new List<string>();
        foreach (var c in allCats)
        {
            foreach (var entry in c.RootContainerEntries)
            {
                allSymbols.Add($"Cat={c.Name} Entry={entry.Name} Id={entry.Id} Type={entry.GetType().Name} Effects={entry.Effects.Length}");
                if (entry is ISelectionEntryContainerSymbol sec)
                {
                    allSymbols.Add($"  IsSelectionEntryContainer=true ChildEntries={sec.ChildSelectionEntries.Length}");
                }
            }
            foreach (var entry in c.SharedSelectionEntryContainers)
            {
                allSymbols.Add($"Cat={c.Name} Shared={entry.Name} Id={entry.Id} Type={entry.GetType().Name}");
            }
        }
        // Also check gamesystem
        foreach (var entry in gsSym.RootContainerEntries)
        {
            allSymbols.Add($"GS Entry={entry.Name} Id={entry.Id} Type={entry.GetType().Name} Effects={entry.Effects.Length}");
        }
        
        var info = string.Join("\n", allSymbols);
        
        // The entry should have 1 effect (the cost modifier)
        // Find it
        IEntrySymbol? found = null;
        foreach (var c in allCats)
        {
            foreach (var entry in c.RootContainerEntries)
            {
                if (entry.Id == "se-unit-a") found = entry;
            }
        }
        
        Assert.NotNull(found);
        Assert.True(found!.Effects.Length > 0, $"Entry has {found.Effects.Length} effects. All symbols:\n{info}");
        
        var effect = found.Effects[0];
        Assert.Equal(EffectTargetKind.Member, effect.TargetKind);
        Assert.Equal(EffectOperation.SetValue, effect.FunctionKind);
        Assert.Equal("50", effect.OperandValue);
        
        var tm = effect.TargetMember;
        var tmInfo = tm is null ? "NULL" : $"Type={tm.GetType().Name} Kind={tm.Kind} Id={tm.Id} Name={tm.Name}";
        Assert.True(tm is not null, $"TargetMember was: {tmInfo}");
        // TargetMember is an IResourceEntrySymbol (CostSymbol), not IResourceDefinitionSymbol
        Assert.True(tm is IResourceEntrySymbol, $"TargetMember should be IResourceEntrySymbol: {tmInfo}");
        var resourceEntry = (IResourceEntrySymbol)tm!;
        Assert.NotNull(resourceEntry.Type);
        Assert.Equal("pts", resourceEntry.Type!.Id);
    }
}
