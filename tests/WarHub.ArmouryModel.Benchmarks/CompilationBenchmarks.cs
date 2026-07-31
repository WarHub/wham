using BenchmarkDotNet.Attributes;
using Phalanx.SampleDataset;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Benchmarks;

/// <summary>
/// Measures the cost of compilation creation and roster edits, comparing:
/// - Full rebuild (monolithic compilation with catalogues + roster)
/// - Incremental (catalogue compilation reused, only roster compilation rebuilt)
/// </summary>
[MemoryDiagnoser]
public class CompilationBenchmarks
{
    // ── Shared state ────────────────────────────────────────────────────

    // Sample dataset (loaded from embedded XML)
    private ImmutableArray<SourceTree> _sampleCatalogueTrees;
    private ImmutableArray<SourceTree> _sampleRosterTrees;
    private ImmutableArray<SourceTree> _sampleAllTrees;

    // Synthetic scaled dataset
    private ImmutableArray<SourceTree> _syntheticCatalogueTrees;
    private ImmutableArray<SourceTree> _syntheticRosterTrees;
    private ImmutableArray<SourceTree> _syntheticAllTrees;

    // Pre-built catalogue compilations for incremental benchmarks
    private WhamCompilation _sampleCatalogueCompilation = null!;
    private WhamCompilation _syntheticCatalogueCompilation = null!;

    // Pre-built compilations for roster edit benchmarks
    private WhamCompilation _sampleFullCompilation = null!;
    private WhamCompilation _sampleIncrementalCompilation = null!;
    private WhamCompilation _syntheticFullCompilation = null!;
    private WhamCompilation _syntheticIncrementalCompilation = null!;

    // Edited roster trees for replacement
    private SourceTree _sampleRosterTree = null!;
    private SourceTree _sampleEditedRosterTree = null!;
    private SourceTree _syntheticRosterTree = null!;
    private SourceTree _syntheticEditedRosterTree = null!;

    [GlobalSetup]
    public void Setup()
    {
        // ── Sample dataset from embedded XML ──
        var workspace = SampleDataResources.CreateXmlWorkspace();
        var allTrees = workspace.Documents
            .Select(x => SourceTree.CreateForRoot(x.GetRootAsync().Result!))
            .ToImmutableArray();

        _sampleCatalogueTrees = allTrees.Where(t => t.GetRoot() is not RosterNode).ToImmutableArray();
        _sampleRosterTrees = allTrees.Where(t => t.GetRoot() is RosterNode).ToImmutableArray();
        _sampleAllTrees = allTrees;

        // Pre-build catalogue compilation and force-complete it
        _sampleCatalogueCompilation = WhamCompilation.Create(_sampleCatalogueTrees);
        _sampleCatalogueCompilation.GetDiagnostics();

        // Pre-build compilations for edit benchmarks
        _sampleFullCompilation = WhamCompilation.Create(_sampleAllTrees);
        _sampleFullCompilation.GetDiagnostics();
        _sampleIncrementalCompilation = WhamCompilation.CreateRosterCompilation(
            _sampleRosterTrees, _sampleCatalogueCompilation);
        _sampleIncrementalCompilation.GetDiagnostics();

        _sampleRosterTree = _sampleRosterTrees[0];
        var sampleRosterNode = (RosterNode)_sampleRosterTree.GetRoot();
        _sampleEditedRosterTree = _sampleRosterTree.WithRoot(
            sampleRosterNode.WithName(sampleRosterNode.Name + " edited"));

        // ── Synthetic scaled dataset ──
        BuildSyntheticData(entryCount: 50, profilesPerEntry: 3, rulesPerEntry: 2);
    }

    private void BuildSyntheticData(int entryCount, int profilesPerEntry, int rulesPerEntry)
    {
        var gst = NodeFactory.Gamesystem("synth-gs")
            .AddCostTypes(
                NodeFactory.CostType("pts", 0m),
                NodeFactory.CostType("pl", 0m))
            .AddProfileTypes(
                new ProfileTypeCore
                {
                    Id = "unit-profile",
                    Name = "Unit",
                    CharacteristicTypes =
                    [
                        new CharacteristicTypeCore { Id = "ct-m", Name = "M" },
                        new CharacteristicTypeCore { Id = "ct-ws", Name = "WS" },
                        new CharacteristicTypeCore { Id = "ct-bs", Name = "BS" },
                        new CharacteristicTypeCore { Id = "ct-s", Name = "S" },
                        new CharacteristicTypeCore { Id = "ct-t", Name = "T" },
                        new CharacteristicTypeCore { Id = "ct-w", Name = "W" },
                    ]
                }.ToNode())
            .AddCategoryEntries(
                NodeFactory.CategoryEntry("cat-hq", "HQ"),
                NodeFactory.CategoryEntry("cat-troops", "Troops"),
                NodeFactory.CategoryEntry("cat-elites", "Elites"))
            .AddForceEntries(
                new ForceEntryCore
                {
                    Id = "fe-patrol",
                    Name = "Patrol",
                    CategoryLinks =
                    [
                        new CategoryLinkCore { Id = "cl-hq", Name = "HQ", TargetId = "cat-hq" },
                        new CategoryLinkCore { Id = "cl-troops", Name = "Troops", TargetId = "cat-troops" },
                        new CategoryLinkCore { Id = "cl-elites", Name = "Elites", TargetId = "cat-elites" },
                    ]
                }.ToNode());

        // Create catalogue with many entries
        var entries = new List<SelectionEntryNode>();
        for (int i = 0; i < entryCount; i++)
        {
            var profiles = new List<ProfileCore>();
            for (int p = 0; p < profilesPerEntry; p++)
            {
                profiles.Add(new ProfileCore
                {
                    Id = $"prof-{i}-{p}",
                    Name = $"Profile {p} of Entry {i}",
                    TypeId = "unit-profile",
                    TypeName = "Unit",
                    Characteristics =
                    [
                        new CharacteristicCore { Name = "M", TypeId = "ct-m", Value = "6\"" },
                        new CharacteristicCore { Name = "WS", TypeId = "ct-ws", Value = "3+" },
                        new CharacteristicCore { Name = "BS", TypeId = "ct-bs", Value = "3+" },
                        new CharacteristicCore { Name = "S", TypeId = "ct-s", Value = "4" },
                        new CharacteristicCore { Name = "T", TypeId = "ct-t", Value = "4" },
                        new CharacteristicCore { Name = "W", TypeId = "ct-w", Value = "2" },
                    ]
                });
            }

            var rules = new List<RuleCore>();
            for (int r = 0; r < rulesPerEntry; r++)
            {
                rules.Add(new RuleCore
                {
                    Id = $"rule-{i}-{r}",
                    Name = $"Rule {r} of Entry {i}",
                    Description = $"Description for rule {r} on entry {i}."
                });
            }

            entries.Add(new SelectionEntryCore
            {
                Id = $"se-{i}",
                Name = $"Entry {i}",
                Type = i % 3 == 0 ? SelectionEntryKind.Unit
                     : i % 3 == 1 ? SelectionEntryKind.Model
                     : SelectionEntryKind.Upgrade,
                Costs =
                [
                    new CostCore { Name = "Points", TypeId = "pts", Value = 10m + i },
                    new CostCore { Name = "Power Level", TypeId = "pl", Value = 1m + i / 5m },
                ],
                Profiles = [.. profiles],
                Rules = [.. rules],
                Constraints =
                [
                    new ConstraintCore
                    {
                        Id = $"con-{i}-max",
                        Type = ConstraintKind.Maximum,
                        Value = 3,
                        Field = "selections",
                        Scope = "parent",
                    }
                ],
                CategoryLinks =
                [
                    new CategoryLinkCore
                    {
                        Id = $"cl-{i}",
                        Name = i % 2 == 0 ? "Troops" : "HQ",
                        TargetId = i % 2 == 0 ? "cat-troops" : "cat-hq",
                        Primary = true,
                    }
                ]
            }.ToNode());
        }

        var cat = NodeFactory.Catalogue(gst, "synth-cat");
        foreach (var entry in entries)
        {
            cat = cat.AddSelectionEntries(entry);
        }

        _syntheticCatalogueTrees = [SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat)];

        var rosterNode = NodeFactory.Roster(gst)
            .WithCosts(
                NodeFactory.Cost("pts", "pts", 0m),
                NodeFactory.Cost("pl", "pl", 0m));
        _syntheticRosterTrees = [SourceTree.CreateForRoot(rosterNode)];
        _syntheticAllTrees = _syntheticCatalogueTrees.AddRange(_syntheticRosterTrees);

        // Pre-build catalogue compilation and force-complete
        _syntheticCatalogueCompilation = WhamCompilation.Create(_syntheticCatalogueTrees);
        _syntheticCatalogueCompilation.GetDiagnostics();

        // Pre-build compilations for edit benchmarks
        _syntheticFullCompilation = WhamCompilation.Create(_syntheticAllTrees);
        _syntheticFullCompilation.GetDiagnostics();
        _syntheticIncrementalCompilation = WhamCompilation.CreateRosterCompilation(
            _syntheticRosterTrees, _syntheticCatalogueCompilation);
        _syntheticIncrementalCompilation.GetDiagnostics();

        _syntheticRosterTree = _syntheticRosterTrees[0];
        _syntheticEditedRosterTree = _syntheticRosterTree.WithRoot(
            rosterNode.WithName("edited"));
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Scenario 1: Create compilation from scratch and force-complete
    // ═══════════════════════════════════════════════════════════════════

    [Benchmark(Description = "Sample: Full rebuild (create + complete)")]
    [BenchmarkCategory("Create", "Sample")]
    public ImmutableArray<Diagnostic> Sample_FullRebuild_Create()
    {
        var compilation = WhamCompilation.Create(_sampleAllTrees);
        return compilation.GetDiagnostics();
    }

    [Benchmark(Description = "Sample: Incremental (create roster + complete)")]
    [BenchmarkCategory("Create", "Sample")]
    public ImmutableArray<Diagnostic> Sample_Incremental_Create()
    {
        var compilation = WhamCompilation.CreateRosterCompilation(
            _sampleRosterTrees, _sampleCatalogueCompilation);
        return compilation.GetDiagnostics();
    }

    [Benchmark(Description = "Synth50: Full rebuild (create + complete)")]
    [BenchmarkCategory("Create", "Synthetic")]
    public ImmutableArray<Diagnostic> Synth_FullRebuild_Create()
    {
        var compilation = WhamCompilation.Create(_syntheticAllTrees);
        return compilation.GetDiagnostics();
    }

    [Benchmark(Description = "Synth50: Incremental (create roster + complete)")]
    [BenchmarkCategory("Create", "Synthetic")]
    public ImmutableArray<Diagnostic> Synth_Incremental_Create()
    {
        var compilation = WhamCompilation.CreateRosterCompilation(
            _syntheticRosterTrees, _syntheticCatalogueCompilation);
        return compilation.GetDiagnostics();
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Scenario 2: Roster edit (replace tree + re-complete)
    // ═══════════════════════════════════════════════════════════════════

    [Benchmark(Description = "Sample: Full rebuild after edit")]
    [BenchmarkCategory("Edit", "Sample")]
    public ImmutableArray<Diagnostic> Sample_FullRebuild_Edit()
    {
        var updated = _sampleFullCompilation.ReplaceSourceTree(
            _sampleRosterTree, _sampleEditedRosterTree);
        return updated.GetDiagnostics();
    }

    [Benchmark(Description = "Sample: Incremental after edit")]
    [BenchmarkCategory("Edit", "Sample")]
    public ImmutableArray<Diagnostic> Sample_Incremental_Edit()
    {
        var updated = _sampleIncrementalCompilation.ReplaceSourceTree(
            _sampleRosterTree, _sampleEditedRosterTree);
        return updated.GetDiagnostics();
    }

    [Benchmark(Description = "Synth50: Full rebuild after edit")]
    [BenchmarkCategory("Edit", "Synthetic")]
    public ImmutableArray<Diagnostic> Synth_FullRebuild_Edit()
    {
        var updated = _syntheticFullCompilation.ReplaceSourceTree(
            _syntheticRosterTree, _syntheticEditedRosterTree);
        return updated.GetDiagnostics();
    }

    [Benchmark(Description = "Synth50: Incremental after edit")]
    [BenchmarkCategory("Edit", "Synthetic")]
    public ImmutableArray<Diagnostic> Synth_Incremental_Edit()
    {
        var updated = _syntheticIncrementalCompilation.ReplaceSourceTree(
            _syntheticRosterTree, _syntheticEditedRosterTree);
        return updated.GetDiagnostics();
    }
}
