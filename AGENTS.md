# AGENTS.md

> **Keeping docs current**: If you discover that information in this file,
> in `docs/`, or in `docs/adrs/` is outdated, incomplete, or contradicts
> the actual codebase, **propose updates** as part of your changeset.
> Documentation should stay in sync with the code — treat it as a living
> reference, not a frozen snapshot.

wham — foundational .NET library (`WarHub.ArmouryModel`) and CLI tool for wargame
datafile management, with a BattleScribe-spec conformant roster engine.

## Stability & compatibility

This project is **experimental**. Breaking changes to public APIs, interfaces,
and project structure are expected and encouraged when they improve the design.
There is **no backwards compatibility requirement** — do not hesitate to rename,
remove, or restructure types, methods, or namespaces when it makes the codebase
cleaner or more correct. Prefer the right design over preserving the current API
surface.

## Build & test

```bash
git submodule update --init                            # first time (required)
dotnet restore && dotnet build                         # build all
dotnet test                                            # all tests
dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/  # conformance only
dotnet pack                                            # NuGet packages (Release mode)
```

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

| Layer | Path |
|-------|------|
| DTO + immutable trees | `src/WarHub.ArmouryModel.Source/` |
| BattleScribe XML (de)serialization | `src/WarHub.ArmouryModel.Source.BattleScribe/` |
| Source Generator (`*Core` → `SourceNode`) | `src/WarHub.ArmouryModel.Source.CodeGeneration/` |
| ISymbol public API, Binder, Diagnostics | `src/WarHub.ArmouryModel.Extensions/` |
| Symbol implementations, lazy binding | `src/WarHub.ArmouryModel.Concrete.Extensions/` |
| Symbol source generators + analyzer | `src/WarHub.ArmouryModel.Concrete.Extensions.Generators/` |

### Roster engine (protocol-based, conformance-tested)

Works directly with `Protocol*` types from BattleScribeSpec.TestKit, not with
SourceNode/ISymbol. The compilation pipeline integrates effective entries and
constraint evaluation via `CompletionPart` phases:

```
WhamRosterEngine (IRosterEngine)
├── EntryResolver      — flatten entries, merge links, resolve info
└── EffectiveContainerEntrySymbol (abstract base)
    ├── EffectiveEntrySymbol        — selection entry effective wrapper
    └── EffectiveForceEntrySymbol   — force entry effective wrapper
    ├── EffectiveEntryCache — lazy cache, owns ModifierEvaluator
    │   └── CollectEffectiveResources() — single-pass resource resolution
    ├── EffectiveProfileSymbol — IProfileSymbol with modifier-applied characteristics
    ├── EffectiveRuleSymbol    — IRuleSymbol with modifier-applied description
    └── ModifierEvaluator      — apply modifiers, evaluate conditions, resolve scopes

Symbol completion pipeline (CompletionPart phases):
  Members → MembersCompleted → EffectiveEntries → CheckReferences → CheckConstraints
  (Bound reference fields are self-completing via Interlocked.CompareExchange)
  (RosterSymbol overrides EffectiveEntries + CheckConstraints phases)

ConstraintEvaluator (Concrete.Extensions) — symbol-layer constraint validation
StateMapper (RosterEngine.Spec) — thin Symbol→Protocol mapper (~140 LOC)
  Reads only the public Symbol API surface — no SourceNode access.
```

| Component | Path |
|-----------|------|
| WhamRosterEngine | `src/WarHub.ArmouryModel.RosterEngine/WhamRosterEngine.cs` |
| EntryResolver | `src/WarHub.ArmouryModel.RosterEngine/EntryResolver.cs` |
| ConstraintEvaluator | `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/ConstraintEvaluator.cs` |
| StateMapper | `src/WarHub.ArmouryModel.RosterEngine.Spec/StateMapper.cs` |
| Effective symbols | `src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/` |
| Conformance tests | `tests/WarHub.ArmouryModel.RosterEngine.Tests/ConformanceTests.cs` |

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

Path: `src/WarHub.ArmouryModel.EditorServices/`

### External dependency

| Path | What |
|------|------|
| `lib/battlescribe-spec/` | Git submodule — conformance specs + TestKit |
| `lib/battlescribe-spec/specs/{category}/{id}.yaml` | YAML conformance specs |
| `lib/battlescribe-spec/src/BattleScribeSpec.TestKit/` | TestKit: IRosterEngine, SpecRunner, Protocol types |

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
Source, Source.BattleScribe, ProjectModel, Workspaces.BattleScribe, Workspaces.Gitree, CliTool (`wham` dotnet tool)

**Internal** (IsPackable=false, relaxed analysis):
Extensions, Concrete.Extensions, Concrete.Extensions.Generators, EditorServices, RosterEngine, RosterEngine.Spec, Phalanx.SampleDataset

**External submodule**: BattleScribeSpec.TestKit (from `lib/battlescribe-spec`)

## Conformance testing

- Specs in `lib/battlescribe-spec/specs/{category}/{id}.yaml`
- Engine name: **"wham"** (registered in `ConformanceTests.cs`)
- Per-engine overrides: specs can have `engines.wham:` section for wham-specific expectations
- Error format: `on='ownerType ownerEntryId', from='entryId/constraintId'`
- `errors:` = exact set match; `errorsContain:` = subset match
- Run single spec: `dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/ --filter "DisplayName~my-spec-id"`

## Common tasks

**Fix a conformance test failure:**
1. Read the failing spec YAML in `lib/battlescribe-spec/specs/`
2. Trace through WhamRosterEngine → EntryResolver/ModifierEvaluator/ConstraintEvaluator
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
- **Concrete.Extensions generators**: `Concrete.Extensions.Generators` provides:
  - `[GenerateSymbol(SymbolKind.X)]` — generates `Kind` property + 3 `Accept` overloads
  - `[Bound]` on properties — generates `CheckReferencesCore` accessing all bound properties
  - `WHAM001` analyzer — warns when `GetBoundField` is called without `[Bound]`
  - Symbol classes using these must be `partial`
- **BattleScribe quirks**: some spec default expectations match BattleScribe bugs rather than
  "correct" behavior; wham uses engine-specific overrides for these (documented in
  `docs/adrs/0004-battlescribe-spec-conformance-testing.md`)
- **Entry link entryId format**: Selections created from entry links MUST use
  `"linkId::targetId"` format for `entryId` (matching BattleScribe). This produces
  `SourceEntryPath = [link, target]` with `SourceEntry` = resolved target.
  Single-segment `entryId = "linkId"` causes `InvalidCastException` in
  `SelectionSymbol.SourceEntry`.

## Documentation

- `docs/roster-engine.md` — Roster engine architecture overview
- `docs/incremental-compilation.md` — Incremental compilation design and benchmark results
- `docs/latent-issues-plan.md` — Plan for addressing known latent issues
- `docs/adrs/` — Architecture Decision Records
