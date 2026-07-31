using BattleScribeSpec.Protocol;
using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using Xunit;
using CoreEngine = WarHub.ArmouryModel.RosterEngine.WhamRosterEngine;

namespace WarHub.ArmouryModel.RosterEngine.Spec.Tests;

public class EffectiveSymbolEdgeCaseTests
{
    private static ProtocolProfileType SingleCharProfileType() => new()
    {
        Id = "pt-1",
        Name = "Stats",
        CharacteristicTypes = [new ProtocolCharacteristicType { Id = "ct-1", Name = "M" }],
    };

    private static ProtocolProfile MakeProfile(string id, string name, string charValue) => new()
    {
        Id = id,
        Name = name,
        TypeId = "pt-1",
        TypeName = "Stats",
        Characteristics = [new ProtocolCharacteristic { Name = "M", TypeId = "ct-1", Value = charValue }],
    };

    [Fact]
    public void ResourceTraversalOrder_DirectProfilesBeforeRulesBeforeLinksBeforeGroups()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            ProfileTypes = [SingleCharProfileType()],
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Detachment" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Test Cat",
            GameSystemId = "gs-1",
            SharedProfiles = [MakeProfile("sp-linked", "Linked Profile", "6")],
            SelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "se-1", Name = "Unit", Type = "unit",
                    // Direct rule
                    Rules = [new ProtocolRule { Id = "r-1", Name = "Direct Rule", Description = "A rule" }],
                    // Direct profile
                    Profiles = [MakeProfile("p-direct", "Direct Profile", "5")],
                    // InfoLink to shared profile
                    InfoLinks =
                    [
                        new ProtocolInfoLink { Id = "il-1", TargetId = "sp-linked", Type = "profile" },
                    ],
                    // Inline InfoGroup with a profile
                    InfoGroups =
                    [
                        new ProtocolInfoGroup
                        {
                            Id = "ig-1", Name = "Group",
                            Profiles = [MakeProfile("p-group", "Group Profile", "7")],
                        },
                    ],
                },
            ],
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, catSym);

        var selEntry = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>().First();
        state = engine.SelectEntry(state, 0, selEntry);

        var roster = state.Compilation.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        state.Compilation.GetDiagnostics(cts.Token);

        var effectiveEntry = roster.Forces[0].Selections[0].EffectiveSourceEntry;

        // Single-pass traversal: resources appear in source order
        effectiveEntry.Resources.OfType<IProfileSymbol>().Select(p => p.Name)
            .Should().BeEquivalentTo(["Direct Profile", "Linked Profile", "Group Profile"]);

        effectiveEntry.Resources.OfType<IRuleSymbol>().Should().ContainSingle()
            .Which.Name.Should().Be("Direct Rule");
    }

    [Fact]
    public void EntryLinkResourceResolution_GetsTargetProfiles()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            ProfileTypes = [SingleCharProfileType()],
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Detachment" }],
        };
        var cat = new ProtocolCatalogue
        {
            Id = "cat-1",
            Name = "Test Cat",
            GameSystemId = "gs-1",
            SharedSelectionEntries =
            [
                new ProtocolSelectionEntry
                {
                    Id = "sse-target", Name = "Target Unit", Type = "unit",
                    Profiles = [MakeProfile("p-target", "Target Profile", "8")],
                    Rules = [new ProtocolRule { Id = "r-target", Name = "Target Rule", Description = "Desc" }],
                },
            ],
            EntryLinks =
            [
                new ProtocolEntryLink
                {
                    Id = "el-1", Name = "Target Unit Link", TargetId = "sse-target", Type = "selectionEntry",
                },
            ],
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, catSym);

        var entryLink = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>()
            .First(e => e.Id == "el-1");
        state = engine.SelectEntry(state, 0, entryLink);

        var roster = state.Compilation.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        state.Compilation.GetDiagnostics(cts.Token);

        var effectiveEntry = roster.Forces[0].Selections[0].EffectiveSourceEntry;

        effectiveEntry.Resources.OfType<IProfileSymbol>().Should().ContainSingle()
            .Which.Name.Should().Be("Target Profile");
        effectiveEntry.Resources.OfType<IRuleSymbol>().Should().ContainSingle()
            .Which.Name.Should().Be("Target Rule");
    }

    [Fact]
    public void PageWithoutPublicationId_PreservedInEffectiveProfile()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            ProfileTypes = [SingleCharProfileType()],
            ForceEntries = [new ProtocolForceEntry { Id = "fe-1", Name = "Detachment" }],
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
                    Id = "se-1", Name = "Unit", Type = "unit",
                    Profiles =
                    [
                        new ProtocolProfile
                        {
                            Id = "p-1", Name = "Unit Stats",
                            TypeId = "pt-1", TypeName = "Stats",
                            Page = "42",
                            // No PublicationId set
                            Characteristics = [new ProtocolCharacteristic { Name = "M", TypeId = "ct-1", Value = "5" }],
                        },
                    ],
                },
            ],
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, [cat]);
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var catSym = state.Compilation.GlobalNamespace.Catalogues.First(c => !c.IsGamesystem);
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, catSym);

        var selEntry = catSym.RootContainerEntries.OfType<ISelectionEntryContainerSymbol>().First();
        state = engine.SelectEntry(state, 0, selEntry);

        var roster = state.Compilation.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        state.Compilation.GetDiagnostics(cts.Token);

        var profile = roster.Forces[0].Selections[0].EffectiveSourceEntry
            .Resources.OfType<IProfileSymbol>().Single();

        profile.Page.Should().Be("42");
        profile.PublicationReference.Should().NotBeNull();
        profile.PublicationReference!.PublicationId.Should().BeNull();
        profile.PublicationReference.Publication.Should().BeNull();
    }

    [Fact]
    public void ForceEffectiveResources_ResolvedWithNullContext()
    {
        var gs = new ProtocolGameSystem
        {
            Id = "gs-1",
            Name = "Test GS",
            ProfileTypes = [SingleCharProfileType()],
            SharedProfiles = [MakeProfile("sp-force", "Force Profile", "10")],
            ForceEntries =
            [
                new ProtocolForceEntry
                {
                    Id = "fe-1", Name = "Detachment",
                    InfoLinks =
                    [
                        new ProtocolInfoLink { Id = "il-fe-1", TargetId = "sp-force", Type = "profile" },
                    ],
                },
            ],
        };

        var compilation = ProtocolConverter.CreateCompilation(gs, []);
        var engine = new CoreEngine();
        var state = engine.CreateRoster(compilation);

        var gsSym = state.Compilation.GlobalNamespace.RootCatalogue;
        var forceEntry = gsSym.RootContainerEntries.OfType<IForceEntrySymbol>().First();
        state = engine.AddForce(state, forceEntry, gsSym);

        var roster = state.Compilation.GlobalNamespace.Rosters.First();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        state.Compilation.GetDiagnostics(cts.Token);

        var force = roster.Forces[0];
        force.EffectiveSourceEntry.Resources.OfType<IProfileSymbol>().Should().ContainSingle()
            .Which.Name.Should().Be("Force Profile");
    }
}
