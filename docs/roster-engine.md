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
│  ConstraintValidator, EffectiveEntries (cache wiring)    │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│            RosterEngine (core, ISymbol-based)             │
│  WhamRosterEngine + ModifierEvaluator + EntryResolver    │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│   Concrete.Extensions (symbols + effective wrappers)     │
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
- **ModifierEvaluator**: Evaluates `IEffectSymbol` effects with
  `EvalContext(Selection?, Force?, EntrySymbol)`.
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

Caching is handled by `EffectiveEntryCache` (ConcurrentDictionary-based), set on
`RosterSymbol` via `SetEffectiveEntryCache()`. The `EffectiveEntries` helper in
`RosterEngine.Spec` wires up the cache from a `ModifierEvaluator` instance.

### ModifierEvaluator (~1000 lines)

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
├── EffectiveEntryCache.cs        (ConcurrentDictionary-based cache)
└── EffectiveEntryKey.cs          (cache key type)

src/WarHub.ArmouryModel.RosterEngine/
├── WhamRosterEngine.cs      (~790 lines)
├── EntryResolver.cs          (~530 lines)
└── ModifierEvaluator.cs      (~1000 lines)

src/WarHub.ArmouryModel.RosterEngine.Spec/
├── SpecRosterEngineAdapter.cs (IRosterEngine impl)
├── ProtocolConverter.cs       (Protocol → SourceNode)
├── StateMapper.cs             (ISymbol → Protocol state)
├── ConstraintValidator.cs     (constraint validation)
└── EffectiveEntries.cs        (cache factory + initialization)

tests/WarHub.ArmouryModel.RosterEngine.Tests/
├── ConformanceTests.cs       (runs all 304 specs)
└── WarHub.ArmouryModel.RosterEngine.Tests.csproj
```

## Related ADRs

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation model (ported from phalanx)
- [ADR-0003](adrs/0003-protocol-based-roster-engine.md) — Why protocol types over ISymbol
- [ADR-0004](adrs/0004-battlescribe-spec-conformance-testing.md) — Conformance testing strategy
