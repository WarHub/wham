# Diagnostics, Validation, and Threading

This document records the investigation, design decisions, workarounds, and
open questions related to the wham diagnostic infrastructure, constraint
validation pipeline, and threading model. It serves as context for future work
in this area.

## Table of Contents

- [Background](#background)
- [Diagnostic Infrastructure](#diagnostic-infrastructure)
- [Validation Pipeline](#validation-pipeline)
- [The Process Hang Investigation](#the-process-hang-investigation)
- [Catalogue Resolution Bug](#catalogue-resolution-bug)
- [DiagnosticMapper ID Bug](#diagnosticmapper-id-bug)
- [Force Constraint Owner Bug](#force-constraint-owner-bug)
- [Code Review Findings](#code-review-findings)
- [Open Questions and Future Work](#open-questions-and-future-work)

---

## Background

The wham compilation model is inspired by Roslyn (the C# compiler). In Roslyn,
symbol completion is lazy: `ForceComplete()` walks through `CompletionPart`
flags, computing each part on-demand and caching results. Diagnostics are
produced as side effects of this completion and accumulated in `DiagnosticBag`
collections. Calling `Compilation.GetDiagnostics()` triggers full completion
of all symbols, then returns the collected diagnostics.

We ported this model to wham for BattleScribe data: catalogues, entries, and
rosters are symbols; binding resolves references; diagnostics report errors.

When we added constraint validation (min/max selection counts, cost limits,
force counts, category counts), we initially tried to follow Roslyn's pattern
exactly: make validation a `CompletionPart` on `RosterSymbol`, run it during
`ForceComplete()`, and return validation diagnostics alongside binding errors
from `GetDiagnostics()`.

**This didn't work.** The investigation below explains why.

---

## Diagnostic Infrastructure

### Type Hierarchy

```
Diagnostic (abstract, in Source layer)
├── DiagnosticWithInfo (internal abstract, carries DiagnosticInfo)
│   ├── WhamDiagnostic (internal sealed, general binding/declaration errors)
│   └── ValidationDiagnostic (internal sealed, constraint violations with metadata)
```

### Key Classes

| Class | Project | Purpose |
|-------|---------|---------|
| `ErrorCode` | Concrete.Extensions | Enum of all error/warning codes |
| `ErrorFacts` | Concrete.Extensions | Severity, messages, warning levels |
| `WhamMessageProvider` | Concrete.Extensions | Roslyn `CommonMessageProvider` impl |
| `WhamDiagnostic` | Concrete.Extensions | General diagnostic with location |
| `WhamDiagnosticInfo` | Concrete.Extensions | Carries ErrorCode + format args |
| `ValidationDiagnostic` | Concrete.Extensions | Extends `DiagnosticWithInfo` with constraint metadata |
| `DiagnosticBag` | Source layer | Thread-safe diagnostic collection |
| `BindingDiagnosticBag` | Concrete.Extensions | Binding-specific bag with dependency tracking |
| `DiagnosticMapper` | RosterEngine.Spec | Maps `Diagnostic` → `ValidationErrorState` |

### Error Code Ranges

```csharp
// Binding/declaration errors (severity: Error)
ERR_GenericError                   = 0
ERR_SyntaxSupportNotYetImplemented = 1
ERR_UnknownEnumerationValue        = 2
ERR_MissingGamesystem              = 3
ERR_MultipleGamesystems            = 4
ERR_UnknownModuleType              = 5
ERR_NoBindingCandidates            = 6
ERR_MultipleViableBindingCandidates = 7
ERR_UnviableBindingCandidates      = 8

// Validation warnings (severity: Warning, range 100-199)
WRN_ConstraintMinViolation  = 100
WRN_ConstraintMaxViolation  = 101
WRN_CostLimitExceeded       = 102
WRN_ForceCountViolation     = 103
WRN_CategoryCountViolation  = 104
```

Severity is derived from the `ErrorCode` enum member name prefix:
- `ERR_` → `DiagnosticSeverity.Error`
- `WRN_` → `DiagnosticSeverity.Warning`
- `INF_` → `DiagnosticSeverity.Info`
- `HDN_` → `DiagnosticSeverity.Hidden`

### Diagnostic ID Format

The `WhamMessageProvider.CodePrefix` is `"WHAM"`. Diagnostic IDs are formatted as:

```
WHAM + errorCode.ToString("0000")
```

Examples: `WHAM0000` (generic error), `WHAM0100` (min constraint violation).

**Important**: Do NOT filter diagnostics by ID string prefix (e.g., `id.StartsWith("WRN")`).
The ID is `WHAM0100`, not `WRN_ConstraintMinViolation`. Use type checks instead:
`diag is ValidationDiagnostic`.

### ValidationDiagnostic Metadata

`ValidationDiagnostic` carries structured metadata for mapping to BattleScribeSpec's
`ValidationErrorState`:

| Property | Description | Example |
|----------|-------------|---------|
| `OwnerType` | Type of the entity that owns the constraint violation | `"selection"`, `"category"`, `"force"`, `"roster"` |
| `OwnerId` | ID of the owner instance (selection/force ID in roster) | `null` for most cases |
| `OwnerEntryId` | Entry ID of the owner (entry definition ID) | `"se-unit-a"`, `"cat-hq"` |
| `EntryId` | Entry whose constraint was violated | `"se-unit-a"` |
| `ConstraintId` | ID of the specific constraint | `"con-max-1"` |

The spec test format is: `on='ownerType ownerEntryId', from='entryId/constraintId'`.

---

## Validation Pipeline

### Architecture

```
Caller (SpecRosterEngineAdapter, editor, etc.)
    │
    ▼
WhamCompilation.GetValidationDiagnostics(forceCatalogues?)
    │
    ▼
ConstraintValidator.Validate(rosterNode, compilation, diagnosticBag, forceCatalogues?)
    │ (creates instance, builds index, runs validation)
    ▼
AddValidationDiagnostic(...) → ValidationDiagnostic → DiagnosticBag
    │
    ▼
DiagnosticMapper.MapValidationDiagnostics(diagnostics) → ValidationErrorState[]
```

### Two Separate Diagnostic Paths

1. **Binding/declaration diagnostics** (`GetDiagnostics()`):
   - Produced during symbol completion (reference binding, member construction)
   - Stored in `DeclarationDiagnostics` bag on `WhamCompilation`
   - Triggered by `ForceComplete()` on all symbols
   - Examples: missing gamesystem, unresolvable references, unknown enum values

2. **Validation diagnostics** (`GetValidationDiagnostics()`):
   - Produced by `ConstraintValidator` running on `RosterNode` data
   - Created fresh on each call (not cached)
   - Not part of `ForceComplete()` pipeline
   - Examples: min/max constraint violations, cost limits, force counts

**These two paths are intentionally separate.** See [The Process Hang Investigation](#the-process-hang-investigation).

### ConstraintValidator Lifecycle

Each call to `GetValidationDiagnostics()` creates a **fresh** `ConstraintValidator`
instance. The validator is not shared between threads or calls. Its lifecycle:

1. **Construction**: Receives `RosterNode`, `WhamCompilation`, `forceCatalogues`
2. **BuildIndex()**: Indexes all catalogues' entries, categories, force entries,
   and shared entry link mappings into dictionaries
3. **Run()**: Iterates forces → root entries → child entries → constraints
4. **Disposal**: Instance is garbage collected after `Run()` returns

This per-call instantiation means the dictionaries (`_entryIndex`, `_categoryIndex`,
`_sharedEntryLinkIds`) are never shared between threads, eliminating the race
conditions that code reviewers flagged on the `_sharedEntryLinkIds` check-then-act
pattern. However, if `GetValidationDiagnostics()` were ever cached or made lazy
(via ForceComplete), thread safety would need revisiting.

### Catalogue Resolution

Force entries (defining force types like "Patrol" or "Battalion") typically live
in the gamesystem. Selection entries (units, upgrades) live in separate catalogues.
When creating a `ForceNode`, `NodeFactory.Force()` sets `CatalogueId` to the
ancestor catalogue of the force entry definition — which is the gamesystem.

But for validation, we need the catalogue that contains the **selection entries**
for that force, not the gamesystem. The `SpecRosterEngineAdapter` tracks the
correct catalogue via `_forceCatalogues` (populated during `AddForce()`).

`GetValidationDiagnostics()` accepts an optional `forceCatalogues` parameter.
When provided, it overrides the `ForceNode.CatalogueId`-based lookup. When not
provided, `ResolveForceCatalogues()` falls back to matching `CatalogueId` against
compilation catalogues, with `RootCatalogue` (gamesystem) as the ultimate fallback.

---

## The Process Hang Investigation

### Symptom

When running the full 304-test conformance suite, the `dotnet test` host process
would complete all tests (xUnit showed ~2s total test time, 0 failures at the time)
but **never exit**. The process hung indefinitely after the test runner reported
results. Killing the process and re-running produced the same hang.

### Investigation Steps

1. **Checked for infinite loops**: Added logging to `ConstraintValidator.Validate()`.
   Validation completed quickly — not the cause.

2. **Checked for deadlocks**: Suspected `SpinWait` in `SymbolCompletionState`
   could cause thread pool starvation. `SpinWaitComplete()` busy-waits until
   a `CompletionPart` is marked complete. In normal Roslyn usage, another thread
   completes the part. But in our single-threaded test scenario, if validation
   took a long time or had re-entrancy, the SpinWait could never resolve.

3. **Checked for orphan processes**: Found 18+ orphaned `testhost.exe` processes
   from previous killed runs, all holding locks on DLLs.

4. **Isolated the trigger**: The hang occurred only when `GetDiagnostics()` was
   called, which triggered `ForceComplete()`, which ran validation inside the
   `CompletionPart.Validate` phase. Removing the validation call from
   `ForceComplete()` eliminated the hang.

5. **Root cause hypothesis**: When `ForceComplete()` runs on a `RosterSymbol`,
   it processes each `CompletionPart` in order. The `Validate` part called
   `ConstraintValidator.Validate()`, which called `_modifierEvaluator` methods
   that internally could trigger `ForceComplete()` on OTHER symbols (entry
   symbols, catalogue symbols) through the Binder chain. This recursive
   `ForceComplete()` was likely causing contention on the `volatile int
   completeParts` field via `SpinWaitComplete()`, leading to live-lock or
   preventing the .NET thread pool from draining cleanly at process exit.

   The exact mechanism is unclear — it may be that SpinWait's escalation to
   `Thread.Sleep()` calls created thread pool threads that never terminated,
   or that the volatile field access pattern prevented the CLR from detecting
   "all work is done" at shutdown.

### Resolution

**Moved validation out of `ForceComplete()`**. The `CompletionPart.Validate`
phase now just marks itself complete immediately:

```csharp
case CompletionPart.Validate:
    // Validation runs via compilation.GetValidationDiagnostics() externally
    // to avoid holding symbol completion open during heavy work.
    state.NotePartComplete(CompletionPart.Validate);
    break;
```

Validation runs on-demand via `WhamCompilation.GetValidationDiagnostics()`,
which creates a fresh `ConstraintValidator`, runs it, and returns the results.
This approach:

- **Eliminates the hang**: No `SpinWait` contention during validation
- **Keeps binding diagnostics lazy**: `ForceComplete()` still handles reference
  binding and member completion normally
- **Trades caching for correctness**: Validation recomputes on each call instead
  of being cached in the symbol tree

### Impact on API

`WhamCompilation.GetDiagnostics()` no longer returns validation diagnostics.
This is documented in the method's XML doc comment. Callers needing validation
must call `GetValidationDiagnostics()` separately.

This was flagged by all three code reviewers (Opus 4.6, GPT-5.4, Codex 5.3)
as a potential API break. For now it's acceptable because:

1. The only consumer is `SpecRosterEngineAdapter`, which calls
   `GetValidationDiagnostics()` directly
2. Future editor integration will need a different validation trigger anyway
   (e.g., on-demand after user actions, not on every `GetDiagnostics()` call)

### Future: Re-integrating Validation into ForceComplete

If we want to restore lazy cached validation, the path forward is:

1. **Profile the SpinWait issue**: Determine exactly which re-entrant
   `ForceComplete()` calls cause contention. It may be that guarding against
   recursive completion (Roslyn uses `Interlocked.CompareExchange` for this)
   is sufficient.

2. **Run validation on a separate thread pool task**: Instead of blocking in
   `ForceComplete()`, queue validation work and let `SpinWaitComplete()` wait
   for it naturally.

3. **Break the re-entrant dependency**: Ensure `ConstraintValidator` doesn't
   trigger `ForceComplete()` on other symbols. This may require pre-computing
   modifier values before validation runs.

4. **Use CancellationToken**: Pass the cancellation token through to validation
   so that `SpinWait` can honor cancellation at test host shutdown.

---

## Catalogue Resolution Bug

### Symptom

25 constraint tests produced `got 0: []` — zero validation errors. All were
constraint tests that should have found violations on selection entries.

### Investigation

The `ConstraintValidator` uses `_forceCatalogues[forceIndex]` to determine which
catalogue's entries to validate for each force. The internal
`ResolveForceCatalogues()` method resolves `ForceNode.CatalogueId` against the
compilation's catalogues.

But `ForceNode.CatalogueId` is set to the **gamesystem** ID (because force
entries live in the gamesystem), not the catalogue containing selection entries.
So `ResolveForceCatalogues()` was returning the gamesystem for every force.

The gamesystem's root entries are force entries, not selection entries. So
`ValidateForceSelections()` iterated zero selection entries and produced zero
diagnostics.

### Resolution

Added `forceCatalogues` parameter to `GetValidationDiagnostics()`. The
`SpecRosterEngineAdapter` passes its tracked `_forceCatalogues` list (populated
during `AddForce()` with the actual catalogue the user selected). This gives the
validator the correct catalogues for entry lookup.

When `forceCatalogues` is not provided (e.g., direct API use), the fallback
`ResolveForceCatalogues()` still runs. The fallback uses `RootCatalogue`
(gamesystem) when no match is found — documented with a comment explaining
this is correct for the gamesystem case.

### Future Consideration

For non-spec API consumers, the `forceCatalogues` parameter is awkward. The
compilation should ideally be able to determine the correct catalogue for each
force without external help. This requires either:

- Storing the catalogue association in the `ForceNode` itself (e.g., adding a
  `SourceCatalogueId` field separate from the gamesystem-derived `CatalogueId`)
- Or using the Binder to resolve which catalogue provides the entries that
  selections in a force reference

---

## DiagnosticMapper ID Bug

### Symptom

After switching to the new `ValidationDiagnostic` type, the `DiagnosticMapper`
was filtering diagnostics by checking `id.StartsWith("WRN")`. This matched zero
diagnostics, causing all constraint tests to report zero validation errors.

### Root Cause

Diagnostic IDs are formatted as `CodePrefix + errorCode.ToString("0000")`:
- `WRN_ConstraintMinViolation` (code 100) → ID is `WHAM0100`, not `WRN_ConstraintMinViolation`
- The `WRN_` prefix is only on the `ErrorCode` enum member name, used for
  severity derivation — it's not part of the diagnostic ID string.

### Resolution

Changed the filter from `id.StartsWith("WRN")` to `diag is ValidationDiagnostic`.
Type-based filtering is more robust and doesn't depend on ID format knowledge.

### Lesson

When working with the diagnostic infrastructure, always use type checks or
`ErrorCode` enum comparisons, not string-based ID filtering. The ID format is
an implementation detail of `WhamMessageProvider`.

---

## Force Constraint Owner Bug

### Symptom

Three force-constraint tests failed:
- `constraint-forces-field-on-forceentry`: Expected `on='roster'`, got `on='force'`
- `constraint-forces-field-per-type`: Same
- `constraint-forces-field-min-error`: Same

### Root Cause

`ValidateForceEntryConstraints()` was setting `ownerType = "force"` for force
count violations. But force count constraints are roster-level: they count how
many forces of a given type exist **in the roster**. The owner should be `"roster"`.

Additionally, `constraint-forces-field-on-forceentry` expected 3 errors (one per
force instance) but got 1, because we had deduplicated by entry type.

### Resolution

1. Changed `ownerType` from `"force"` to `"roster"` for force count constraints
2. Changed `ownerEntryId` from `force.EntryId` to `null` (roster has no entry ID)
3. Removed the deduplication (each force instance evaluates its constraints
   independently, producing one error per instance per the spec)

### Spec Reference

The battlescribe-spec YAML files contain explicit comments:
> "Each Patrol force instance evaluates the constraint independently"

This means 3 patrol forces exceeding max:2 → 3 errors, not 1.

---

## Code Review Findings

Three-model review panel (Opus 4.6, GPT-5.4, Codex 5.3) findings:

### 1. GetDiagnostics() API Break (all three reviewers)

**Finding**: `GetDiagnostics()` no longer returns validation diagnostics.

**Response**: Documented intentionally. See [Process Hang Investigation](#the-process-hang-investigation).

### 2. Thread-unsafe `_sharedEntryLinkIds` (Opus 4.6)

**Finding**: Race condition in `BuildIndex()` — check-then-act pattern on
`_sharedEntryLinkIds` dictionary.

**Response**: Not a real risk currently — each `Validate()` call creates its own
`ConstraintValidator` instance. The dictionaries are never shared. Would become
a real issue if validation were cached or made lazy via `ForceComplete()`.

### 3. ResolveForceCatalogues Silent Fallback (Opus 4.6)

**Finding**: When `CatalogueId` doesn't match any catalogue, silently falls back
to `RootCatalogue` without diagnostic.

**Response**: Added explanatory comment. The fallback is correct for the common
case (gamesystem). A diagnostic could be added for debugging but would be noisy
in normal operation.

### 4. Multi-roster `forceCatalogues` (GPT-5.4)

**Finding**: `GetValidationDiagnostics()` loops all rosters but passes the same
`forceCatalogues` to each, which only describes one roster's forces.

**Response**: Added `<remarks>` documenting the single-roster assumption. Current
usage is always single-roster per compilation. Will need refactoring for
multi-roster support.

---

## Open Questions and Future Work

### Lazy Validation Caching

The current approach recomputes validation on every `GetValidationDiagnostics()`
call. For editor scenarios with frequent validation requests, this could be
expensive. Options:

1. **Dirty-flag caching**: Cache validation results on `WhamCompilation`, invalidate
   when source trees change (compilations are already immutable — new compilation
   on each change, so cache per instance)
2. **Re-integrate into ForceComplete**: Fix the SpinWait issue and restore lazy
   evaluation. Requires understanding the exact re-entrancy pattern.
3. **Incremental validation**: Only validate changed forces/selections. Requires
   diff tracking between compilation versions.

### Unifying GetDiagnostics

In Roslyn, `GetDiagnostics()` returns ALL diagnostics (syntax + semantic + flow).
Our split into `GetDiagnostics()` + `GetValidationDiagnostics()` is unusual.
Future option: make `GetDiagnostics()` call `GetValidationDiagnostics()` internally,
but only after confirming the SpinWait issue is fully resolved.

### Force Catalogue Association

The `forceCatalogues` parameter is a leaky abstraction. The compilation should
know which catalogue provides entries for each force. This likely requires:

- Enriching `ForceNode` with the source catalogue ID
- Or teaching the Binder to associate forces with their catalogues
- Or using `RosterState` which already tracks force-catalogue associations

### Location Information

Currently all `ValidationDiagnostic` objects use `Location.None`. In the future,
diagnostics should carry location information pointing to the relevant SourceNode
(the selection, force, or roster that has the violation). This would enable
IDE-style error highlighting.

### ValidationDiagnostic Visibility

`ValidationDiagnostic` is `internal` because its base class `DiagnosticWithInfo`
is internal. Access from the Spec project is via `InternalsVisibleTo`. If other
projects need to inspect validation metadata, either:

- Make `ValidationDiagnostic` public with a public base class
- Or provide a public interface for accessing the metadata
- Or use extension methods that cast and extract

### Multi-roster Compilation

The current API assumes one roster per compilation. If multi-roster support is
needed, `GetValidationDiagnostics()` needs per-roster `forceCatalogues` mapping,
and return values should be keyed by roster.

---

## Appendix: Key File Locations

| File | Path |
|------|------|
| CompletionPart enum | `Concrete.Extensions/Utilities/CompletionPart.cs` |
| SymbolCompletionState | `Concrete.Extensions/Utilities/SymbolCompletionState.cs` |
| RosterSymbol (ForceComplete) | `Concrete.Extensions/Symbols/RosterSymbol.cs` |
| WhamCompilation | `Concrete.Extensions/WhamCompilation.cs` |
| ConstraintValidator | `Concrete.Extensions/Validation/ConstraintValidator.cs` |
| ModifierEvaluator | `Concrete.Extensions/Validation/ModifierEvaluator.cs` |
| ErrorCode enum | `Concrete.Extensions/Diagnostics/ErrorCode.cs` |
| ErrorFacts | `Concrete.Extensions/Diagnostics/ErrorFacts.cs` |
| WhamMessageProvider | `Concrete.Extensions/Diagnostics/WhamMessageProvider.cs` |
| WhamDiagnostic | `Concrete.Extensions/Diagnostics/WhamDiagnostic.cs` |
| ValidationDiagnostic | `Concrete.Extensions/Diagnostics/ValidationDiagnostic.cs` |
| DiagnosticMapper | `RosterEngine.Spec/DiagnosticMapper.cs` |
| SpecRosterEngineAdapter | `RosterEngine.Spec/SpecRosterEngineAdapter.cs` |

All paths relative to `src/WarHub.ArmouryModel.`.
