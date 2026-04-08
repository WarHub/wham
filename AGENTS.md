# AGENTS.md

> **Keeping docs current**: If you discover that information in this file,
> in `docs/`, or in `docs/adrs/` is outdated, incomplete, or contradicts
> the actual codebase, **propose updates** as part of your changeset.
> This applies to file paths, project lists, build commands, conventions,
> architecture descriptions, and any other factual claims. Documentation
> should stay in sync with the code — treat it as a living reference,
> not a frozen snapshot.

wham — foundational .NET library (`WarHub.ArmouryModel`) and CLI tool for wargame
datafile management, with a BattleScribe-spec conformant roster engine (304/304 specs).

## Build & test

```bash
git submodule update --init                            # first time (required)
dotnet restore && dotnet build                         # build all
dotnet test                                            # all tests (628)
dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/  # conformance only (304)
dotnet pack                                            # NuGet packages (Release mode)
```

## Key files

### Build & configuration

| Path | What |
|------|------|
| `wham.sln` | Solution file (14 src + 10 test projects) |
| `Directory.Build.props` | Shared build properties, analysis, packaging |
| `Directory.Packages.props` | Central Package Management — all NuGet versions |
| `global.json` | .NET SDK version (10.0) |
| `nuget.config` | NuGet feed configuration |
| `.editorconfig` | Code style (4-space C#, 2-space XML/JSON) |
| `version.json` | Nerdbank.GitVersioning configuration |

### Roster engine (conformance-tested)

| Path | What |
|------|------|
| `src/WarHub.ArmouryModel.RosterEngine/WhamRosterEngine.cs` | IRosterEngine impl — setup, forces, selections, state |
| `src/WarHub.ArmouryModel.RosterEngine/EntryResolver.cs` | Entry/link resolution, merging, flattening, cycle detection |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/ConstraintEvaluator.cs` | Symbol-layer constraint validation, produces WhamDiagnostics |
| `src/WarHub.ArmouryModel.RosterEngine.Spec/StateMapper.cs` | Thin Symbol→Protocol mapper (~140 LOC), reads only public Symbol API |
| `tests/WarHub.ArmouryModel.RosterEngine.Tests/ConformanceTests.cs` | Runs all 304 BattleScribe-spec conformance specs |

### Effective entry symbols (Roslyn-style wrappers)

| Path | What |
|------|------|
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveContainerEntrySymbol.cs` | Abstract base for effective container entries (shared Name/Hidden/Costs/Constraints/Resources/Page) |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveEntrySymbol.cs` | Selection entry effective wrapper, extends EffectiveContainerEntrySymbol with Categories/PrimaryCategory |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveProfileSymbol.cs` | IEffectiveProfileSymbol impl — effective name, hidden, characteristics, page, publicationId |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveRuleSymbol.cs` | IEffectiveRuleSymbol impl — effective name, description, hidden, page, publicationId |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveConstraintSymbol.cs` | Wraps IConstraintSymbol with effective Query ReferenceValue |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveCostSymbol.cs` | Wraps ICostSymbol with effective Value |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveQuerySymbol.cs` | Wraps IQuerySymbol with effective ReferenceValue |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveForceEntrySymbol.cs` | Wraps IForceEntrySymbol with effective Resources (profiles+rules) |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveEntryCache.cs` | Lazy cache + single-pass resource resolution; owns ModifierEvaluator, self-initializing on RosterSymbol |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/EffectiveEntryKey.cs` | Cache key: (entry, selection?, force?) |
| `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/ModifierEvaluator.cs` | Modifier application, condition eval, scope resolution (internal to Compilation) |

### Source model (DTO and immutable trees)

| Path | What |
|------|------|
| `src/WarHub.ArmouryModel.Source/` | DTO (`*Core` types), immutable `SourceNode` wrappers, code-generated |
| `src/WarHub.ArmouryModel.Source.BattleScribe/` | BattleScribe XML (de)serialization |
| `src/WarHub.ArmouryModel.Source.CodeGeneration/` | C# Source Generator for `*Core` → `SourceNode` |

### Ported compilation model (from phalanx, non-packable)

| Path | What |
|------|------|
| `src/WarHub.ArmouryModel.Extensions/` | ISymbol public API (40+ interfaces), Binder, Diagnostics |
| `src/WarHub.ArmouryModel.Concrete.Extensions/` | Symbol implementations (89 files), lazy binding |
| `src/WarHub.ArmouryModel.EditorServices/` | Roster operations, undo/redo, formatting, workspace |

### Workspace layer (multi-roster management)

| Path | What |
|------|------|
| `src/WarHub.ArmouryModel.EditorServices/WhamWorkspace.cs` | Central workspace: catalogue + multi-roster management, owns all mutations, change events |
| `src/WarHub.ArmouryModel.EditorServices/CompilationTracker.cs` | Internal lazy per-roster compilation via `CreateRosterCompilation()` |
| `src/WarHub.ArmouryModel.EditorServices/DocumentId.cs` | Synthetic GUID for stable source tree identity |
| `src/WarHub.ArmouryModel.EditorServices/WorkspaceChangeKind.cs` | Enum for workspace change event kinds |
| `src/WarHub.ArmouryModel.EditorServices/WorkspaceChangedEventArgs.cs` | Versioned event args for workspace change notifications |

### External dependency

| Path | What |
|------|------|
| `lib/battlescribe-spec/` | Git submodule — conformance specs + TestKit |
| `lib/battlescribe-spec/specs/{category}/{id}.yaml` | 304 YAML conformance specs |
| `lib/battlescribe-spec/src/BattleScribeSpec.TestKit/` | TestKit: IRosterEngine, SpecRunner, Protocol types |

### Documentation

| Path | What |
|------|------|
| `docs/roster-engine.md` | Roster engine architecture overview |
| `docs/incremental-compilation.md` | Incremental compilation design and benchmark results |
| `docs/latent-issues-plan.md` | Plan for addressing known latent issues |
| `docs/adrs/` | Architecture Decision Records (8 ADRs) |

### Benchmarks

| Path | What |
|------|------|
| `tests/WarHub.ArmouryModel.Benchmarks/` | BenchmarkDotNet project — compilation performance |

## Architecture

### Source model (4-layer, Roslyn-inspired)

```
XML file → DTO (*Core) → SourceNode tree → SourceTree
                              ↓
                  Compilation.AddSourceTrees()
                              ↓
                  Binder chain resolves IDs → Symbol
                              ↓
                  ISymbol public API
```

### Roster engine (protocol-based)

Works directly with `Protocol*` types from BattleScribeSpec.TestKit, not with
SourceNode/ISymbol. The compilation pipeline integrates effective entries and
constraint evaluation via `CompletionPart` phases:

```
WhamRosterEngine (IRosterEngine)
├── EntryResolver      — flatten entries, merge links, resolve info
└── EffectiveContainerEntrySymbol (abstract base)
    ├── EffectiveSelectionEntrySymbol — selection entry effective wrapper
    └── EffectiveForceEntrySymbol    — force entry effective wrapper
    ├── EffectiveEntryCache — lazy cache, owns ModifierEvaluator
    │   └── CollectEffectiveResources() — single-pass resource resolution
    ├── EffectiveProfileSymbol — IEffectiveProfileSymbol with modifier-applied characteristics
    ├── EffectiveRuleSymbol    — IEffectiveRuleSymbol with modifier-applied description
    └── ModifierEvaluator      — apply modifiers, evaluate conditions, resolve scopes

Symbol completion pipeline (CompletionPart phases):
  BindReferences → Members → EffectiveEntries → Constraints
  (RosterSymbol overrides EffectiveEntries + Constraints phases)

ConstraintEvaluator (Concrete.Extensions) — symbol-layer constraint validation
StateMapper (RosterEngine.Spec) — thin Symbol→Protocol mapper (~140 LOC)
  Reads only the public Symbol API surface — no SourceNode access.
```

### Workspace layer (multi-roster management)

```
WhamWorkspace (owns all mutations, fires all events)
├── CatalogueCompilation (shared, rebuilt when catalogues change)
├── CatalogueTrees: ImmutableDictionary<DocumentId, SourceTree>
├── Version: long (incremented on every state change)
├── Per-roster (internal RosterDocumentState):
│   ├── CompilationTracker (lazily rebuilds RosterCompilation)
│   ├── RosterEditor (internal, undo/redo stack)
│   └── DocumentId (stable identity)
├── Events: WorkspaceChanged (EventHandler<WorkspaceChangedEventArgs>)
└── Background diagnostics (Task-based, snapshot-then-compute)
```

Workspace owns all roster mutations — no public `GetEditor()`. Consumers use
`ApplyOperation()`, `Undo()`, `Redo()` on the workspace. This ensures all
state changes fire events and prevents stale editor references.

## Code conventions

- **C# latest**, nullable enabled, implicit usings
- **4-space indent** for `.cs`, 2-space for `.xml`/`.json`/`.csproj`
- **TreatWarningsAsErrors** in Release builds (affects `dotnet pack`)
- **AnalysisMode=All** for established library projects
- **AnalysisMode=Default** for ported projects (Extensions, Concrete.Extensions, EditorServices)
- **Central Package Management**: all package versions in `Directory.Packages.props`
- **xUnit v3** for tests, **FluentAssertions** `[7.x, 8.0)` for assertions
- **Nerdbank.GitVersioning** for version numbers from git history

## Project groups

**NuGet packages** (IsPackable=true, strict analysis):
- WarHub.ArmouryModel.Source
- WarHub.ArmouryModel.Source.BattleScribe
- WarHub.ArmouryModel.ProjectModel
- WarHub.ArmouryModel.Workspaces.BattleScribe
- WarHub.ArmouryModel.Workspaces.Gitree
- WarHub.ArmouryModel.CliTool (`wham` dotnet tool)

**Internal** (IsPackable=false, relaxed analysis):
- WarHub.ArmouryModel.Extensions
- WarHub.ArmouryModel.Concrete.Extensions
- WarHub.ArmouryModel.EditorServices
- WarHub.ArmouryModel.RosterEngine
- Phalanx.SampleDataset

**External submodule**:
- BattleScribeSpec.TestKit (from `lib/battlescribe-spec`)

## Conformance testing

- **304 specs** in `lib/battlescribe-spec/specs/{category}/{id}.yaml`
- Engine name: **"wham"** (registered in `ConformanceTests.cs`)
- Per-engine overrides: specs can have `engines.wham:` section for wham-specific expectations
- Error format: `on='ownerType ownerEntryId', from='entryId/constraintId'`
- `errors:` = exact set match; `errorsContain:` = subset match
- Run single spec: `dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/ --filter "DisplayName~my-spec-id"`

## Common tasks

**Fix a conformance test failure:**
1. Read the failing spec YAML in `lib/battlescribe-spec/specs/`
2. Trace through WhamRosterEngine → EntryResolver/ModifierEvaluator/ConstraintValidator
3. Fix the engine code; run `dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/`

**Add a NuGet dependency:**
1. Add `<PackageVersion>` to `Directory.Packages.props`
2. Add `<PackageReference>` (without version) to the consuming `.csproj`

**Modify CI:**
- `.github/workflows/ci.yml` — main CI (build, test, pack)
- `.github/workflows/publish.yml` — NuGet publishing

## Known gotchas

- **Submodule required**: `git submodule update --init` before building
- **Private submodule**: `lib/battlescribe-spec` is private — CI needs a `GH_PAT` secret
  with `repo` scope, or the repo must be made public
- **Ported projects**: Extensions, Concrete.Extensions, EditorServices were ported from
  phalanx with relaxed analysis (`AnalysisMode=Default`, `GenerateDocumentationFile=false`)
- **`dotnet pack` uses Release**: this enables TreatWarningsAsErrors — ported projects are
  marked IsPackable=false to avoid failures
- **Code generation**: `WarHub.ArmouryModel.Source` uses a C# Source Generator —
  changes to `*Core` types require regeneration
- **BattleScribe quirks**: some spec default expectations match BattleScribe bugs rather than
  "correct" behavior; wham uses engine-specific overrides for these (documented in
  `docs/adrs/0004-battlescribe-spec-conformance-testing.md`)
- **Entry link entryId format**: Selections created from entry links MUST use
  `"linkId::targetId"` format for `entryId` (matching BattleScribe). This produces
  `SourceEntryPath = [link, target]` with `SourceEntry` = resolved target.
  Single-segment `entryId = "linkId"` causes `InvalidCastException` in
  `SelectionSymbol.SourceEntry`.
