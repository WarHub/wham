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

1. **Binding/declaration diagnostics**:
   - Produced during symbol completion (reference binding, member construction)
   - Stored in `DeclarationDiagnostics` bag on `WhamCompilation`
   - Triggered by `ForceComplete()` on all symbols
   - Examples: missing gamesystem, unresolvable references, unknown enum values

2. **Validation diagnostics**:
   - Produced by `ConstraintValidator` during the `CompletionPart.Validate`
     phase of `RosterSymbol.ForceComplete()`
   - Stored in the same `DeclarationDiagnostics` bag alongside binding diagnostics
   - Cached: runs once per compilation, not recomputed
   - Examples: min/max constraint violations, cost limits, force counts

**Both paths are unified in `GetDiagnostics()`**, which triggers `ForceComplete()`
and returns all diagnostics. `GetValidationDiagnostics()` is a convenience filter.

When called with explicit `forceCatalogues` (from `SpecRosterEngineAdapter`),
`GetValidationDiagnostics()` runs validation on-demand instead of using cached
results. See [The Process Hang Investigation](#the-process-hang-investigation).

### ConstraintValidator Lifecycle

When called via `ForceComplete()`, the `ConstraintValidator` is created fresh
for each `RosterSymbol`'s `Validate` phase. The `NotePartComplete` CAS ensures
only one thread enters the `Validate` phase per `RosterSymbol`, so the
validator instance is never shared between threads. Its lifecycle:

1. **Construction**: Receives `RosterNode`, `WhamCompilation`, `forceCatalogues`
2. **BuildIndex()**: Indexes all catalogues' entries, categories, force entries,
   and shared entry link mappings into dictionaries
3. **Run()**: Iterates forces → root entries → child entries → constraints
4. **Disposal**: Instance is garbage collected after `Run()` returns

The dictionaries (`_entryIndex`, `_categoryIndex`, `_sharedEntryLinkIds`) are
never shared between threads. The `_sharedEntryLinkIds` check-then-act pattern
is safe because the CAS on `CompletionPart.Validate` guarantees single-threaded
access to the validator instance.

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

**Re-integrated validation into `ForceComplete()`** using the pre-completion
strategy (option 3 from the original investigation — break the re-entrant
dependency). The `CompletionPart.Validate` phase now:

1. Pre-completes all catalogue symbols to ensure lazy symbol properties accessed
   during validation are already resolved
2. Runs `ConstraintValidator.Validate()` with `CancellationToken` support
3. Adds validation diagnostics to the compilation's `DeclarationDiagnostics` bag

```csharp
case CompletionPart.Validate:
    {
        // Pre-complete catalogues to prevent re-entrant ForceComplete via
        // GetBoundField → BindReferences → SpinWaitComplete.
        var compilation = (WhamCompilation)DeclaringCompilation;
        foreach (var catalogue in compilation.SourceGlobalNamespace.Catalogues)
        {
            cancellationToken.ThrowIfCancellationRequested();
            catalogue.ForceComplete(cancellationToken);
        }
        ConstraintValidator.Validate(
            Declaration, compilation, compilation.DeclarationDiagnostics,
            forceCatalogues: null, cancellationToken);
        state.NotePartComplete(CompletionPart.Validate);
        break;
    }
```

**Why pre-complete catalogues, not the global namespace?** The global namespace's
`MembersCompleted` phase calls `ForceComplete()` on all members including this
`RosterSymbol`. Calling `SourceGlobalNamespace.ForceComplete()` from within
the `Validate` phase would create infinite recursion (stack overflow). Instead,
we complete only catalogue symbols — these are the cross-graph references that
validation accesses through the Binder. The roster's own member tree is already
completed by the `MembersCompleted` phase.

This approach:

- **Eliminates the hang**: All lazy symbol bindings are pre-resolved; no
  `SpinWait` contention during validation
- **Caches validation results**: Diagnostics are stored in the compilation's
  `DeclarationDiagnostics` bag during `ForceComplete()`, not recomputed
- **Supports cancellation**: `CancellationToken` is threaded through validation,
  allowing clean test host shutdown
- **Maintains thread safety**: `NotePartComplete` CAS ensures only one thread
  enters the `Validate` phase; the `ConstraintValidator` instance is never shared

### Impact on API

`WhamCompilation.GetDiagnostics()` now returns **all** diagnostics: binding,
declaration, and validation. This matches Roslyn's convention where
`GetDiagnostics()` is the single entry point for all diagnostic information.

`GetValidationDiagnostics()` is retained as a convenience method:
- When called with `forceCatalogues` (non-null), it runs validation on-demand
  with the specified catalogues (used by `SpecRosterEngineAdapter`)
- When called without `forceCatalogues`, it filters `GetDiagnostics()` for
  `ValidationDiagnostic` instances

### Previous Workaround (Removed)

The original workaround moved validation out of `ForceComplete()` entirely,
splitting the API into `GetDiagnostics()` (binding only) and
`GetValidationDiagnostics()` (validation only). This was flagged by all three
code reviewers as an API break. The current implementation resolves this.

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

### 1. GetDiagnostics() API Break (all three reviewers) — RESOLVED

**Finding**: `GetDiagnostics()` no longer returns validation diagnostics.

**Resolution**: Validation has been re-integrated into `ForceComplete()`.
`GetDiagnostics()` now returns all diagnostics (binding + validation).

### 2. Thread-unsafe `_sharedEntryLinkIds` (Opus 4.6) — DOCUMENTED

**Finding**: Race condition in `BuildIndex()` — check-then-act pattern on
`_sharedEntryLinkIds` dictionary.

**Resolution**: Now that validation runs inside `ForceComplete()`, single-threaded
access is guaranteed by the `NotePartComplete` CAS on `CompletionPart.Validate`.
Only one thread enters the `Validate` phase; others SpinWait until it completes.
This is documented in the `ConstraintValidator` class doc comment.

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

### ~~Lazy Validation Caching~~ — RESOLVED

Validation now runs inside `ForceComplete()` and results are cached in the
compilation's `DeclarationDiagnostics` bag. Since compilations are immutable
(new instance per change), validation runs once per compilation instance.

### ~~Unifying GetDiagnostics~~ — RESOLVED

`GetDiagnostics()` now returns all diagnostics (binding + validation).
`GetValidationDiagnostics()` is retained as a convenience filter, and also
supports the on-demand path with explicit `forceCatalogues` for the spec adapter.

### ~~Force Catalogue Association~~ — RESOLVED

Fixed in Phase 2: `NodeFactory.Force()` now accepts a `catalogueOverride`
parameter. `WhamRosterEngine.AddForce()` passes the actual selection-catalogue,
so `ForceNode.CatalogueId` is correct and `ForceCatalogueReferenceSymbol`
resolves via the Binder without any external mapping.

### ~~Location Information~~ — RESOLVED

`ValidationDiagnostic` objects now carry `SourceNode` locations instead of
`Location.None`. Each validation call site passes the relevant roster node:
- Selection constraint violations → `ForceNode` location
- Child constraint violations → parent `SelectionNode` location
- Force count violations → `RosterNode` location
- Cost limit violations → `RosterNode` location
- Category count violations → `ForceNode` location

The existing `SourceNodeExtensions.GetLocation()` creates a `SourceLocation`
with `SourceTree` and `TextSpan`, enabling IDE-style error highlighting.

### ~~ValidationDiagnostic Visibility~~ — RESOLVED

Resolved in Phase 2: `IValidationDiagnostic` public interface in the Extensions
layer exposes validation metadata (OwnerType, OwnerId, OwnerEntryId, EntryId,
ConstraintId, RosterId). Consumers check `diag is IValidationDiagnostic vd`.
`DiagnosticMapper` uses the interface instead of casting to internal types.

### ~~Multi-roster Compilation~~ — RESOLVED

Each `RosterSymbol.ForceComplete()` runs validation independently. Each
`ValidationDiagnostic` now carries a `RosterId` property (from the roster node),
enabling per-roster filtering. The new
`WhamCompilation.GetValidationDiagnostics(IRosterSymbol)` overload filters
diagnostics by roster ID.

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
| IValidationDiagnostic | `Extensions/Diagnostics/IValidationDiagnostic.cs` |
| DiagnosticMapper | `RosterEngine.Spec/DiagnosticMapper.cs` |
| SpecRosterEngineAdapter | `RosterEngine.Spec/SpecRosterEngineAdapter.cs` |

All paths relative to `src/WarHub.ArmouryModel.`.
