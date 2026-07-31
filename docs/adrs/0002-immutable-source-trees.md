# ADR-0002: Immutable source trees and operation-based editing

**Status**: Accepted

## Context

Roster editing requires undo/redo support, consistent state across
operations, and the ability to derive fully-bound semantic models from any
point in the editing history. Mutable state makes these guarantees difficult.

This design was originally developed in phalanx, inspired by Roslyn's
red/green tree model.

## Decision

Adopt **immutable source trees** with **first-class operation objects**:

- **`SourceTree`**: Wraps a root `SourceNode` with a file path
- **`IRosterOperation`**: Interface for all edit operations (e.g.,
  `AddSelectionOperation`, `RemoveForceOperation`)
- **`RosterState`**: Immutable record holding current `WhamCompilation`
  and roster's `SourceTree`
- **`RosterEditor`**: Maintains `ImmutableStack` instances for undo/redo
- **`SourceRewriter`**: Visitor enabling immutable transformations

### Operation Flow

```
User Action → IRosterOperation → RosterState (new immutable snapshot)
                                      ↕
                            RosterEditor undo/redo stacks
```

## Consequences

### Positive
- Trivial undo/redo (swap state snapshots)
- Consistency guaranteed (operations produce valid state or unchanged)
- Structural sharing reduces memory for similar trees
- Operations are debuggable first-class objects

### Negative
- Allocation pressure on frequent edits
- Two overlapping base types for building operations
- Several operations have incomplete implementations (TODOs)
