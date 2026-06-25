# ADR-0006: ISymbol-based roster engine (replacing Protocol-based)

**Status**: Accepted

**Supersedes**: [ADR-0003](0003-protocol-based-roster-engine.md)

## Context

ADR-0003 chose to build the roster engine directly on Protocol types from
BattleScribeSpec.TestKit. While this achieved 303/303 conformance quickly,
it created problems:

- **Two parallel type systems** — the engine reimplemented entry resolution,
  modifier evaluation, and constraint validation from scratch against flat
  Protocol types, ignoring the Roslyn-inspired ISymbol/Compilation layer
- **No connection to EditorServices** — the Protocol engine couldn't
  integrate with RosterState, RosterEditor, or the undo/redo stack
- **Latent issues** — parent context, ancestor scope, and linked identity
  were harder to solve without the ISymbol parent-tracking chain

## Decision

**Rewrite the engine core to operate on ISymbol/SourceNode/WhamCompilation
types.** The BattleScribe-spec adapter becomes a thin translation layer.

### Architecture

```
BattleScribeSpec.TestKit (Protocol types, IRosterEngine interface)
        │
WarHub.ArmouryModel.RosterEngine.Spec (adapter project)
  ├── ProtocolConverter         Protocol* → SourceNode trees → WhamCompilation
  ├── SpecRosterEngineAdapter   IRosterEngine impl, delegates to core
  └── StateMapper               ISymbol roster state → Protocol RosterState
        │
WarHub.ArmouryModel.RosterEngine (core engine)
  ├── WhamRosterEngine          Functional API: operations on RosterNode
  ├── ModifierEvaluator         IEffectSymbol eval with runtime context
  └── ConstraintValidator       IConstraintSymbol/IQuerySymbol validation
        │
WarHub.ArmouryModel.EditorServices (existing)
  ├── RosterState               Wraps Compilation (roster + catalogs)
  └── RosterEditor              Undo/redo stack (future integration)
        │
Concrete.Extensions + Extensions + Source (existing symbol/node layers)
```

### Key Design Choices

1. **Functional engine API**: `WhamRosterEngine` methods take `RosterNode`
   + `Compilation` and return modified `RosterNode`. State is immutable.

2. **Compilation as truth**: The `Compilation` object (with Binder chain)
   provides symbol resolution, entry link targets, and catalogue closure.
   No manual index-building needed for entry resolution.

3. **ModifierEvaluator**: Evaluates `IEffectSymbol` effects with an
   `EvalContext(Selection?, Force?, EntrySymbol)`. Handles name, hidden,
   costs, characteristics, categories, pages, rule descriptions, and
   constraint values.

4. **ConstraintValidator**: Validates `IConstraintSymbol` constraints with
   shared counting across entry links, constraint merging (most restrictive
   wins), and modifier-constraint interaction.

5. **Adapter isolation**: TestKit dependency is confined to `RosterEngine.Spec`.
   The core `RosterEngine` project has zero TestKit dependency.

## Consequences

### Positive
- **Single type system** — engine operates on the same ISymbol types used
  by the Binder, EditorServices, and tooling
- **Leverages Binder** — entry link resolution, shared entry lookup, and
  catalogue closure handled by the existing Compilation
- **EditorServices integration** — engine output is a `RosterNode` that can
  be wrapped in `RosterState` for editor integration
- **Better parent tracking** — ISymbol parent chain enables correct scope
  resolution for ancestor/parent queries

### Negative
- **Conversion overhead** — Protocol types must be converted to SourceNode
  trees (one-time cost per setup, not per operation)
- **Two engines coexist temporarily** — legacy Protocol engine kept in
  `Legacy/` namespace for reference until fully retired

### Conformance Results
- 291/293 passing (99.3%)
- 2 expected failures (`undefined-behavior` specs that all engines fail)
