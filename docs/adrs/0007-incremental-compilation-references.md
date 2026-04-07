# ADR-0007: Incremental compilation via references model

**Status**: Accepted

## Context

`WhamCompilation.ReplaceSourceTree()` creates a new compilation from scratch,
rebuilding all symbols — catalogue bindings, profile resolution, constraint
evaluation — even when only the roster tree changed. For realistic datasets
(50+ catalogue entries with profiles, rules, constraints), a single roster edit
took ~25 ms and allocated ~3 MB.

The roster engine's primary use case is interactive editing: add a unit, change
a count, remove a force. These operations modify only the roster tree. Catalogue
data (game systems, army books) changes infrequently and independently of roster
edits.

Two architectural approaches were considered:

1. **Separate subclasses** (`CatalogueCompilation` / `RosterCompilation`):
   Type-safe but requires splitting the `WhamCompilation` hierarchy, moving
   factory methods, and updating every consumer to handle two types.

2. **References model on `WhamCompilation`**: A `References` property (analogous
   to Roslyn's `ProjectReference`) that lets a roster compilation borrow symbols
   from a pre-built catalogue compilation. Single type, runtime invariants
   enforce the separation.

## Decision

Use the **references model** on `WhamCompilation`.

A catalogue compilation is a `WhamCompilation` with only catalogue/gamesystem
source trees and no references. A roster compilation has only roster source
trees and references exactly one catalogue compilation.

When a roster edit occurs via `ReplaceSourceTree()`, only the roster compilation
is rebuilt. The catalogue compilation — and all its symbols — is reused by
object reference.

### Runtime invariants (enforced in constructor)

1. Referenced compilations must not themselves have references (no chains).
2. Roster compilations (compilations with references) must contain only
   `RosterNode` source trees.
3. `Update()` preserves references across tree replacements.

### Why not separate classes?

Separate classes add type safety but don't eliminate the real complexity:
diagnostics aggregation, completion ordering, and consumer API assumptions.
The references model achieves the same safety with runtime invariants in a
single type, with significantly less code churn.

The `References` property lives on `WhamCompilation` (not abstract
`Compilation`) until the semantics are proven through Phase 3 (workspace
layer).

## Consequences

### Positive

- **~1,000× faster roster edits** on realistic datasets (25 µs vs 25 ms)
- **99%+ less memory allocation** per edit (17 KB vs 2.9 MB)
- Multiple rosters can share a single catalogue compilation
- Catalogue symbols are identical object references across rosters (enables
  `SymbolKey` cross-resolution without remapping)
- Minimal consumer changes — `SpecRosterEngineAdapter` and `StateMapper`
  required zero modifications due to namespace adaptation
- Foundation for Phase 3 workspace layer (multi-roster management)

### Negative

- Runtime invariants instead of compile-time type safety — violations are
  caught at construction time, not at the call site
- `GetDiagnostics()` must aggregate diagnostics from referenced compilations
  (adds complexity to diagnostic reporting)
- `FindSourceTree()` must fall through to referenced compilations
- `ForceComplete` behavior depends on `HasCatalogueReference` — catalogue-only
  compilations complete all symbols, roster compilations complete only rosters

### Risks

- **Stale catalogue compilation**: If catalogue data changes, all roster
  compilations referencing it must be rebuilt. The workspace layer (Phase 3)
  will manage this lifecycle.
- **Thread safety**: Catalogue compilation must be fully completed before
  creating roster compilations. Currently enforced by calling
  `GetDiagnostics()` or `ForceComplete()` on the catalogue compilation first.

## Benchmark Evidence

BenchmarkDotNet results (`--job Short`, .NET 10):

| Scenario | Full Rebuild | Incremental | Speedup |
|----------|-------------|-------------|---------|
| 50-entry create + complete | 23,981 µs | 26 µs | 908× |
| 50-entry roster edit | 25,493 µs | 25 µs | 1,014× |
| Sample dataset create | 3,328 µs | 1,596 µs | 2.1× |
| Sample dataset edit | 5,186 µs | 1,425 µs | 3.6× |

See [incremental-compilation.md](../incremental-compilation.md) for full
analysis.
