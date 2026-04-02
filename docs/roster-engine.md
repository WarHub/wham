# Roster Engine Architecture

The wham roster engine is a BattleScribe-spec conformant implementation that
passes 304/304 conformance specs (100%). It operates on ISymbol/WhamCompilation
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
│  DiagnosticMapper (Diagnostic → ValidationErrorState)    │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│            RosterEngine (core, ISymbol-based)             │
│  WhamRosterEngine + EntryResolver                        │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│   Concrete.Extensions (compilation + validation)         │
│  WhamCompilation + ModifierEvaluator + ConstraintValidator│
│  Diagnostic infrastructure (ErrorCode, ValidationDiag.)  │
└──────────────────────┬──────────────────────────────────┘
                       ↓
┌──────────────────────────────────────────────────────────┐
│           Extensions / Source (symbols + nodes)           │
└──────────────────────────────────────────────────────────┘
```

## Projects

### WarHub.ArmouryModel.Concrete.Extensions

Compilation layer with integrated validation. Key classes:

- **WhamCompilation**: Compilation with `GetDiagnostics()` for binding errors
  and `GetValidationDiagnostics()` for constraint violations.
- **ModifierEvaluator**: Evaluates `IEffectSymbol` effects with
  `EvalContext(Selection?, Force?, EntrySymbol)`.
- **ConstraintValidator**: Validates constraints producing `Diagnostic` objects
  with structured metadata (ownerType, ownerId, entryId, constraintId).
- **ValidationDiagnostic**: `Diagnostic` subclass carrying constraint metadata.
- **RosterSymbol**: Symbol with `ForceComplete()` phases including reserved
  `EvaluateModifiers` and `Validate` CompletionParts.

### WarHub.ArmouryModel.RosterEngine

Core engine with zero TestKit dependency. Key classes:

- **WhamRosterEngine**: Functional API — takes `RosterNode` + `Compilation`,
  returns modified `RosterNode`. Operations: CreateRoster, AddForce,
  RemoveForce, SelectEntry, SelectChildEntry, DeselectSelection, etc.
- **EntryResolver**: Resolves entry links and shared entries via Binder.
- Entry resolution leverages the Compilation's Binder for link targets.

### WarHub.ArmouryModel.RosterEngine.Spec

Adapter bridging TestKit's `IRosterEngine` to the ISymbol engine:

- **ProtocolConverter**: Protocol* → SourceNode trees → WhamCompilation
- **SpecRosterEngineAdapter**: `IRosterEngine` impl maintaining current state
- **StateMapper**: Maps ISymbol roster tree to Protocol `RosterState`
- **DiagnosticMapper**: Maps `Diagnostic` → `ValidationErrorState`

## Components

### ModifierEvaluator (~1100 lines, in Concrete.Extensions)

Evaluates modifiers and conditions using IEffectSymbol/IConditionSymbol:

- **Effect types**: set, increment, decrement, append, add/remove category
- **Target kinds**: name, hidden, costs, characteristics, constraint values,
  categories, page, rule description
- **Condition evaluation**: IQuerySymbol comparison with scope resolution
- **Scopes**: self, parent, force, roster, primary-category, ancestor
- **Repeat handling**: Multiplicative repeat counts based on queries

### ConstraintValidator (~780 lines, in Concrete.Extensions)

Validates constraints producing `Diagnostic` objects:

- **Force-level validation**: Root entry constraints (scope=force/roster)
- **Child validation**: Parent-scoped constraints on child selections
- **Category validation**: Min/max on category links
- **Force count validation**: Force entry constraints with roster-level scope
- **Shared constraint counting**: Counts across ALL entry links to same
  shared entry using link ID → shared entry mapping
- **Constraint merging**: Link + shared constraints merged, most restrictive
  wins. Merge key: `direction:valueKind` (not scope)
- **Modifier integration**: Uses `GetEffectiveConstraintValues` for
  modifier-adjusted boundary values
- **Diagnostic output**: `ValidationDiagnostic` with ownerType, ownerId,
  ownerEntryId, entryId, constraintId metadata

### DiagnosticMapper (in RosterEngine.Spec)

Maps `Diagnostic` → `ValidationErrorState` for spec test assertions:

- Filters by `ValidationDiagnostic` type (not ID-based)
- Extracts ownerType/ownerId/entryId/constraintId from diagnostic metadata
- Produces `ValidationErrorState` compatible with TestKit assertions

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
| Force count constraint owner | Per-force error, owner=roster | Same | Each force instance evaluates independently |

## Expected Failures

None — 304/304 conformance specs pass.

## File Layout

```
src/WarHub.ArmouryModel.Concrete.Extensions/
├── WhamCompilation.cs            (~145 lines)
├── Validation/
│   ├── ModifierEvaluator.cs      (~540 lines, moved from RosterEngine)
│   └── ConstraintValidator.cs    (~780 lines, rewritten from Spec)
├── Diagnostics/
│   ├── ErrorCode.cs              (WRN_ codes for constraints)
│   ├── ValidationDiagnostic.cs   (~45 lines)
│   └── DiagnosticBag.cs          (diagnostic collection)
└── Symbols/
    └── RosterSymbol.cs           (~130 lines, ForceComplete phases)

src/WarHub.ArmouryModel.RosterEngine/
├── WhamRosterEngine.cs           (~790 lines)
├── EntryResolver.cs              (~530 lines)
├── RosterForce.cs                (state model)
├── RosterSelection.cs            (state model)
└── WarHub.ArmouryModel.RosterEngine.csproj

src/WarHub.ArmouryModel.RosterEngine.Spec/
├── ProtocolConverter.cs
├── SpecRosterEngineAdapter.cs    (~175 lines)
├── StateMapper.cs                (~780 lines)
├── DiagnosticMapper.cs           (~55 lines)
└── WarHub.ArmouryModel.RosterEngine.Spec.csproj

tests/WarHub.ArmouryModel.RosterEngine.Tests/
├── ConformanceTests.cs           (runs all 304 specs)
└── WarHub.ArmouryModel.RosterEngine.Tests.csproj
```

## Related ADRs

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation model (ported from phalanx)
- [ADR-0003](adrs/0003-protocol-based-roster-engine.md) — Why protocol types over ISymbol (superseded by ADR-0006)
- [ADR-0004](adrs/0004-battlescribe-spec-conformance-testing.md) — Conformance testing strategy
- [ADR-0006](adrs/0006-isymbol-based-roster-engine.md) — ISymbol-based engine with compilation-integrated validation
