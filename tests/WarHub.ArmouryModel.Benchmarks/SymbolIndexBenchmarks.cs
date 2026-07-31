using BenchmarkDotNet.Attributes;
using Phalanx.SampleDataset;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Benchmarks;

/// <summary>
/// Measures SymbolIndex building and resolution costs, comparing catalogue-only vs
/// roster compilation scenarios. This isolates the indexing overhead that is currently
/// duplicated for every roster compilation.
/// </summary>
[MemoryDiagnoser]
public class SymbolIndexBenchmarks
{
    // ── Pre-built compilations ──────────────────────────────────────────

    // Sample dataset
    private WhamCompilation _sampleCatalogueCompilation = null!;
    private WhamCompilation _sampleRosterCompilation = null!;
    private SymbolKey _sampleCatalogueEntryKey;

    // Synthetic 50-entry dataset
    private WhamCompilation _syntheticCatalogueCompilation = null!;
    private WhamCompilation _syntheticRosterCompilation = null!;
    private SymbolKey _syntheticCatalogueEntryKey;

    // Pre-built indices for resolve comparison
    private SymbolIndex _syntheticMonolithicIndex = null!;
    private SymbolIndex _syntheticLayeredIndex = null!;

    // Multi-roster scenario
    private WhamCompilation[] _multiRosterCompilations = null!;

    [Params(1, 5, 10)]
    public int RosterCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // ── Sample dataset ──
        var workspace = SampleDataResources.CreateXmlWorkspace();
        var allTrees = workspace.Documents
            .Select(x => SourceTree.CreateForRoot(x.GetRootAsync().Result!))
            .ToImmutableArray();

        var sampleCatTrees = allTrees.Where(t => t.GetRoot() is not RosterNode).ToImmutableArray();
        var sampleRosterTrees = allTrees.Where(t => t.GetRoot() is RosterNode).ToImmutableArray();

        _sampleCatalogueCompilation = WhamCompilation.Create(sampleCatTrees);
        _sampleCatalogueCompilation.GetDiagnostics(); // force complete
        _sampleRosterCompilation = WhamCompilation.CreateRosterCompilation(sampleRosterTrees, _sampleCatalogueCompilation);
        _sampleRosterCompilation.GetDiagnostics(); // force complete

        // Get a SymbolKey for a catalogue entry to test cross-compilation resolution
        var sampleCat = _sampleCatalogueCompilation.GlobalNamespace.Catalogues
            .FirstOrDefault(c => !c.IsGamesystem);
        if (sampleCat is not null)
        {
            var entry = sampleCat.RootContainerEntries.FirstOrDefault()
                ?? sampleCat.SharedSelectionEntryContainers.FirstOrDefault();
            if (entry is not null)
                _sampleCatalogueEntryKey = SymbolKey.Create(entry);
        }

        // ── Synthetic dataset ──
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
                    ]
                }.ToNode())
            .AddCategoryEntries(
                NodeFactory.CategoryEntry("cat-hq", "HQ"),
                NodeFactory.CategoryEntry("cat-troops", "Troops"))
            .AddForceEntries(
                new ForceEntryCore
                {
                    Id = "fe-patrol",
                    Name = "Patrol",
                    CategoryLinks =
                    [
                        new CategoryLinkCore { Id = "cl-hq", Name = "HQ", TargetId = "cat-hq" },
                        new CategoryLinkCore { Id = "cl-troops", Name = "Troops", TargetId = "cat-troops" },
                    ]
                }.ToNode());

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
                Type = SelectionEntryKind.Unit,
                Costs =
                [
                    new CostCore { Name = "Points", TypeId = "pts", Value = 10m + i },
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
                        Name = "HQ",
                        TargetId = "cat-hq",
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

        var catTrees = ImmutableArray.Create(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        _syntheticCatalogueCompilation = WhamCompilation.Create(catTrees);
        _syntheticCatalogueCompilation.GetDiagnostics();

        // Create a roster with a few selections to have some roster-level symbols
        var rosterNode = NodeFactory.Roster(gst)
            .WithCosts(
                NodeFactory.Cost("pts", "pts", 0m),
                NodeFactory.Cost("pl", "pl", 0m));
        var rosterTrees = ImmutableArray.Create(SourceTree.CreateForRoot(rosterNode));

        _syntheticRosterCompilation = WhamCompilation.CreateRosterCompilation(rosterTrees, _syntheticCatalogueCompilation);
        _syntheticRosterCompilation.GetDiagnostics();

        // SymbolKey for a catalogue entry to test resolution
        var synthCat = _syntheticCatalogueCompilation.GlobalNamespace.Catalogues
            .First(c => !c.IsGamesystem);
        var firstEntry = synthCat.RootContainerEntries.First();
        _syntheticCatalogueEntryKey = SymbolKey.Create(firstEntry);

        // Pre-built indices for resolve comparison
        _syntheticMonolithicIndex = SymbolIndex.Build(_syntheticRosterCompilation);
        _syntheticLayeredIndex = SymbolIndex.Build(
            _syntheticRosterCompilation, _syntheticCatalogueCompilation.GetSymbolIndex());

        // Multi-roster scenario: N independent roster compilations sharing catalogue
        _multiRosterCompilations = new WhamCompilation[RosterCount];
        for (int i = 0; i < RosterCount; i++)
        {
            var r = NodeFactory.Roster(gst).WithName($"Roster {i}")
                .WithCosts(NodeFactory.Cost("pts", "pts", 0m));
            var comp = WhamCompilation.CreateRosterCompilation(
                [SourceTree.CreateForRoot(r)], _syntheticCatalogueCompilation);
            comp.GetDiagnostics();
            _multiRosterCompilations[i] = comp;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Scenario 1: SymbolIndex.Build() — isolated cost comparison
    // ═══════════════════════════════════════════════════════════════════

    [Benchmark(Description = "Synth50: Build (catalogue only)")]
    [BenchmarkCategory("Build", "Synthetic")]
    public object Synth_Build_CatalogueOnly()
    {
        return SymbolIndex.Build(_syntheticCatalogueCompilation);
    }

    [Benchmark(Description = "Synth50: Build (roster — monolithic, re-indexes all)")]
    [BenchmarkCategory("Build", "Synthetic")]
    public object Synth_Build_RosterMonolithic()
    {
        // Old behavior: no catalogue index, walks all symbols.
        return SymbolIndex.Build(_syntheticRosterCompilation);
    }

    [Benchmark(Description = "Synth50: Build (roster — layered, roster-only)")]
    [BenchmarkCategory("Build", "Synthetic")]
    public object Synth_Build_RosterLayered()
    {
        // New behavior: catalogue index provided, skips catalogue symbols.
        var catIndex = _syntheticCatalogueCompilation.GetSymbolIndex();
        return SymbolIndex.Build(_syntheticRosterCompilation, catIndex);
    }

    [Benchmark(Description = "Sample: Build (catalogue only)")]
    [BenchmarkCategory("Build", "Sample")]
    public object Sample_Build_CatalogueOnly()
    {
        return SymbolIndex.Build(_sampleCatalogueCompilation);
    }

    [Benchmark(Description = "Sample: Build (roster — monolithic, re-indexes all)")]
    [BenchmarkCategory("Build", "Sample")]
    public object Sample_Build_RosterMonolithic()
    {
        return SymbolIndex.Build(_sampleRosterCompilation);
    }

    [Benchmark(Description = "Sample: Build (roster — layered, roster-only)")]
    [BenchmarkCategory("Build", "Sample")]
    public object Sample_Build_RosterLayered()
    {
        var catIndex = _sampleCatalogueCompilation.GetSymbolIndex();
        return SymbolIndex.Build(_sampleRosterCompilation, catIndex);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Scenario 2: Resolve lookup — monolithic vs layered (catalogue key)
    // ═══════════════════════════════════════════════════════════════════

    [Benchmark(Description = "Synth50: Resolve (monolithic index, catalogue key)")]
    [BenchmarkCategory("Resolve", "Synthetic")]
    public SymbolKeyResolution Synth_Resolve_Monolithic()
    {
        return _syntheticMonolithicIndex.Resolve(_syntheticCatalogueEntryKey);
    }

    [Benchmark(Description = "Synth50: Resolve (layered index, catalogue key)")]
    [BenchmarkCategory("Resolve", "Synthetic")]
    public SymbolKeyResolution Synth_Resolve_Layered()
    {
        return _syntheticLayeredIndex.Resolve(_syntheticCatalogueEntryKey);
    }

    [Benchmark(Description = "Synth50: ResolveSymbolKey (catalogue entry from roster)")]
    [BenchmarkCategory("Resolve", "Synthetic")]
    public SymbolKeyResolution Synth_Resolve_CatalogueEntryFromRoster()
    {
        return _syntheticRosterCompilation.ResolveSymbolKey(_syntheticCatalogueEntryKey);
    }

    [Benchmark(Description = "Sample: ResolveSymbolKey (catalogue entry from roster)")]
    [BenchmarkCategory("Resolve", "Sample")]
    public SymbolKeyResolution Sample_Resolve_CatalogueEntryFromRoster()
    {
        return _sampleRosterCompilation.ResolveSymbolKey(_sampleCatalogueEntryKey);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  Scenario 3: Multi-roster — N roster compilations building indices
    // ═══════════════════════════════════════════════════════════════════

    [Benchmark(Description = "Synth50: Multi-roster Build (monolithic, N rosters)")]
    [BenchmarkCategory("MultiRoster", "Synthetic")]
    public object Synth_MultiRoster_Monolithic()
    {
        object last = null!;
        for (int i = 0; i < _multiRosterCompilations.Length; i++)
        {
            last = SymbolIndex.Build(_multiRosterCompilations[i]);
        }
        return last;
    }

    [Benchmark(Description = "Synth50: Multi-roster Build (layered, N rosters)")]
    [BenchmarkCategory("MultiRoster", "Synthetic")]
    public object Synth_MultiRoster_Layered()
    {
        var catIndex = _syntheticCatalogueCompilation.GetSymbolIndex();
        object last = null!;
        for (int i = 0; i < _multiRosterCompilations.Length; i++)
        {
            last = SymbolIndex.Build(_multiRosterCompilations[i], catIndex);
        }
        return last;
    }
}
