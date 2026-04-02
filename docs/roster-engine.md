# Roster Engine Architecture

The wham roster engine is a BattleScribe-spec conformant implementation that
passes 291/293 conformance specs (99.3%). It operates on ISymbol/WhamCompilation
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
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│            RosterEngine (core, ISymbol-based)             │
│  WhamRosterEngine + ModifierEvaluator + ConstraintValidator│
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│   Concrete.Extensions / Extensions / Source (symbols)     │
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

### ModifierEvaluator (~1100 lines)

Evaluates modifiers and conditions using IEffectSymbol/IConditionSymbol:

- **Effect types**: set, increment, decrement, append, add/remove category
- **Target kinds**: name, hidden, costs, characteristics, constraint values,
  categories, page, rule description
- **Condition evaluation**: IQuerySymbol comparison with scope resolution
- **Scopes**: self, parent, force, roster, primary-category, ancestor
- **Repeat handling**: Multiplicative repeat counts based on queries

### ConstraintValidator (~700 lines)

Validates IConstraintSymbol constraints:

- **Force-level validation**: Root entry constraints (scope=force/roster)
- **Child validation**: Parent-scoped constraints on child selections
- **Category validation**: Min/max on category links
- **Shared constraint counting**: Counts across ALL entry links to same
  shared entry using link ID → shared entry mapping
- **Constraint merging**: Link + shared constraints merged, most restrictive
  wins. Merge key: `direction:valueKind` (not scope)
- **Modifier integration**: Uses `GetEffectiveConstraintValues` for
  modifier-adjusted boundary values
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
src/WarHub.ArmouryModel.RosterEngine/
├── WhamRosterEngine.cs      (~790 lines)
├── EntryResolver.cs          (~530 lines)
├── ModifierEvaluator.cs      (~540 lines)
├── ConstraintValidator.cs    (~480 lines)
├── RosterForce.cs            (state model)
├── RosterSelection.cs        (state model)
└── WarHub.ArmouryModel.RosterEngine.csproj

tests/WarHub.ArmouryModel.RosterEngine.Tests/
├── ConformanceTests.cs       (runs all 303 specs)
└── WarHub.ArmouryModel.RosterEngine.Tests.csproj
```

## Related ADRs

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation model (ported from phalanx)
- [ADR-0003](adrs/0003-protocol-based-roster-engine.md) — Why protocol types over ISymbol
- [ADR-0004](adrs/0004-battlescribe-spec-conformance-testing.md) — Conformance testing strategy
