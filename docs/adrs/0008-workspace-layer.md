# ADR-0008: Workspace layer for multi-roster management

**Status**: Accepted

## Context

After Phase 1 (SymbolKey for cross-compilation identity) and Phase 2
(incremental compilation via catalogue/roster split), the next step is managing
multiple rosters and their compilations in a unified workspace.

The existing `RosterEditor` manages a single roster with undo/redo. There is no
centralized concept of "the workspace" containing catalogues plus multiple
rosters, no document identity tracking, no lazy compilation rebuilding, and no
change notification mechanism for UI consumers.

Two ownership models for roster mutations were considered:

1. **Public `GetEditor()`**: Workspace exposes the `RosterEditor` directly.
   Consumers call methods on the editor, and workspace hooks `OperationApplied`
   to stay in sync.

2. **Workspace-owned mutations**: Workspace does NOT expose `RosterEditor`.
   Instead, it provides `ApplyOperation()`, `Undo()`, `Redo()` methods that
   delegate to an internal editor.

## Decision

**Workspace-owned mutations** (option 2). The workspace is the sole owner of
roster editors and the only entry point for state changes.

### Key design choices

- **`WhamWorkspace`** is mutable with lock-based thread safety. Immutable
  internal state is swapped atomically under the lock. Events are fired outside
  the lock to prevent deadlocks.

- **`DocumentId`** is a synthetic GUID assigned at load time. BattleScribe root
  node IDs are not used because they can collide across different files.

- **`CompilationTracker`** (internal) lazily creates per-roster compilations via
  `WhamCompilation.CreateRosterCompilation()`. Thread-safe via
  `Interlocked.CompareExchange`.

- **Catalogue changes reset all roster editors** (undo history is lost).
  Catalogue changes are rare (loading/reloading data files), so this is an
  acceptable trade-off.

- **Versioned events**: `WorkspaceChangedEventArgs` carries a `Version` counter
  so consumers can detect stale events when events are fired outside the lock.

- **Snapshot-then-compute diagnostics**: `GetDiagnosticsAsync` captures the
  compilation reference first, then runs `GetDiagnostics()` on that snapshot
  via `Task.Run`, avoiding races with concurrent edits.

## Rationale

### Why workspace-owned mutations?

`RosterEditor.OperationApplied` only fires on `ApplyOperation()`, not on
`Undo()` or `Redo()`. If the workspace relied on this event to stay in sync,
undo/redo would silently desync the workspace's state from the editor's.

Additionally, exposing `GetEditor()` creates a leaked reference problem:
catalogue changes replace internal editors, but callers holding old editor
references would silently operate on an orphaned editor. By owning mutations,
the workspace guarantees all state changes flow through it.

### Why GUID-based DocumentId?

BattleScribe root node IDs (authored in XML or generated GUIDs for rosters)
could theoretically serve as document identity. However:
- Catalogue IDs may collide if duplicate datasets are loaded
- Root node IDs are nullable
- Synthetic GUIDs are guaranteed unique within the workspace

### Why lock-based thread safety?

At wham's scale (1 gamesystem, <20 catalogues, 1–few rosters), contention is
negligible. Lock-free patterns (`Interlocked.CompareExchange` on full state)
only pay off when the exposed model is fully immutable — but the workspace
exposes mutable operations, so locks are the simpler correct choice.

## Consequences

- UI consumers must route all roster edits through the workspace — direct
  `RosterEditor` usage bypasses workspace tracking.
- Catalogue changes lose undo history for all open rosters. A future
  enhancement could rebase historical states onto the new catalogue compilation.
- `TryFindDocumentId()` provides a bridge from BattleScribe root node IDs to
  synthetic DocumentIds, enabling integration with file-based workflows.
