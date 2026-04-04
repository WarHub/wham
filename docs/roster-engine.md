# Roster Engine Architecture

The wham roster engine is a BattleScribe-spec conformant implementation that
passes 304 conformance specs. It operates on ISymbol/WhamCompilation
types from the Roslyn-inspired compilation model.

See [ADR-0006](adrs/0006-isymbol-based-roster-engine.md) for the architectural
decision record.

## Overview

```
┌─────────────────────────────────────────────────────────┐
│         BattleScribeSpec.TestKit (Protocol types)        │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│         RosterEngine.Spec (adapter layer)                │
│  ProtocolConverter → SpecRosterEngineAdapter → StateMapper│
│  (ConstraintValidator — legacy, replaced by evaluator)   │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│            RosterEngine (core, ISymbol-based)             │
│  WhamRosterEngine + EntryResolver                        │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│   Concrete.Extensions (symbols + effective wrappers)     │
│   ModifierEvaluator + EffectiveEntryCache (internal)     │
│   ConstraintEvaluator (internal)                         │
│   CompletionPart pipeline (EffectiveEntries, Constraints)│
│   Extensions (public ISymbol interfaces)                 │
│   Source (DTO types, SourceNode trees)                   │
└──────────────────────────────────────────────────────────┘
```

## Projects

### WarHub.ArmouryModel.RosterEngine

Core engine with zero TestKit dependency. Key classes:

- **WhamRosterEngine**: Functional API — takes `RosterNode` + `Compilation`,
  returns modified `RosterNode`. Operations: CreateRoster, AddForce,
  RemoveForce, SelectEntry, SelectChildEntry, DeselectSelection, etc.
- **ModifierEvaluator** (internal to Concrete.Extensions): Evaluates
  `IEffectSymbol` effects with `EvalContext(ISelectionSymbol?, IForceSymbol?, EntrySymbol)`.
  All public methods accept ISymbol types. Internally accesses `Declaration`
  node properties for EntryId/EntryGroupId/Categories/Costs to avoid
  triggering lazy binding reentrancy during evaluation.
- Entry resolution leverages the Compilation's Binder for link targets.

### WarHub.ArmouryModel.RosterEngine.Spec

Adapter bridging TestKit's `IRosterEngine` to the ISymbol engine:

- **ProtocolConverter**: Protocol* → SourceNode trees → WhamCompilation
- **SpecRosterEngineAdapter**: `IRosterEngine` impl maintaining current state
- **StateMapper**: Maps ISymbol roster tree to Protocol `RosterState`
- **ConstraintValidator**: Validates constraints using ISymbol types
  (legacy — replaced by ConstraintEvaluator in Concrete.Extensions)

## Components

### Effective Entry Symbols (Roslyn-style SubstitutedSymbol pattern)

Wrapper symbols in `Concrete.Extensions/Symbols/Effective/` that implement the
same public interfaces but return modifier-applied values:

- **EffectiveEntrySymbol**: Wraps `ISelectionEntryContainerSymbol` — overrides
  Name, IsHidden, Constraints, Costs, Categories. Exposes `OriginalEntry`.
- **EffectiveConstraintSymbol**: Wraps `IConstraintSymbol` with effective Query.
- **EffectiveQuerySymbol**: Wraps `IQuerySymbol` with effective ReferenceValue.
- **EffectiveCostSymbol**: Wraps `ICostSymbol` with effective Value.

Access patterns:
```csharp
// From ISelectionSymbol → effective entry (populated via cache)
var sel = roster.Forces[0].Selections[0];
var effectiveName = sel.EffectiveSourceEntry.Name;     // modifier-applied
var effectiveCosts = sel.EffectiveSourceEntry.Costs;   // modifier-applied

// From IForceSymbol → effective entry for force-context lookups
var force = roster.Forces[0];
var effectiveEntry = force.GetEffectiveEntry(catalogueEntry);

// From IRosterSymbol → general effective entry lookup
var effective = roster.GetEffectiveEntry(declaredEntry, selection, force);
```

Caching is handled by `EffectiveEntryCache` (ConcurrentDictionary-based), which
is self-initializing on `RosterSymbol`. The cache lazily creates its own
`ModifierEvaluator` from the roster symbol and compilation
— no external wiring required. Consumers access effective values through the
symbol API; `ConstraintValidator` and `StateMapper` share a single cache per roster.

**Important**: `StateMapper` and `ConstraintValidator` map node→symbol via lazy
lookup dictionaries (built from `Compilation.SourceGlobalNamespace.Rosters` symbol
tree) when calling evaluator methods that require `ISelectionSymbol`/`IForceSymbol`.

### ModifierEvaluator (~960 lines, internal to Concrete.Extensions)

Evaluates modifiers and conditions using IEffectSymbol/IConditionSymbol.
All public methods accept ISymbol types (ISelectionSymbol, IForceSymbol).
Constructor takes `IRosterSymbol` and `WhamCompilation`.

**Lazy binding safety**: Methods that query selection properties (EntryId,
EntryGroupId, Categories, Costs) use `SelectionSymbol.Declaration` node
properties instead of ISymbol accessors like `SourceEntry` or `SourceEntryPath`,
because those trigger lazy binder resolution that would cause reentrancy
during modifier evaluation.

Evaluates modifiers and conditions using IEffectSymbol/IConditionSymbol:

- **Effect types**: set, increment, decrement, append, add/remove category
- **Target kinds**: name, hidden, costs, characteristics, constraint values,
  categories, page, rule description
- **Condition evaluation**: IQuerySymbol comparison with scope resolution
- **Scopes**: self, parent, force, roster, primary-category, ancestor
- **Repeat handling**: Multiplicative repeat counts based on queries

### ConstraintValidator (~850 lines, legacy)

> **Note**: `ConstraintValidator` in RosterEngine.Spec is the legacy node-layer
> validator. Constraint evaluation is now performed by `ConstraintEvaluator` in
> Concrete.Extensions as part of the `CompletionPart.Constraints` phase.
> The adapter's `GetValidationErrors()` reads from
> `compilation.GetConstraintDiagnostics()`.

Validates IConstraintSymbol constraints using effective entry symbols:

- **Force-level validation**: Root entry constraints (scope=force/roster)
- **Child validation**: Parent-scoped constraints on child selections
- **Category validation**: Min/max on category links
- **Shared constraint counting**: Counts across ALL entry links to same
  shared entry using link ID → shared entry mapping
- **Constraint merging**: Link + shared constraints merged, most restrictive
  wins. Merge key: `direction:valueKind` (not scope)
- **Effective symbols**: Uses `EffectiveEntryCache` for modifier-adjusted
  constraint boundary values (replaces direct ModifierEvaluator calls)
- **Error format**: `on='ownerType ownerEntryId', from='entryId/constraintId'`

## BattleScribe Behavioral Alignment

| Behavior | BattleScribe | wham | Notes |
|----------|-------------|------|-------|
| scope=parent on uncategorised entries | No error | Error (correct) | `scope-parent` spec has wham override |
| Shared constraint counting | Counts across all links | Same | via `_sharedEntryLinkIds` index |
| Constraint merging (link+shared) | Most restrictive wins | Same | Keys by direction+valueKind |
| Modifier on constraint value | Modifies boundary | Same | `GetEffectiveConstraintValues` |
| Auto-select on min constraint | Yes | Yes | On addForce |
| field=forces on SelectionEntry | Always 0 | Same | Child filter never matches |
| Hidden entry + selections | Error with constraintId="hidden" | Same | Checks `IsEffectivelyHidden` |

## CompletionPart Pipeline (Roslyn-inspired)

Symbol completion proceeds through ordered phases using a `CompletionPart`
flags enum. Each phase has `Start`/`Finish` pairs with CAS-protected
once-only execution:

```
Phase 0-1: BindReferences     — resolve IDs to symbols (all symbols)
Phase 2-3: Members             — compute GetMembers() (all symbols)
Phase 4-5: EffectiveEntries    — compute effective entry symbols (RosterSymbol only)
Phase 6-7: Constraints         — evaluate constraint diagnostics (RosterSymbol only)
```

Non-roster symbols auto-complete phases 4-7 in the base virtual methods.
`RosterSymbol` overrides `ComputeEffectiveEntries()` and
`EvaluateConstraints()` to perform actual work.

**EffectiveEntries phase** (`RosterSymbol.ComputeEffectiveEntries`):
1. Force-complete only referenced catalogues (gamesystem + each force's catalogue)
2. Create `EffectiveEntryCache` for the roster
3. Walk force→selection tree, eagerly populate `SelectionSymbol.lazyEffectiveSourceEntry`

**Constraints phase** (`RosterSymbol.EvaluateConstraints`):
1. Call `ConstraintEvaluator.Evaluate(roster, compilation, diagnosticBag)`
2. Diagnostics are stored in `WhamCompilation.ConstraintDiagnostics`
3. Accessed via `compilation.GetConstraintDiagnostics()`

### ConstraintEvaluator (~810 lines, internal to Concrete.Extensions)

Symbol-layer port of the legacy `ConstraintValidator`. Produces
`WhamDiagnostic` instances with structured args:

- **Args format**: `[ownerType, ownerEntryId, entryId, constraintId]`
- **Diagnostic codes**: `WRN_ConstraintMinSelections` through
  `WRN_ConstraintCostLimit` (WHAM0100-0107)
- **Force catalogue**: Uses `force.CatalogueReference.Catalogue` to get the
  per-force catalogue (requires correct `catalogueId` on force nodes)
- **Entry link handling**: Merges link + shared constraints, counts across
  all links to the same shared entry

## Entry Link Selection Binding

When creating selections from entry links, the `entryId` must use
BattleScribe's `"::"` path format:

```
entryId = "linkId::targetId"   ← correct (produces 2-element path)
entryId = "linkId"             ← WRONG (produces 1-element path, cast fails)
```

The binder resolves this into `SourceEntryPath = [link, resolvedTarget]`:
- `SourceEntryPath.SourceEntries[0]` = the entry link symbol
- `SourceEntryPath.SourceEntries[1]` = the resolved target entry
- `SourceEntry` = last element = resolved target (`ISelectionEntrySymbol`)

`WhamRosterEngine.BuildEntryIdPath()` generates this format automatically.

## Expected Failures

2 specs tagged `undefined-behavior` that all engines fail:
- `modifier-group-on-infogroup` — ModifierGroup on infoGroup
- `modifier-group-on-category-link` — ModifierGroup on categoryLink

## File Layout

```
src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/Effective/
├── EffectiveEntrySymbol.cs       (wrapper: entry with effective values)
├── EffectiveConstraintSymbol.cs  (wrapper: constraint with effective query)
├── EffectiveQuerySymbol.cs       (wrapper: query with effective ReferenceValue)
├── EffectiveCostSymbol.cs        (wrapper: cost with effective Value)
├── EffectiveEntryCache.cs        (self-initializing cache, owns ModifierEvaluator)
├── EffectiveEntryKey.cs          (cache key type)
└── ModifierEvaluator.cs          (~1000 lines, internal)

src/WarHub.ArmouryModel.Concrete.Extensions/Symbols/
└── ConstraintEvaluator.cs        (~810 lines, internal, symbol-layer constraints)

src/WarHub.ArmouryModel.Concrete.Extensions/Utilities/
└── CompletionPart.cs             (8-bit flags enum, phases 0-7)

src/WarHub.ArmouryModel.RosterEngine/
├── WhamRosterEngine.cs      (~790 lines)
└── EntryResolver.cs          (~530 lines)

src/WarHub.ArmouryModel.RosterEngine.Spec/
├── SpecRosterEngineAdapter.cs (IRosterEngine impl, uses GetConstraintDiagnostics())
├── ProtocolConverter.cs       (Protocol → SourceNode)
├── StateMapper.cs             (ISymbol → Protocol state)
└── ConstraintValidator.cs     (legacy constraint validation, replaced by ConstraintEvaluator)

tests/WarHub.ArmouryModel.RosterEngine.Tests/
├── ConformanceTests.cs       (runs all 304 specs)
└── WarHub.ArmouryModel.RosterEngine.Tests.csproj
```

## Related ADRs

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation model (ported from phalanx)
- [ADR-0003](adrs/0003-protocol-based-roster-engine.md) — Why protocol types over ISymbol
- [ADR-0004](adrs/0004-battlescribe-spec-conformance-testing.md) — Conformance testing strategy
