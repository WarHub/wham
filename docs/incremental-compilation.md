# Incremental Compilation

wham's compilation model supports **incremental compilation** by splitting a
monolithic `WhamCompilation` into a stable **catalogue compilation** (shared
across rosters) and lightweight **roster compilations** that reference it.

## Motivation

When editing a roster, the common operation is modifying selections — adding a
unit, changing a count, removing a force. In the original model, every such edit
called `ReplaceSourceTree()`, which rebuilt **all** symbols from scratch:
catalogue bindings, profile resolutions, and constraint evaluations — even
though catalogues hadn't changed.

For a dataset with 50 catalogue entries (each with profiles, rules, and
constraints), a single roster edit took **~25 ms** and allocated **~2.9 MB**.
With incremental compilation it takes **~25 µs** and allocates **~17 KB** — a
**1,000× improvement** in both time and memory.

## Design: CatalogueReference Model

Rather than separate `CatalogueCompilation`/`RosterCompilation` subclasses,
`Compilation` has a `CatalogueReference` property (analogous to Roslyn's
`ProjectReference`), with `WhamCompilation` providing the concrete covariant
override:

```
Catalogue Compilation                Roster Compilation
┌─────────────────────┐              ┌─────────────────────┐
│ SourceTrees:        │              │ SourceTrees:        │
│   gamesystem.gst    │◄─────────── │   roster.ros        │
│   catalogue.cat     │  CatRef      │                     │
│                     │              │ CatalogueReference: │
│ GlobalNamespace:    │              │   catalogue comp    │
│   RootCatalogue     │              │                     │
│   Catalogues [...]  │              │ GlobalNamespace:    │
│   Rosters []        │              │   RootCatalogue ◄───── same object
└─────────────────────┘              │   Catalogues ◄──────── same objects
                                     │   Rosters [roster]  │
                                     └─────────────────────┘
```

**Key properties:**

- **Catalogue compilation** = compilation with catalogue/gamesystem trees,
  no `CatalogueReference`, no roster trees.
- **Roster compilation** = compilation with roster tree(s), references
  exactly one catalogue compilation. **Options are always inherited** from the
  catalogue reference (structurally enforced — the roster constructor has no
  options parameter).
- `CatalogueReference` is defined on abstract `Compilation` as `virtual Compilation?`
  (defaults to `null`). `WhamCompilation` overrides with covariant return
  `WhamCompilation?`, making multiple references structurally impossible.
- Catalogue symbols are **shared by object reference** — not duplicated.
- Catalogue symbols' `DeclaringCompilation` always points to the catalogue
  compilation (through the `ContainingNamespace` chain).

### Invariants (enforced at construction time)

1. No chained references (a referenced compilation must not itself have a catalogue reference)
2. Catalogue compilations must not contain `RosterNode` source trees
3. Roster compilations must contain only `RosterNode` source trees
4. Options always match between roster and catalogue compilations (by construction)
5. `AddSourceTrees()` validates tree type consistency; `ReplaceSourceTree()` rejects kind changes

### API

```csharp
// Create a catalogue compilation (standalone, no roster)
var catComp = WhamCompilation.Create(catalogueTrees);

// Create from mixed trees — auto-splits into catalogue + roster compilation
var comp = WhamCompilation.Create(allTrees);  // returns roster compilation
comp.CatalogueReference  // the auto-created catalogue subcompilation
comp.AllSourceTrees       // all trees (catalogue + roster), for round-tripping

// Create a roster compilation referencing the catalogue (inherits options)
var rosterComp = WhamCompilation.CreateRosterCompilation(
    rosterTrees, catComp);

// Edit a roster — catalogue compilation is reused
var editedComp = rosterComp.ReplaceSourceTree(oldTree, newTree);
// editedComp.CatalogueReference is still catComp — same object

// Add roster trees to a catalogue compilation
var rosterComp = catComp.AddRosterTrees(rosterTree);
// rosterComp.CatalogueReference is catComp

// Multiple rosters can share one catalogue compilation
var roster1 = WhamCompilation.CreateRosterCompilation([tree1], catComp);
var roster2 = WhamCompilation.CreateRosterCompilation([tree2], catComp);
```

## Benchmark Results

Measured with BenchmarkDotNet (`--job Short`, .NET 10, Release mode).
Source: `tests/WarHub.ArmouryModel.Benchmarks/`.

### Scenario: Create compilation from scratch

| Dataset | Full Rebuild | Incremental | Speedup | Memory Saved |
|---------|-------------|-------------|---------|--------------|
| Sample (3 catalogues + 1 roster) | 3,328 µs / 929 KB | 1,596 µs / 455 KB | **2.1×** | **51%** |
| Synthetic (50 entries, profiles, rules) | 23,981 µs / 2,908 KB | 26 µs / 17 KB | **908×** | **99.4%** |

### Scenario: Roster edit (replace tree + re-complete)

| Dataset | Full Rebuild | Incremental | Speedup | Memory Saved |
|---------|-------------|-------------|---------|--------------|
| Sample (3 catalogues + 1 roster) | 5,186 µs / 945 KB | 1,425 µs / 455 KB | **3.6×** | **52%** |
| Synthetic (50 entries, profiles, rules) | 25,493 µs / 2,908 KB | 25 µs / 17 KB | **1,014×** | **99.4%** |

The sample dataset is small (the embedded "Spec Kata Wars 9000" test data),
so the improvement is modest. The synthetic dataset (50 selection entries with
3 profiles, 2 rules, and constraints each) is closer to a real-world army book
and shows the dramatic savings.

### Why are the savings so large?

In the incremental model, a roster-only compilation:

1. **Skips catalogue symbol creation** — they already exist in the referenced
   compilation and are reused by reference.
2. **Skips binder chain resolution for catalogues** — all catalogue symbols are
   fully bound before the roster compilation is created.
3. **Only creates roster-specific symbols** — `RosterSymbol`, `ForceSymbol`,
   `SelectionSymbol` — which are a tiny fraction of total symbols.
4. **Diagnostics aggregate lazily** — catalogue diagnostics are read from the
   referenced compilation, not recomputed.

## Running the Benchmarks

```bash
cd tests/WarHub.ArmouryModel.Benchmarks
dotnet run -c Release -- --filter "*" --job Short
```

To run a specific scenario:

```bash
dotnet run -c Release -- --filter "*Synth*Edit*"
```

## Related

- [ADR-0001](adrs/0001-roslyn-inspired-compilation-model.md) — Compilation
  model architecture
- [ADR-0007](adrs/0007-incremental-compilation-references.md) — Decision record
  for the references model
- [Roster Engine Architecture](roster-engine.md) — How the engine uses
  compilations
