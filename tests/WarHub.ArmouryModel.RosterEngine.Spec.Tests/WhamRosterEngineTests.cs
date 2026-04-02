using BattleScribeSpec.Protocol;
using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;
using CoreEngine = WarHub.ArmouryModel.RosterEngine.WhamRosterEngine;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Tests;

public class WhamRosterEngineTests
{
    private static (WhamCompilation compilation, ProtocolGameSystem gs) CreateTestCompilation()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            CostTypes =
            [
                new ProtocolCostType { Id = "pts", Name = "Points", DefaultCostLimit = -1 },
                new ProtocolCostType { Id = "pl", Name = "Power Level", DefaultCostLimit = -1 },
            ],
            CategoryEntries =
            [
                new ProtocolCategoryEntry { Id = "cat-hq", Name = "HQ" },
                new ProtocolCategoryEntry { Id = "cat-tr", Name = "Troops" },
            ],
            ForceEntries =
            [
                new ProtocolForceEntry
                {
                    Id = "fe-1",
                    Name = "Battalion",
                    CategoryLinks =
                    [
                        new ProtocolCategoryLink { Id = "cl-1", TargetId = "cat-hq", Name = "HQ" },
                        new ProtocolCategoryLink { Id = "cl-2", TargetId = "cat-tr", Name = "Troops" },
                    ],
                },
                new ProtocolForceEntry
                {
                    Id = "fe-2",
                    Name = "Patrol",
                    CategoryLinks =
                    [
                        new ProtocolCategoryLink { Id = "cl-3", TargetId = "cat-hq", Name = "HQ" },
                    ],
                },
            ],
        };
        var compilation = ProtocolConverter.CreateCompilation(gs, []);
        return (compilation, gs);
    }

    [Fact]
    public void CreateRoster_ReturnsValidRosterState()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation);

        state.Should().NotBeNull();
        state.RosterRequired.Should().NotBeNull();
    }

    [Fact]
    public void CreateRoster_HasGameSystemReference()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation);
        var roster = state.RosterRequired;

        roster.GameSystemId.Should().Be("gs-1");
        roster.GameSystemName.Should().Be("Test GS");
    }

    [Fact]
    public void CreateRoster_InitializesCostLimitsFromCostTypes()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation);
        var roster = state.RosterRequired;

        roster.CostLimits.Should().HaveCount(2);
        roster.CostLimits.Should().Contain(cl => cl.TypeId == "pts" && cl.Name == "Points");
        roster.CostLimits.Should().Contain(cl => cl.TypeId == "pl" && cl.Name == "Power Level");
    }

    [Fact]
    public void CreateRoster_InitializesZeroCosts()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation);
        var roster = state.RosterRequired;

        roster.Costs.Should().HaveCount(2);
        roster.Costs.Should().OnlyContain(c => c.Value == 0m);
    }

    [Fact]
    public void CreateRoster_StartsWithNoForces()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation);

        state.RosterRequired.Forces.Should().HaveCount(0);
    }

    [Fact]
    public void CreateRoster_WithName_SetsRosterName()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();

        var state = engine.CreateRoster(compilation, name: "My Army");

        state.RosterRequired.Name.Should().Be("My Army");
    }

    [Fact]
    public void AddForce_AppendsForceToRoster()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();

        state = engine.AddForce(state, forceEntry, gsSym);

        state.RosterRequired.Forces.Should().HaveCount(1);
        state.RosterRequired.Forces[0].Name.Should().Be("Battalion");
    }

    [Fact]
    public void AddForce_CreatesForceWithCategories()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();

        state = engine.AddForce(state, forceEntry, gsSym);

        var force = state.RosterRequired.Forces[0];
        force.Categories.Should().HaveCount(2);
        force.Categories.Should().Contain(c => c.Name == "HQ");
        force.Categories.Should().Contain(c => c.Name == "Troops");
    }

    [Fact]
    public void AddForce_MultipleForces_AppendsInOrder()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntries = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().ToList();

        state = engine.AddForce(state, forceEntries[0], gsSym);
        state = engine.AddForce(state, forceEntries[1], gsSym);

        state.RosterRequired.Forces.Should().HaveCount(2);
        state.RosterRequired.Forces[0].Name.Should().Be("Battalion");
        state.RosterRequired.Forces[1].Name.Should().Be("Patrol");
    }

    [Fact]
    public void RemoveForce_RemovesCorrectForce()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntries = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().ToList();
        state = engine.AddForce(state, forceEntries[0], gsSym);
        state = engine.AddForce(state, forceEntries[1], gsSym);

        state = engine.RemoveForce(state, 0);

        state.RosterRequired.Forces.Should().HaveCount(1);
        state.RosterRequired.Forces[0].Name.Should().Be("Patrol");
    }

    [Fact]
    public void RemoveForce_LastForce_LeavesEmptyRoster()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, gsSym);

        state = engine.RemoveForce(state, 0);

        state.RosterRequired.Forces.Should().HaveCount(0);
    }

    [Fact]
    public void RemoveForce_InvalidIndex_ThrowsArgumentOutOfRange()
    {
        var (compilation, _) = CreateTestCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var act = () => engine.RemoveForce(state, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EntryLink_HasConstraintsAndReferencedEntry()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Patrol" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Cat",
            GameSystemId = "test-gs",
            SharedSelectionEntries =
            [
                new ProtocolSelectionEntry { Id = "shared-unit", Name = "Strike Team", Type = "unit" }
            ],
            EntryLinks =
            [
                new ProtocolEntryLink
                {
                    Id = "link-1",
                    Name = "Strike Team",
                    TargetId = "shared-unit",
                    Type = "selectionEntry",
                    Constraints =
                    [
                        new ProtocolConstraint { Id = "con-link-max", Type = "max", Value = 2, Field = "selections", Scope = "force" }
                    ]
                }
            ]
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var catSymbol = compilation.GlobalNamespace.Catalogues.First(c => c.Id == "cat-1");

        // Check root entries include the entry link
        var rootEntries = catSymbol.RootContainerEntries;
        rootEntries.Length.Should().BeGreaterThan(0, "catalogue should have root entries");

        var selEntries = rootEntries.OfType<ISelectionEntryContainerSymbol>().ToList();
        selEntries.Count.Should().BeGreaterThan(0,
            "root entries should contain ISelectionEntryContainerSymbol, got: " +
            string.Join(", ", rootEntries.Select(e => e.GetType().Name)));

        var link = selEntries.FirstOrDefault(e => e.Id == "link-1");
        link.Should().NotBeNull("link-1 should be in root entries");
        link!.IsReference.Should().BeTrue("link-1 should be a reference");
        link.ReferencedEntry.Should().NotBeNull("link-1 should have ReferencedEntry");
        link.ReferencedEntry!.Id.Should().Be("shared-unit");
        link.Constraints.Length.Should().BeGreaterThan(0,
            "link-1 should have its own constraints");
    }
}
