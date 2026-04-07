using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class IncrementalCompilationTests
{
    [Fact]
    public void RosterCompilation_reuses_catalogue_symbol_references()
    {
        // arrange: create catalogue compilation
        var gst = NodeFactory.Gamesystem("foo")
            .AddCostTypes(NodeFactory.CostType("pts", 0m));
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry"));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // act: create roster compilation referencing catalogue
        var rosterNode = NodeFactory.Roster(gst)
            .WithCosts(NodeFactory.Cost("pts", "pts", 0m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(rosterNode)],
            catalogueCompilation);

        // assert: catalogue symbols are same object references
        var catSymbol = catalogueCompilation.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem);
        var rosterCatSymbol = rosterCompilation.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem);
        ReferenceEquals(catSymbol, rosterCatSymbol).Should().BeTrue(
            "catalogue symbols should be same object reference (not duplicated)");

        var gsSym = catalogueCompilation.GlobalNamespace.RootCatalogue;
        var rosterGsSym = rosterCompilation.GlobalNamespace.RootCatalogue;
        ReferenceEquals(gsSym, rosterGsSym).Should().BeTrue(
            "gamesystem symbol should be same object reference");
    }

    [Fact]
    public void RosterCompilation_has_own_roster_symbols()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
        ]);
        var rosterNode = NodeFactory.Roster(gst);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(rosterNode)],
            catalogueCompilation);

        // assert
        rosterCompilation.GlobalNamespace.Rosters.Should().ContainSingle();
        catalogueCompilation.GlobalNamespace.Rosters.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceSourceTree_preserves_references()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo")
            .AddCostTypes(NodeFactory.CostType("pts", 0m));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
        ]);
        var rosterNode = NodeFactory.Roster(gst)
            .WithCosts(NodeFactory.Cost("pts", "pts", 0m));
        var rosterTree = SourceTree.CreateForRoot(rosterNode);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [rosterTree], catalogueCompilation);

        // act: replace roster tree (simulates roster edit)
        var newRosterNode = rosterNode.WithName("edited");
        var newTree = rosterTree.WithRoot(newRosterNode);
        var updatedCompilation = rosterCompilation.ReplaceSourceTree(rosterTree, newTree);

        // assert: catalogue symbols are still same references
        updatedCompilation.HasReferences.Should().BeTrue();
        var gsSym = catalogueCompilation.GlobalNamespace.RootCatalogue;
        var updatedGsSym = updatedCompilation.GlobalNamespace.RootCatalogue;
        ReferenceEquals(gsSym, updatedGsSym).Should().BeTrue(
            "catalogue symbols should be reused after roster edit");
    }

    [Fact]
    public void SymbolKey_resolves_catalogue_entry_from_roster_compilation()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo")
            .AddCostTypes(NodeFactory.CostType("pts", 0m));
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry"));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // Force-complete catalogue to bind symbols
        var catSymbol = catalogueCompilation.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem);
        var entrySymbol = catSymbol.SharedSelectionEntryContainers.Single();
        var key = SymbolKey.Create(entrySymbol);

        // act: resolve in roster compilation
        var rosterNode = NodeFactory.Roster(gst);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(rosterNode)],
            catalogueCompilation);
        var resolution = rosterCompilation.ResolveSymbolKey(key);

        // assert
        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        ReferenceEquals(resolution.Symbol, entrySymbol).Should().BeTrue(
            "resolved symbol should be same object from catalogue compilation");
    }

    [Fact]
    public void MultiRoster_share_catalogue_compilation()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo")
            .AddCostTypes(NodeFactory.CostType("pts", 0m));
        var cat = NodeFactory.Catalogue(gst, "bar");
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // act: create two roster compilations sharing same catalogue
        var roster1 = NodeFactory.Roster(gst).WithName("Roster 1");
        var roster2 = NodeFactory.Roster(gst).WithName("Roster 2");
        var rosterCompilation1 = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster1)], catalogueCompilation);
        var rosterCompilation2 = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster2)], catalogueCompilation);

        // assert: both see same catalogue symbols
        var cat1 = rosterCompilation1.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem);
        var cat2 = rosterCompilation2.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem);
        ReferenceEquals(cat1, cat2).Should().BeTrue(
            "both roster compilations should share same catalogue symbol");

        // Each has its own roster
        rosterCompilation1.GlobalNamespace.Rosters.Single()
            .GetDeclaration().Name.Should().Be("Roster 1");
        rosterCompilation2.GlobalNamespace.Rosters.Single()
            .GetDeclaration().Name.Should().Be("Roster 2");
    }

    [Fact]
    public void MultiRoster_edits_are_independent()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
        ]);
        var roster1 = NodeFactory.Roster(gst).WithName("R1");
        var roster1Tree = SourceTree.CreateForRoot(roster1);
        var rosterCompilation1 = WhamCompilation.CreateRosterCompilation(
            [roster1Tree], catalogueCompilation);
        var roster2 = NodeFactory.Roster(gst).WithName("R2");
        var rosterCompilation2 = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster2)], catalogueCompilation);

        // act: edit roster 1
        var editedRoster1 = roster1.WithName("R1-edited");
        var editedCompilation1 = rosterCompilation1.ReplaceSourceTree(
            roster1Tree, roster1Tree.WithRoot(editedRoster1));

        // assert: roster 2 unchanged
        editedCompilation1.GlobalNamespace.Rosters.Single()
            .GetDeclaration().Name.Should().Be("R1-edited");
        rosterCompilation2.GlobalNamespace.Rosters.Single()
            .GetDeclaration().Name.Should().Be("R2");
    }

    [Fact]
    public void Invariant_rejects_catalogue_trees_in_roster_compilation()
    {
        var gst = NodeFactory.Gamesystem("foo");
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
        ]);

        // Try to sneak a catalogue tree into a roster compilation
        var cat = NodeFactory.Catalogue(gst, "bad");
        var act = () => WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(cat)], catalogueCompilation);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*roster*only roster*");
    }

    [Fact]
    public void Invariant_rejects_chained_references()
    {
        var gst = NodeFactory.Gamesystem("foo");
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
        ]);
        var roster = NodeFactory.Roster(gst);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Try to create a compilation referencing a roster compilation
        var act = () => WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(NodeFactory.Roster(gst))], rosterCompilation);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must not itself have references*");
    }

    [Fact]
    public void GetDiagnostics_aggregates_reference_diagnostics()
    {
        // arrange: create catalogue with a bad entry link
        var gst = NodeFactory.Gamesystem("foo");
        var invalidEntry = NodeFactory.SelectionEntry("entry");
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddEntryLinks(NodeFactory.EntryLink(invalidEntry));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // act: create roster compilation and get all diagnostics
        var roster = NodeFactory.Roster(gst);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);
        var diagnostics = rosterCompilation.GetDiagnostics();

        // assert: catalogue diagnostic is visible from roster compilation
        diagnostics.Should().ContainSingle()
            .Which.Id.Should().Be("WHAM0006");
    }

    [Fact]
    public void FindSourceTree_finds_trees_in_referenced_compilation()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var gstTree = SourceTree.CreateForRoot(gst);
        var catalogueCompilation = WhamCompilation.Create([gstTree]);
        var roster = NodeFactory.Roster(gst);
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // act
        var found = rosterCompilation.FindSourceTree(gst);

        // assert
        found.Should().BeSameAs(gstTree);
    }

    [Fact]
    public void GetConstraintDiagnostics_aggregates_reference_constraint_diagnostics()
    {
        // arrange: create a standalone compilation with catalogue + roster.
        // The standalone compilation's constraint diagnostics (if any) should be
        // visible from a roster compilation that references it.
        var gst = NodeFactory.Gamesystem("foo")
            .AddCostTypes(NodeFactory.CostType("pts", 0m))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry));
        var cat = NodeFactory.Catalogue(gst, "bar");
        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst))
            .WithCosts(NodeFactory.Cost("pts", "pts", 0m));
        var standaloneCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
            SourceTree.CreateForRoot(roster),
        ]);
        var standaloneConstraints = standaloneCompilation.GetConstraintDiagnostics();

        // act: create a roster compilation referencing the standalone compilation
        var newRoster = NodeFactory.Roster(gst)
            .WithCosts(NodeFactory.Cost("pts", "pts", 0m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(newRoster)], standaloneCompilation);
        var rosterConstraints = rosterCompilation.GetConstraintDiagnostics();

        // assert: roster compilation's constraint diagnostics include the reference's
        rosterConstraints.Length.Should().BeGreaterThanOrEqualTo(standaloneConstraints.Length);
        foreach (var diag in standaloneConstraints)
        {
            rosterConstraints.Should().Contain(diag);
        }
    }
}
