using BattleScribeSpec.Protocol;
using FluentAssertions;
using WarHub.ArmouryModel;
using WarHub.ArmouryModel.RosterEngine.Spec;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Tests;

public class ProtocolConverterTests
{
    private static (ProtocolGameSystem gs, ProtocolCatalogue cat) CreateBasicInput()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            CostTypes = [new ProtocolCostType { Id = "pts", Name = "Points" }],
            ProfileTypes =
            [
                new ProtocolProfileType
                {
                    Id = "pt-1", Name = "Unit",
                    CharacteristicTypes = [new ProtocolCharacteristicType { Id = "ct-m", Name = "M" }]
                }
            ],
            CategoryEntries = [new ProtocolCategoryEntry { Id = "cat-hq", Name = "HQ" }],
            ForceEntries =
            [
                new ProtocolForceEntry
                {
                    Id = "fe-1", Name = "Battalion",
                    CategoryLinks =
                    [
                        new ProtocolCategoryLink { Id = "cl-1", TargetId = "cat-hq", Name = "HQ", Primary = false }
                    ]
                }
            ],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Test Cat",
            GameSystemId = "gs-1",
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-1", Name = "Commander", Type = "unit",
                    Costs = [new ProtocolCostValue { Name = "Points", TypeId = "pts", Value = 100 }],
                    CategoryLinks =
                    [
                        new ProtocolCategoryLink { Id = "cl-se1", TargetId = "cat-hq", Name = "HQ", Primary = true }
                    ],
                    Constraints =
                    [
                        new ProtocolConstraint { Id = "con-1", Type = "max", Value = 3, Field = "selections", Scope = "force" }
                    ],
                    Modifiers =
                    [
                        new ProtocolModifier
                        {
                            Type = "set", Field = "hidden", Value = "true",
                            Conditions = [new ProtocolCondition { Type = "atLeast", Value = 3, Field = "selections", Scope = "force", ChildId = "se-1" }]
                        }
                    ],
                }
            ],
            SharedSelectionEntries =
            [
                new ProtocolSelectionEntry { Id = "sse-1", Name = "Shared Upgrade", Type = "upgrade" }
            ],
            EntryLinks =
            [
                new ProtocolEntryLink { Id = "el-1", Name = "Shared Upgrade Link", TargetId = "sse-1", Type = "selectionEntry" }
            ],
        };
        return (gs, cat);
    }

    [Fact]
    public void CreateCompilation_ProducesValidCompilation()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);

        compilation.Should().NotBeNull();
        compilation.SourceTrees.Should().HaveCount(2);
    }

    [Fact]
    public void CreateCompilation_ResolvesGamesystem()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var ns = compilation.GlobalNamespace;

        ns.RootCatalogue.Should().NotBeNull();
        ns.RootCatalogue.IsGamesystem.Should().BeTrue();
        ns.RootCatalogue.Name.Should().Be("Test GS");
    }

    [Fact]
    public void CreateCompilation_ResolvesCatalogue()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var ns = compilation.GlobalNamespace;

        ns.Catalogues.Should().HaveCount(2); // gamesystem + catalogue
        var catalogue = ns.Catalogues.First(c => !c.IsGamesystem);
        catalogue.Name.Should().Be("Test Cat");
        catalogue.Id.Should().Be("cat-1");
    }

    [Fact]
    public void ConvertGameSystem_MapsAllTopLevelFields()
    {
        var (gs, _) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, []);
        var gsSym = compilation.GlobalNamespace.RootCatalogue;

        gsSym.Id.Should().Be("gs-1");
        gsSym.Name.Should().Be("Test GS");
        gsSym.ResourceDefinitions.Should().Contain(rd => rd.Name == "Points");
    }

    [Fact]
    public void ConvertCatalogue_MapsEntriesAndLinks()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var catalogue = compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);

        catalogue.Id.Should().Be("cat-1");
        catalogue.Name.Should().Be("Test Cat");
        catalogue.RootContainerEntries.Should().NotBeEmpty();
        catalogue.SharedSelectionEntryContainers.Should().NotBeEmpty();
    }

    [Fact]
    public void ConvertSelectionEntry_MapsAllFields()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var catalogue = compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);

        // First root entry should be the Commander
        var entry = catalogue.RootContainerEntries.OfType<ISelectionEntrySymbol>().First();
        entry.Name.Should().Be("Commander");
        entry.Constraints.Should().NotBeEmpty();
        entry.Effects.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateCompilation_EntryLinkResolvesToSharedEntry()
    {
        var (gs, cat) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var catalogue = compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);

        // Should have entries from both direct entries and links
        catalogue.RootContainerEntries.Should().HaveCountGreaterThanOrEqualTo(2);
        // The link should be a reference
        var link = catalogue.RootContainerEntries.FirstOrDefault(e => e.IsReference);
        link.Should().NotBeNull();
    }

    [Fact]
    public void ConvertForceEntry_MapsCategoryLinks()
    {
        var (gs, _) = CreateBasicInput();
        var compilation = ProtocolConverter.CreateCompilation(gs, []);
        var gsSym = compilation.GlobalNamespace.RootCatalogue;
        var forceEntries = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>();

        forceEntries.Should().ContainSingle();
        var fe = forceEntries.Single();
        fe.Name.Should().Be("Battalion");
        fe.Categories.Should().NotBeEmpty();
    }
}
