using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class ReentrancyDetectionTests
{
    [Fact]
    public void Compilation_with_entry_links_succeeds()
    {
        // Self-completing properties should handle entry links without reentrancy issues.
        var gst = NodeFactory.Gamesystem("gst");
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry))
            .AddEntryLinks(
                NodeFactory.EntryLink(entry));
        var compilation = WhamCompilation.Create(
            [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat)]);

        var diagnostics = compilation.GetDiagnostics();

        diagnostics.Should().BeEmpty();
        var catalogue = compilation.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        catalogue.RootContainerEntries.Should().ContainSingle()
            .Which.ReferencedEntry
            .Should().Be(catalogue.SharedSelectionEntryContainers.Single());
    }

    [Fact]
    public void Concurrent_ForceComplete_does_not_deadlock()
    {
        // Multiple threads hitting ForceComplete on the same roster compilation
        // should not deadlock. The NotePartComplete/SpinWaitComplete pattern
        // ensures only one thread performs each phase while others wait.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry)
                .AddConstraints(
                    NodeFactory.Constraint(field: "forces", scope: "roster", value: 1, type: ConstraintKind.Minimum),
                    NodeFactory.Constraint(field: "forces", scope: "roster", value: 3, type: ConstraintKind.Maximum)));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("unit", id: "unit-1").Tee(out var unitEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 10m))
                    .AddConstraints(
                        NodeFactory.Constraint(field: "selections", scope: "parent", value: 1, type: ConstraintKind.Minimum)))
            .AddEntryLinks(NodeFactory.EntryLink(unitEntry));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);
        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(unitEntry, entryId: unitEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 10m))))
            .WithCosts(NodeFactory.Cost(ptsCostType, 0m))
            .WithCostLimits(NodeFactory.CostLimit(ptsCostType, 100m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Hit ForceComplete from multiple threads simultaneously
        const int threadCount = 8;
        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            var diags = rosterCompilation.GetDiagnostics();
            var constraintDiags = rosterCompilation.GetConstraintDiagnostics();
            return (diags, constraintDiags);
        })).ToArray();

        // Hard timeout to detect deadlock
        var completed = Task.WaitAll(tasks, TimeSpan.FromSeconds(30));
        completed.Should().BeTrue("ForceComplete should not deadlock under concurrent access");

        // All threads should see the same diagnostics
        var referenceResult = tasks[0].Result;
        foreach (var task in tasks.Skip(1))
        {
            task.Result.diags.Length.Should().Be(referenceResult.diags.Length);
            task.Result.constraintDiags.Length.Should().Be(referenceResult.constraintDiags.Length);
        }
    }

    [Fact]
    public void Symbol_layer_cost_access_works_during_constraint_evaluation()
    {
        // ConstraintEvaluator uses CostSymbol.Type (a [Bound] property) to aggregate costs.
        // This verifies that by CheckConstraints phase, member symbols have completed
        // CheckReferences and their bound cost type properties are resolved.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("unit", id: "unit-1").Tee(out var unitEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 50m)));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // Roster with a selection that has costs, and a cost limit that should be exceeded.
        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(unitEntry, entryId: unitEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 150m))))
            .WithCosts(NodeFactory.Cost(ptsCostType, 0m))
            .WithCostLimits(NodeFactory.CostLimit(ptsCostType, 100m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // This exercises the Symbol-layer code path in AggregateCostsRecursive
        // (which now uses CostSymbol.Type instead of CostNode.TypeId)
        var constraintDiags = rosterCompilation.GetConstraintDiagnostics();

        // Verify cost limit violation is detected (proving the Symbol-layer
        // cost aggregation resolved the type correctly)
        constraintDiags.Should().Contain(d => d.Id == "WHAM0103",
            "cost limit exceeded diagnostic should be reported via Symbol-layer cost aggregation");
    }

    [Fact]
    public void EffectiveSourceEntry_accessible_on_roster_selections_after_compilation()
    {
        // After compilation, EffectiveSourceEntry should be accessible without
        // deadlock or reentrancy issues. The EffectiveEntries phase populates
        // the cache before CheckConstraints accesses it.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("unit", id: "unit-1").Tee(out var unitEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 10m)));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(unitEntry, entryId: unitEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 10m))))
            .WithCosts(NodeFactory.Cost(ptsCostType, 0m))
            .WithCostLimits(NodeFactory.CostLimit(ptsCostType, 100m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Force full completion
        _ = rosterCompilation.GetConstraintDiagnostics();

        // Access EffectiveSourceEntry on all selections — should not deadlock
        var rosterSymbol = rosterCompilation.GlobalNamespace.Rosters.Single();
        foreach (var force in rosterSymbol.Forces)
        {
            foreach (var sel in force.Selections)
            {
                var effective = sel.EffectiveSourceEntry;
                effective.Should().NotBeNull();
                effective.Name.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Fact]
    public void Bound_category_SourceEntry_resolves_during_modifier_evaluation()
    {
        // During modifier evaluation (EffectiveEntries phase), accessing
        // CategorySymbol.SourceEntry (a [Bound] property) on roster selections
        // should not cause reentrancy. The catalogue symbols are fully bound
        // before roster EffectiveEntries starts.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry))
            .AddCategoryEntries(NodeFactory.CategoryEntry("HQ").Tee(out var hqCatEntry));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("commander", id: "cmd-1").Tee(out var cmdEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 50m))
                    .AddCategoryLinks(NodeFactory.CategoryLink(hqCatEntry)));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(cmdEntry, entryId: cmdEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 50m))
                        .AddCategories(NodeFactory.Category(hqCatEntry))))
            .WithCosts(NodeFactory.Cost(ptsCostType, 0m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Force full completion — exercises modifier evaluation which
        // accesses cat.SourceEntry on roster selection categories
        _ = rosterCompilation.GetConstraintDiagnostics();

        var rosterSymbol = rosterCompilation.GlobalNamespace.Rosters.Single();
        var selection = rosterSymbol.Forces.Single().Selections.Single();

        // CategorySymbol.SourceEntry should be resolved to the catalogue category
        var category = selection.Categories.Single();
        category.SourceEntry.Should().NotBeNull();
        category.SourceEntry.Should().NotBeAssignableTo<IErrorSymbol>(
            "category SourceEntry should resolve via catalogue binding without reentrancy");
        category.SourceEntry.Id.Should().Be(hqCatEntry.Id);
    }

    [Fact]
    public void RosterCostSymbol_Limit_and_CostType_resolve_for_cost_limit_validation()
    {
        // ValidateCostLimits now uses _roster.Costs (RosterCostSymbol) instead of
        // _roster.Declaration.CostLimits. This test verifies that RosterCostSymbol.Limit
        // and RosterCostSymbol.CostType are correctly resolved during CheckConstraints.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(
                NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType),
                NodeFactory.CostType("PL", 0m).Tee(out var plCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("unit", id: "unit-1").Tee(out var unitEntry)
                    .AddCosts(
                        NodeFactory.Cost(ptsCostType, 10m),
                        NodeFactory.Cost(plCostType, 2m)));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // Roster with costs within pts limit but exceeding PL limit
        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(unitEntry, entryId: unitEntry.Id)
                        .AddCosts(
                            NodeFactory.Cost(ptsCostType, 10m),
                            NodeFactory.Cost(plCostType, 20m))))
            .WithCosts(
                NodeFactory.Cost(ptsCostType, 0m),
                NodeFactory.Cost(plCostType, 0m))
            .WithCostLimits(
                NodeFactory.CostLimit(ptsCostType, 100m),
                NodeFactory.CostLimit(plCostType, 10m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Verify RosterCostSymbol properties are correctly populated
        var rosterSymbol = rosterCompilation.GlobalNamespace.Rosters.Single();
        rosterSymbol.Costs.Should().HaveCount(2);
        var ptsCost = rosterSymbol.Costs.First(c => c.CostType.Id == ptsCostType.Id);
        ptsCost.Limit.Should().Be(100m);
        ptsCost.CostType.Should().NotBeAssignableTo<IErrorSymbol>();

        var plCost = rosterSymbol.Costs.First(c => c.CostType.Id == plCostType.Id);
        plCost.Limit.Should().Be(10m);
        plCost.CostType.Should().NotBeAssignableTo<IErrorSymbol>();

        // Only PL should have a cost limit violation
        var diags = rosterCompilation.GetConstraintDiagnostics();
        diags.Should().ContainSingle(d => d.Id == "WHAM0103",
            "only PL cost type should exceed its limit (20 > 10)");
    }

    [Fact]
    public void Category_counting_uses_symbol_layer_CategorySymbol_SourceEntry()
    {
        // ValidateCategoryConstraints now uses sel.Categories (CategorySymbol) and
        // accesses cat.SourceEntry.Id instead of sel.Declaration.Categories and
        // cat.EntryId. This test verifies that CategorySymbol.SourceEntry resolves
        // correctly and category information is accessible through the Symbol layer.
        var gst = NodeFactory.Gamesystem("gst")
            .AddCostTypes(NodeFactory.CostType("pts", 0m).Tee(out var ptsCostType))
            .AddForceEntries(NodeFactory.ForceEntry("det").Tee(out var forceEntry))
            .AddCategoryEntries(
                NodeFactory.CategoryEntry("HQ").Tee(out var hqCatEntry),
                NodeFactory.CategoryEntry("Troops").Tee(out var troopsCatEntry));
        var cat = NodeFactory.Catalogue(gst, "cat")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("commander", id: "cmd-1").Tee(out var cmdEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 50m))
                    .AddCategoryLinks(NodeFactory.CategoryLink(hqCatEntry)),
                NodeFactory.SelectionEntry("squad", id: "sq-1").Tee(out var sqEntry)
                    .AddCosts(NodeFactory.Cost(ptsCostType, 10m))
                    .AddCategoryLinks(NodeFactory.CategoryLink(troopsCatEntry)));
        var catalogueCompilation = WhamCompilation.Create([
            SourceTree.CreateForRoot(gst),
            SourceTree.CreateForRoot(cat),
        ]);

        // Roster with selections in different categories
        var roster = NodeFactory.Roster(gst)
            .AddForces(NodeFactory.Force(forceEntry, gst)
                .AddSelections(
                    NodeFactory.Selection(cmdEntry, entryId: cmdEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 50m))
                        .AddCategories(NodeFactory.Category(hqCatEntry)),
                    NodeFactory.Selection(sqEntry, entryId: sqEntry.Id)
                        .AddCosts(NodeFactory.Cost(ptsCostType, 10m))
                        .AddCategories(NodeFactory.Category(troopsCatEntry))))
            .WithCosts(NodeFactory.Cost(ptsCostType, 0m));
        var rosterCompilation = WhamCompilation.CreateRosterCompilation(
            [SourceTree.CreateForRoot(roster)], catalogueCompilation);

        // Force full compilation through CheckConstraints
        _ = rosterCompilation.GetConstraintDiagnostics();

        // Verify CategorySymbol.SourceEntry is resolved for all selection categories
        var rosterSymbol = rosterCompilation.GlobalNamespace.Rosters.Single();
        var selections = rosterSymbol.Forces.Single().Selections;
        selections.Should().HaveCount(2);

        foreach (var sel in selections)
        {
            var category = sel.Categories.Single();
            category.SourceEntry.Should().NotBeAssignableTo<IErrorSymbol>(
                "category SourceEntry should resolve to catalogue category entry");
            category.SourceEntry.Id.Should().NotBeNullOrEmpty(
                "resolved category entry should have a valid Id");
        }

        var hqCategory = selections[0].Categories.Single();
        hqCategory.SourceEntry.Id.Should().Be(hqCatEntry.Id);

        var troopsCategory = selections[1].Categories.Single();
        troopsCategory.SourceEntry.Id.Should().Be(troopsCatEntry.Id);
    }
}
