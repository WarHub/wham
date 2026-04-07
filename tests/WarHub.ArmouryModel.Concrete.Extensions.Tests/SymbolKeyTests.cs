using FluentAssertions;
using Phalanx.SampleDataset;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class SymbolKeyTests
{
    #region Round-trip: same compilation

    [Fact]
    public void Catalogue_symbol_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);

        var key = SymbolKey.Create(catalogue);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(catalogue);
    }

    [Fact]
    public void Root_container_entry_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var rootEntry = catalogue.RootContainerEntries.First();

        var key = SymbolKey.Create(rootEntry);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(rootEntry);
    }

    [Fact]
    public void Nested_container_entry_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var rootEntry = catalogue.RootContainerEntries
            .OfType<ISelectionEntryContainerSymbol>()
            .FirstOrDefault(x => x.ChildSelectionEntries.Length > 0);
        if (rootEntry is null)
        {
            // Try shared entries if no root entry has children.
            rootEntry = catalogue.SharedSelectionEntryContainers
                .FirstOrDefault(x => x.ChildSelectionEntries.Length > 0);
        }
        rootEntry.Should().NotBeNull("Sample dataset should have a nested entry");
        var nestedEntry = rootEntry!.ChildSelectionEntries.First();

        var key = SymbolKey.Create(nestedEntry);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(nestedEntry);
    }

    [Fact]
    public void Resource_definition_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var gamesystem = compilation.GlobalNamespace.RootCatalogue;
        var resDef = gamesystem.ResourceDefinitions.First();

        var key = SymbolKey.Create(resDef);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(resDef);
    }

    [Fact]
    public void Resource_entry_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        // Find any resource entry with a non-null ID - search shared entries too.
        var entry = catalogue.SharedSelectionEntryContainers
            .SelectMany(x => x.Resources)
            .Concat(catalogue.RootContainerEntries
                .OfType<ISelectionEntryContainerSymbol>()
                .SelectMany(x => x.Resources))
            .Concat(catalogue.RootResourceEntries)
            .First(r => r.Id is not null);

        var key = SymbolKey.Create(entry);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(entry);
    }

    [Fact]
    public void Roster_symbol_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var roster = compilation.GlobalNamespace.Rosters.First();

        var key = SymbolKey.Create(roster);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(roster);
    }

    [Fact]
    public void Roster_force_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var force = compilation.GlobalNamespace.Rosters.First().Forces.First();

        var key = SymbolKey.Create(force);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(force);
    }

    [Fact]
    public void Roster_selection_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var selection = compilation.GlobalNamespace.Rosters.First()
            .Forces.First()
            .Selections.First();

        var key = SymbolKey.Create(selection);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(selection);
    }

    [Fact]
    public void Entry_link_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var entryLink = catalogue.RootContainerEntries
            .OfType<ISelectionEntryContainerSymbol>()
            .First(x => x.IsReference);

        var key = SymbolKey.Create(entryLink);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(entryLink);
    }

    #endregion

    #region Round-trip: across recompilation

    [Fact]
    public void Catalogue_entry_resolves_in_recompiled_compilation()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var rootEntry = catalogue.RootContainerEntries.First();

        var key = SymbolKey.Create(rootEntry);

        // Rebuild compilation from the same source trees.
        var newCompilation = WhamCompilation.Create(compilation.SourceTrees);
        var resolution = key.Resolve(newCompilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().NotBeSameAs(rootEntry, "New compilation has new symbol instances");
        resolution.Symbol!.Id.Should().Be(rootEntry.Id);
        resolution.Symbol.Kind.Should().Be(rootEntry.Kind);
    }

    [Fact]
    public void Roster_force_resolves_in_recompiled_compilation()
    {
        var compilation = CreateSampleCompilation();
        var force = compilation.GlobalNamespace.Rosters.First().Forces.First();

        var key = SymbolKey.Create(force);

        var newCompilation = WhamCompilation.Create(compilation.SourceTrees);
        var resolution = key.Resolve(newCompilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().NotBeSameAs(force);
        resolution.Symbol!.Id.Should().Be(force.Id);
    }

    #endregion

    #region Edge cases

    [Fact]
    public void Resolve_in_empty_compilation_returns_Missing()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var key = SymbolKey.Create(catalogue);

        var emptyCompilation = WhamCompilation.Create();
        var resolution = key.Resolve(emptyCompilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Missing);
        resolution.Symbol.Should().BeNull();
    }

    [Fact]
    public void Resolve_after_removing_source_tree_returns_Missing()
    {
        var compilation = CreateSampleCompilation();
        var roster = compilation.GlobalNamespace.Rosters.First();
        var key = SymbolKey.Create(roster);

        // Remove the roster's source tree.
        var rosterTree = compilation.SourceTrees.First(t =>
            t.GetRoot() is RosterNode);
        var withoutRoster = compilation.ReplaceSourceTree(rosterTree, null);
        var resolution = key.Resolve(withoutRoster);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Missing);
    }

    [Fact]
    public void SymbolKey_with_default_values_resolves_to_Missing()
    {
        var compilation = CreateSampleCompilation();
        var key = default(SymbolKey);

        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Missing);
    }

    [Fact]
    public void SymbolKey_equality_same_fields_are_equal()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);

        var key1 = SymbolKey.Create(catalogue);
        var key2 = SymbolKey.Create(catalogue);

        key1.Should().Be(key2);
    }

    [Fact]
    public void SymbolKey_equality_different_symbols_are_not_equal()
    {
        var compilation = CreateSampleCompilation();
        var catalogues = compilation.GlobalNamespace.Catalogues;
        var cat1 = catalogues.First(x => x.IsGamesystem);
        var cat2 = catalogues.First(x => !x.IsGamesystem);

        var key1 = SymbolKey.Create(cat1);
        var key2 = SymbolKey.Create(cat2);

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void Create_captures_correct_fields_for_catalogue_entry()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var rootEntry = catalogue.RootContainerEntries.First();

        var key = SymbolKey.Create(rootEntry);

        key.Kind.Should().Be(SymbolKind.ContainerEntry);
        key.SymbolId.Should().Be(rootEntry.Id);
        key.ContainingModuleId.Should().Be(catalogue.Id);
        key.ContainingEntryId.Should().BeNull("Root entries have no containing entry");
    }

    [Fact]
    public void Create_captures_correct_fields_for_nested_entry()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        var rootEntry = catalogue.SharedSelectionEntryContainers
            .First(x => x.ChildSelectionEntries.Length > 0);
        var nestedEntry = rootEntry.ChildSelectionEntries.First();

        var key = SymbolKey.Create(nestedEntry);

        key.Kind.Should().Be(SymbolKind.ContainerEntry);
        key.SymbolId.Should().Be(nestedEntry.Id);
        key.ContainingModuleId.Should().Be(catalogue.Id);
        key.ContainingEntryId.Should().Be(rootEntry.Id, "Nested entry's containing entry is the parent");
    }

    [Fact]
    public void Create_captures_correct_fields_for_roster_selection()
    {
        var compilation = CreateSampleCompilation();
        var roster = compilation.GlobalNamespace.Rosters.First();
        var force = roster.Forces.First();
        var selection = force.Selections.First();

        var key = SymbolKey.Create(selection);

        key.Kind.Should().Be(SymbolKind.Container);
        key.SymbolId.Should().Be(selection.Id);
        key.ContainingModuleId.Should().Be(roster.Id);
        key.ContainingEntryId.Should().Be(force.Id, "Selection's containing entry is the force");
    }

    #endregion

    #region Ambiguous key resolution

    [Fact]
    public void Duplicate_ids_disambiguated_by_containing_entry_id()
    {
        var gst = NodeFactory.Gamesystem("foo");
        var duplicateId = "shared-child-id";
        var parent1 = NodeFactory.SelectionEntry("parent1")
            .AddSelectionEntries(
                NodeFactory.SelectionEntry("child", id: duplicateId));
        var parent2 = NodeFactory.SelectionEntry("parent2")
            .AddSelectionEntries(
                NodeFactory.SelectionEntry("child", id: duplicateId));
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSharedSelectionEntries(parent1, parent2);
        var compilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        var catalogue = compilation.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        var entry1 = catalogue.SharedSelectionEntryContainers[0].ChildSelectionEntries[0];
        var entry2 = catalogue.SharedSelectionEntryContainers[1].ChildSelectionEntries[0];

        // Both children have same ID but different parents
        entry1.Id.Should().Be(duplicateId);
        entry2.Id.Should().Be(duplicateId);

        // Keys should differ by ContainingEntryId
        var key1 = SymbolKey.Create(entry1);
        var key2 = SymbolKey.Create(entry2);
        key1.ContainingEntryId.Should().NotBe(key2.ContainingEntryId);

        // Both should resolve correctly via disambiguation
        var resolution1 = key1.Resolve(compilation);
        resolution1.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution1.Symbol.Should().BeSameAs(entry1);

        var resolution2 = key2.Resolve(compilation);
        resolution2.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution2.Symbol.Should().BeSameAs(entry2);
    }

    [Fact]
    public void Duplicate_ids_without_disambiguation_returns_Ambiguous()
    {
        var gst = NodeFactory.Gamesystem("foo");
        var duplicateId = "shared-root-id";
        // Two root entries with same ID — no ContainingEntryId to disambiguate
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSelectionEntries(
                NodeFactory.SelectionEntry("entry1", id: duplicateId),
                NodeFactory.SelectionEntry("entry2", id: duplicateId));
        var compilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // Both entries are root-level, so ContainingEntryId is null for both
        var entry1 = compilation.GlobalNamespace.Catalogues
            .Single(x => !x.IsGamesystem).RootContainerEntries[0];
        var key = SymbolKey.Create(entry1);
        key.ContainingEntryId.Should().BeNull();

        var resolution = key.Resolve(compilation);
        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Ambiguous);
        resolution.CandidateSymbols.Should().HaveCount(2);
    }

    #endregion

    #region Simple compilation tests

    [Fact]
    public void Round_trip_with_simple_catalogue()
    {
        var gst = NodeFactory.Gamesystem("foo");
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry));
        var compilation = WhamCompilation.Create(
            [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat)]);
        compilation.GetDiagnostics().Should().BeEmpty();

        var catalogue = compilation.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        var entrySymbol = catalogue.RootContainerEntries.Single();

        var key = SymbolKey.Create(entrySymbol);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(entrySymbol);
    }

    [Fact]
    public void Round_trip_with_simple_roster()
    {
        var gst = NodeFactory.Gamesystem("foo")
            .AddForceEntries(NodeFactory.ForceEntry("detachment").Tee(out var forceEntry));
        var selEntry = NodeFactory.SelectionEntry("unit").Tee(out var selEntryNode);
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSelectionEntries(selEntry);
        var roster = NodeFactory.Roster(gst)
            .AddForces(
                NodeFactory.Force(forceEntry, gst).Tee(out var forceNode)
                    .AddSelections(
                        NodeFactory.Selection(selEntryNode, selEntryNode.Id!)));
        var compilation = WhamCompilation.Create(
            [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat), SourceTree.CreateForRoot(roster)]);

        var rosterSymbol = compilation.GlobalNamespace.Rosters.Single();
        var forceSymbol = rosterSymbol.Forces.Single();
        var selectionSymbol = forceSymbol.Selections.Single();

        // Round-trip roster
        var rosterKey = SymbolKey.Create(rosterSymbol);
        rosterKey.Resolve(compilation).Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        rosterKey.Resolve(compilation).Symbol.Should().BeSameAs(rosterSymbol);

        // Round-trip force
        var forceKey = SymbolKey.Create(forceSymbol);
        forceKey.Resolve(compilation).Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        forceKey.Resolve(compilation).Symbol.Should().BeSameAs(forceSymbol);

        // Round-trip selection
        var selectionKey = SymbolKey.Create(selectionSymbol);
        selectionKey.Resolve(compilation).Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        selectionKey.Resolve(compilation).Symbol.Should().BeSameAs(selectionSymbol);
    }

    #endregion

    #region Category link and selection member tests

    [Fact]
    public void Selection_entry_category_link_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var catalogue = compilation.GlobalNamespace.Catalogues.First(x => !x.IsGamesystem);
        // Find a selection entry that has category links.
        var entryWithCategories = catalogue.RootContainerEntries
            .OfType<ISelectionEntryContainerSymbol>()
            .Concat(catalogue.SharedSelectionEntryContainers)
            .FirstOrDefault(x => x.Categories.Length > 0);
        if (entryWithCategories is null)
            return; // Skip if sample data has no category links on entries
        var categoryLink = entryWithCategories.Categories.First();

        var key = SymbolKey.Create(categoryLink);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(categoryLink);
    }

    [Fact]
    public void Roster_selection_cost_has_null_id_and_is_not_indexable()
    {
        // CostNode does not implement IIdentifiableNode, so cost symbols have null ID
        // and cannot be resolved via SymbolKey. This is by design.
        var compilation = CreateSampleCompilation();
        var selection = compilation.GlobalNamespace.Rosters.First()
            .Forces.First()
            .Selections.FirstOrDefault(s => s.Costs.Length > 0);
        if (selection is null)
            return;
        var cost = selection.Costs.First();

        cost.Id.Should().BeNull("CostNode does not have an ID");
    }

    [Fact]
    public void Roster_selection_category_round_trips()
    {
        var compilation = CreateSampleCompilation();
        var selection = compilation.GlobalNamespace.Rosters.First()
            .Forces.First()
            .Selections.FirstOrDefault(s => s.Categories.Length > 0);
        if (selection is null)
            return; // Skip if sample data has no selection categories
        var category = selection.Categories.First();

        var key = SymbolKey.Create(category);
        var resolution = key.Resolve(compilation);

        resolution.Kind.Should().Be(SymbolKeyResolutionKind.Resolved);
        resolution.Symbol.Should().BeSameAs(category);
    }

    #endregion

    #region Helpers

    private static WhamCompilation CreateSampleCompilation()
    {
        var xmlWorkspace = SampleDataResources.CreateXmlWorkspace();
        return WhamCompilation.Create(
#pragma warning disable xUnit1031
            xmlWorkspace.Documents.Select(x => SourceTree.CreateForRoot(x.GetRootAsync().Result!)).ToImmutableArray());
#pragma warning restore xUnit1031
    }

    #endregion
}
