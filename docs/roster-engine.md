# Roster Engine Architecture

The wham roster engine is a BattleScribe-spec conformant implementation that
passes all 303 conformance specs. It implements the `IRosterEngine` interface
from the BattleScribeSpec TestKit.

## Overview

```
┌─────────────────────────────────────────────────┐
│              WhamRosterEngine                    │
│  (IRosterEngine: Setup, AddForce, SelectEntry,  │
│   GetRosterState, etc.)                         │
├─────────────┬───────────────┬───────────────────┤
│ EntryResolver│ModifierEvaluator│ConstraintValidator│
│ - Entry/link │- Modifier apply│- Min/max checks  │
│   resolution │- Condition eval│- Error generation │
│ - Merging    │- Scope resolve │- Shared constraints│
│ - Flattening │- Repeat count  │- Category checks  │
├──────────────┴───────────────┴───────────────────┤
│  RosterForce / RosterSelection (internal state)  │
└──────────────────────────────────────────────────┘
         ↑ Protocol types from TestKit ↑
```

## Components

### WhamRosterEngine (`WhamRosterEngine.cs`)

The main engine implementing `IRosterEngine`. Manages roster lifecycle:

- **Setup**: Loads game system and catalogues, builds resolver index
- **AddForce/RemoveForce**: Creates force state with auto-selections
- **SelectEntry/SelectChildEntry**: Creates selections, resolves entries
- **GetRosterState**: Builds complete roster state with profiles, rules,
  categories, costs, and validation errors

Key behaviors:
- Auto-select entries with `min > 0` constraints
- Category modifier application (set-primary, add, remove)
- Group categoryLink inheritance to child selections
- Recursive cost type collection from child entries
- Collective cost handling

### EntryResolver (`EntryResolver.cs`)

Resolves entries from catalogues and handles link merging:

- **Available entries**: Flattens root entries from catalogue and game system
- **Child entries**: Flattens children of a selection entry
- **EntryLink merging**: Merges link with target (`merged.Id = target.Id`),
  concatenating constraints, modifiers, and children from both
- **InfoLink resolution**: Resolves profile/rule links to shared definitions
- **InfoGroup collection**: Recursively collects profiles and rules from
  nested info groups (with cycle detection)
- **Group flattening**: Recursively flattens group children (with cycle
  detection)

### ModifierEvaluator (`ModifierEvaluator.cs`)

Evaluates modifiers and conditions using a static `EvalContext`:

- **Modifier types**: set, increment, decrement, append, add, remove
- **Condition types**: atLeast, atMost, equalTo, greaterThan, lessThan,
  instanceOf
- **Scopes**: self, parent, force, roster, primary-category,
  primary-catalogue, ancestor
- **Condition groups**: AND/OR logic with nested conditions
- **Modifier groups**: Shared conditions applied to grouped modifiers
- **Repeat handling**: Multiplicative repeat counts based on conditions
- **Profile/rule modification**: Applies modifiers to characteristic values

### ConstraintValidator (`ConstraintValidator.cs`)

Validates constraints and generates structured error messages:

- **Force-level validation**: Entry constraints scoped to force
- **Child validation**: Parent-scoped constraints on child selections
- **Category validation**: Min/max constraints on category links
- **Shared constraints**: Deduplicated validation across multiple selections
- **Constraint merging**: Absorbs link constraint values into shared
  constraints (uses most restrictive value)
- **Error format**: `on='ownerType ownerEntryId', from='entryId/constraintId'`

### State Types

- **RosterForce**: Force state with selections and available entries
- **RosterSelection**: Selection state with entry, children, source link/group

## BattleScribe Behavioral Alignment

The engine aims to match BattleScribe behavior with documented deviations:

| Behavior | BattleScribe | wham | Notes |
|----------|-------------|------|-------|
| scope=parent on uncategorised entries | No error | Error (correct) | `scope-parent` spec has wham override |
| Null childId + non-self scope | NaN → false | false | Matches BS |
| Shared constraint + link constraint | Absorbs most restrictive | Same | Implemented |
| EntryLink merge ID | target.Id | Same | `merged.Id = target.Id` |
| Auto-select on min constraint | Yes | Yes | On addForce |
| scope=force min constraint owner | "force" | "force" | Correct |
| scope=force max constraint owner | varies | "selection" | Matches BS |

## Known Limitations (Latent)

These are not triggered by current conformance specs but were identified
during code review:

1. **Ancestor scope depth**: Only checks immediate parent, not full ancestor
   chain. Would need parent references on `RosterSelection`.

2. **Child constraint parent context**: `ValidateChildConstraints` evaluates
   modifiers without parent context, so parent/ancestor-scoped conditions
   on child constraints may resolve incorrectly.

3. **Cost conditions use raw costs**: Conditions/constraints that count cost
   fields use `Entry.Costs` directly, not the effective (modified) costs.

4. **Linked child selection identity**: Child selections from entry links may
   be counted under the target entry ID instead of the link ID.

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
