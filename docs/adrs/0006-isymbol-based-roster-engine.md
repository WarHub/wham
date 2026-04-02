# ADR-0006: ISymbol-based roster engine (replacing Protocol-based)

**Status**: Accepted (updated with compilation-integrated validation)

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
  ├── StateMapper               ISymbol roster state → Protocol RosterState
  └── DiagnosticMapper          Diagnostic → ValidationErrorState
        │
WarHub.ArmouryModel.RosterEngine (core engine)
  ├── WhamRosterEngine          Functional API: operations on RosterNode
  └── (ModifierEvaluator)       → moved to Concrete.Extensions
        │
WarHub.ArmouryModel.Concrete.Extensions (compilation + symbols + validation)
  ├── WhamCompilation           Compilation with GetDiagnostics()/GetValidationDiagnostics()
  ├── Symbols/RosterSymbol      ForceComplete with EvaluateModifiers/Validate phases
  ├── Validation/               Constraint and modifier evaluation
  │   ├── ModifierEvaluator     IEffectSymbol eval with runtime context
  │   └── ConstraintValidator   IConstraintSymbol validation → Diagnostic objects
  └── Diagnostics/              Diagnostic infrastructure
      ├── ErrorCode             WRN_ codes for constraint violations
      ├── ValidationDiagnostic  Diagnostic with constraint metadata
      └── DiagnosticBag         Collection and dedup of diagnostics
        │
WarHub.ArmouryModel.EditorServices (existing)
  ├── RosterState               Wraps Compilation (roster + catalogs)
  └── RosterEditor              Undo/redo stack (future integration)
        │
Extensions + Source (existing node layers)
```

### Key Design Choices

1. **Functional engine API**: `WhamRosterEngine` methods take `RosterNode`
   + `Compilation` and return modified `RosterNode`. State is immutable.

2. **Compilation as truth**: The `Compilation` object (with Binder chain)
   provides symbol resolution, entry link targets, and catalogue closure.
   No manual index-building needed for entry resolution.

3. **ModifierEvaluator in Concrete.Extensions**: Evaluates `IEffectSymbol`
   effects with an `EvalContext(Selection?, Force?, EntrySymbol)`. Handles
   name, hidden, costs, characteristics, categories, pages, rule
   descriptions, and constraint values. Moved from RosterEngine to be a
   compilation-internal service.

4. **ConstraintValidator in Concrete.Extensions**: Validates constraints and
   produces `Diagnostic` objects with structured metadata (ownerType,
   ownerId, ownerEntryId, entryId, constraintId). Supports shared counting
   across entry links, constraint merging, and modifier-constraint
   interaction.

5. **Validation via GetValidationDiagnostics()**: Constraint validation
   runs on-demand through `WhamCompilation.GetValidationDiagnostics()`,
   separate from the ForceComplete symbol completion pipeline. The
   `RosterSymbol.Validate` CompletionPart is reserved for future lazy
   validation integration.

   > **Design note**: Initially attempted to integrate validation into
   > `ForceComplete()` phases, but this caused process hang issues due to
   > SpinWait contention in the completion state machine during test host
   > shutdown. The current approach keeps validation explicit and avoids
   > these threading issues.

6. **DiagnosticMapper in Spec adapter**: Translates `Diagnostic` objects
   back to `ValidationErrorState` for the BattleScribeSpec test kit.
   Uses `ValidationDiagnostic` type check (not ID-based filtering) to
   identify constraint violations.

7. **Adapter isolation**: TestKit dependency is confined to `RosterEngine.Spec`.
   The core `RosterEngine` and `Concrete.Extensions` projects have zero
   TestKit dependency.

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
- **Validation as diagnostics** — constraint violations are first-class
  `Diagnostic` objects, same infrastructure as binding errors
- **Clean separation** — validation logic in Concrete.Extensions, spec
  mapping in Spec adapter, engine operations in RosterEngine

### Negative
- **Conversion overhead** — Protocol types must be converted to SourceNode
  trees (one-time cost per setup, not per operation)
- **Validation not lazy** — GetValidationDiagnostics() recomputes on each
  call rather than caching via ForceComplete. Acceptable for current use
  but may need caching for editor scenarios.

### Conformance Results
- 304/304 passing (100%)
- 0 expected failures
