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
│  ConstraintValidator                                     │
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

### ConstraintValidator (~850 lines)

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

src/WarHub.ArmouryModel.RosterEngine/
├── WhamRosterEngine.cs      (~790 lines)
└── EntryResolver.cs          (~530 lines)

src/WarHub.ArmouryModel.RosterEngine.Spec/
├── SpecRosterEngineAdapter.cs (IRosterEngine impl)
├── ProtocolConverter.cs       (Protocol → SourceNode)
├── StateMapper.cs             (ISymbol → Protocol state)
└── ConstraintValidator.cs     (constraint validation)

tests/WarHub.ArmouryModel.RosterEngine.Tests/
├── ConformanceTests.cs       (runs all 304 specs)
└── WarHub.ArmouryModel.RosterEngine.Tests.csproj
```

## Related ADRs

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation model (ported from phalanx)
- [ADR-0003](adrs/0003-protocol-based-roster-engine.md) — Why protocol types over ISymbol
- [ADR-0004](adrs/0004-battlescribe-spec-conformance-testing.md) — Conformance testing strategy
