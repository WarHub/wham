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

    /// <summary>
    /// Creates a test compilation with a catalogue containing an entry link
    /// to a shared entry — the common pattern for BattleScribe data.
    /// </summary>
    private static (WhamCompilation compilation, ProtocolGameSystem gs) CreateEntryLinkCompilation()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            ForceEntries = [new ProtocolForceEntry { Id = "fe-patrol", Name = "Patrol" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Test Catalogue",
            GameSystemId = "test-gs",
            SharedSelectionEntries =
            [
                new ProtocolSelectionEntry { Id = "se-shared-unit", Name = "Base Unit", Type = "upgrade" }
            ],
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-squad",
                    Name = "Squad",
                    Type = "unit",
                    EntryLinks =
                    [
                        new ProtocolEntryLink
                        {
                            Id = "el-unit",
                            Name = "Base Unit",
                            TargetId = "se-shared-unit",
                            Type = "selectionEntry",
                        }
                    ]
                }
            ]
        };
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
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

    /// <summary>
    /// When selecting a child entry that is an entry link, the engine should
    /// produce a selection whose entryId is "linkId::targetId" (BattleScribe
    /// format). This ensures the binder resolves SourceEntryPath = [link, target]
    /// with SourceEntry being the resolved target entry, not the link itself.
    /// </summary>
    [Fact]
    public void SelectChildEntry_EntryLink_SourceEntryPathIncludesLinkAndTarget()
    {
        var (compilation, _) = CreateEntryLinkCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        // Get catalogue and force entry symbols
        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => c.Id == "cat-1");
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();

        // Add force with the catalogue
        state = engine.AddForce(state, forceEntry, catSym);
        var force = state.RosterRequired.Forces[0];

        // Select the root entry (Squad)
        var squadEntry = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>()
            .First(e => e.Id == "se-squad");
        state = engine.SelectEntry(state, 0, squadEntry);

        // Select the child entry link (el-unit -> se-shared-unit)
        var squadSelection = state.RosterRequired.Forces[0].Selections[0];
        var entryLink = squadEntry.ChildSelectionEntries
            .First(e => e.Id == "el-unit");
        state = engine.SelectChildEntry(state, 0, 0, entryLink);

        // Recompile to get symbols
        var recompiled = state.Compilation;
        var roster = recompiled.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        recompiled.GetDiagnostics(cts.Token);

        var parentSelection = roster.Forces[0].Selections[0];
        var childSelection = parentSelection.Selections.Single();

        // The child selection's SourceEntryPath should contain
        // [entryLink, resolvedTarget], matching BattleScribe's :: format.
        childSelection.SourceEntryPath.SourceEntries.Should().HaveCount(2,
            "entry link selections should have a 2-element path: [link, target]");
        childSelection.SourceEntryPath.SourceEntries[0].Id.Should().Be("el-unit",
            "first path element should be the entry link");
        childSelection.SourceEntryPath.SourceEntries[1].Id.Should().Be("se-shared-unit",
            "second path element should be the resolved target");

        // SourceEntry should be the resolved target, not the link.
        childSelection.SourceEntry.Id.Should().Be("se-shared-unit",
            "SourceEntry should resolve through the link to the target entry");
        childSelection.SourceEntry.Should().BeAssignableTo<ISelectionEntrySymbol>(
            "SourceEntry should be a concrete ISelectionEntrySymbol, not a link");
    }

    /// <summary>
    /// Verifies that AddForce records the correct catalogue (not the gamesystem)
    /// when the force entry is defined in the gamesystem but the catalogue is
    /// a separate catalogue. This was a bug where NodeFactory.Force derived
    /// catalogueId from the force entry's ancestor (gamesystem).
    /// </summary>
    [Fact]
    public void AddForce_WithCatalogue_RecordsCorrectCatalogueId()
    {
        var (compilation, _) = CreateEntryLinkCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => c.Id == "cat-1");
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();

        state = engine.AddForce(state, forceEntry, catSym);

        var force = state.RosterRequired.Forces[0];
        // The force's catalogueId should reference the actual catalogue, not the gamesystem
        force.CatalogueId.Should().Be("cat-1",
            "force should reference the selected catalogue, not the gamesystem where the force entry is defined");
        force.CatalogueName.Should().Be("Test Catalogue");
    }

    /// <summary>
    /// Direct entries (not links) should produce a single-element SourceEntryPath,
    /// with SourceEntry equal to the entry itself.
    /// </summary>
    [Fact]
    public void SelectEntry_DirectEntry_SourceEntryIsSingleElement()
    {
        var (compilation, _) = CreateEntryLinkCompilation();
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => c.Id == "cat-1");
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, catSym);

        var squadEntry = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>()
            .First(e => e.Id == "se-squad");
        state = engine.SelectEntry(state, 0, squadEntry);

        // Recompile and check
        var recompiled = state.Compilation;
        var roster = recompiled.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        recompiled.GetDiagnostics(cts.Token);

        var selection = roster.Forces[0].Selections[0];

        // Direct entries have a single-element path
        selection.SourceEntryPath.SourceEntries.Should().HaveCount(1,
            "direct entry selections should have a single-element path");
        selection.SourceEntry.Id.Should().Be("se-squad");
        selection.SourceEntry.Should().BeAssignableTo<ISelectionEntrySymbol>();
    }

    /// <summary>
    /// Entry links to shared entries defined in the gamesystem should also
    /// produce the correct 2-element SourceEntryPath.
    /// </summary>
    [Fact]
    public void SelectChildEntry_EntryLinkToGamesystemShared_SourceEntryResolvesCorrectly()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "test-gs",
            Name = "Test GS",
            ForceEntries = [new ProtocolForceEntry { Id = "fe-patrol", Name = "Patrol" }],
            SharedSelectionEntries =
            [
                new ProtocolSelectionEntry { Id = "se-gs-weapon", Name = "Shared Weapon", Type = "upgrade" }
            ],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Test Catalogue",
            GameSystemId = "test-gs",
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-unit",
                    Name = "Unit A",
                    Type = "unit",
                    EntryLinks =
                    [
                        new ProtocolEntryLink
                        {
                            Id = "el-weapon",
                            Name = "Shared Weapon",
                            TargetId = "se-gs-weapon",
                            Type = "selectionEntry",
                        }
                    ]
                }
            ]
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => c.Id == "cat-1");
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();

        state = engine.AddForce(state, forceEntry, catSym);

        var unitEntry = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>()
            .First(e => e.Id == "se-unit");
        state = engine.SelectEntry(state, 0, unitEntry);

        var entryLink = unitEntry.ChildSelectionEntries.First(e => e.Id == "el-weapon");
        state = engine.SelectChildEntry(state, 0, 0, entryLink);

        // Recompile and check
        var recompiled = state.Compilation;
        var roster = recompiled.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        recompiled.GetDiagnostics(cts.Token);

        var childSelection = roster.Forces[0].Selections[0].Selections.Single();

        // Path should be [link, gamesystem-shared-target]
        childSelection.SourceEntryPath.SourceEntries.Should().HaveCount(2);
        childSelection.SourceEntryPath.SourceEntries[0].Id.Should().Be("el-weapon");
        childSelection.SourceEntryPath.SourceEntries[1].Id.Should().Be("se-gs-weapon");

        childSelection.SourceEntry.Id.Should().Be("se-gs-weapon",
            "SourceEntry should resolve to the gamesystem's shared entry");
    }
}
