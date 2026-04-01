# ADR-0003: Protocol-based roster engine (direct TestKit types)

**Status**: Accepted

## Context

The BattleScribe-spec TestKit defines an `IRosterEngine` interface and a
set of `Protocol*` types (`ProtocolGameSystem`, `ProtocolCatalogue`,
`ProtocolSelectionEntry`, etc.) that represent the BattleScribe data model
in a simplified, JSON-serializable form. The conformance test suite sends
protocol messages to the engine and validates roster state against expected
outcomes.

wham already has a rich type system (SourceNode, ISymbol) for BattleScribe
data. The question: should the roster engine convert protocol types into
wham's internal types, or work with protocol types directly?

## Decision

**Work directly with protocol types** from `BattleScribeSpec.TestKit`.

The roster engine (`WhamRosterEngine`) accepts `ProtocolGameSystem` and
`ProtocolCatalogue` objects, builds internal state (`RosterForce`,
`RosterSelection`), and returns `RosterState` objects — all using the
TestKit's protocol types without converting to SourceNode or ISymbol.

### Architecture

```
IRosterEngine interface (from TestKit)
    ↓
WhamRosterEngine
    ├── EntryResolver     — entry/link resolution, merging, flattening
    ├── ModifierEvaluator — modifier application, condition evaluation
    ├── ConstraintValidator — min/max validation, error generation
    ├── RosterForce       — force state (selections, available entries)
    └── RosterSelection   — selection state (entry, children, number)
```

### Key Design Choices

1. **EntryLink merging**: Links are merged with their targets at resolution
   time. `merged.Id = target.Id` (BattleScribe behavior). Constraints,
   modifiers, and children from both target and link are concatenated.

2. **Modifier evaluation via EvalContext**: A static evaluation model where
   `EvalContext` carries `AllForces`, `Force`, `Selection`, `ParentSelection`,
   and `OwnerEntryId` — enabling scope resolution without tree navigation.

3. **Constraint validation**: Two-pass validation — first for force-level
   entry constraints (including shared constraints), then for child selection
   constraints and category constraints.

4. **Category inheritance**: SelectionEntryGroup's categoryLinks propagate
   to child entries when creating selections.

## Consequences

### Positive
- **Zero conversion overhead** — no SourceNode/ISymbol translation layer
- **Direct spec alignment** — engine speaks the same language as the test suite
- **Simpler debugging** — protocol types are plain DTOs, easy to inspect
- **Fast iteration** — changes to engine logic don't require changes to
  the type system

### Negative
- **Parallel type systems** — wham now has two representations of BattleScribe
  data (SourceNode/ISymbol AND Protocol types)
- **No reuse of existing binding** — the Roslyn-inspired Binder chain is not
  used by the engine; entry resolution is reimplemented in `EntryResolver`
- **Future integration work** — connecting the protocol-based engine to the
  SourceNode editing pipeline will require a translation layer

### Future Direction
The protocol-based engine proves the algorithmic correctness against the
conformance spec. A future phase could:
1. Add a thin adapter that converts SourceNode trees to protocol types
2. Or refactor the engine to work with ISymbol types directly
3. Or keep both paths (protocol for testing, ISymbol for rich editing)
