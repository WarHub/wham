---
name: wham-spec-sync
description: >
  Sync wham's lib/battlescribe-spec submodule to latest main and make wham conform
  to new specs. Full lifecycle: evaluate changes, update submodule, fix API breaks,
  run conformance tests, triage and fix failures, handle spec contradictions (with
  upstream issues/PRs), create draft PR, iterate until CI green.
  Use when user says "update spec", "sync conformance", "new battlescribe-spec changes",
  "update the submodule", or "spec PR".
---

# wham-spec-sync

Sync `lib/battlescribe-spec` to latest `main` and drive wham to full conformance.
Covers the full lifecycle from evaluation to a CI-green PR ready to merge.

## When to Use

- "Update spec", "sync the submodule", "new battlescribe-spec changes"
- "Run conformance tests" after a spec repo update
- "Spec PR", "update wham to latest spec"
- Before or after a battlescribe-spec PR merges upstream

## Prerequisites

- `git submodule update --init` must have been run at least once
- `gh` CLI authenticated (`gh auth status`)
- Working `dotnet` SDK (build and test)
- Apply **gh-cli** skill guardrails for all PR/issue body operations

## Phase 0 — Evaluate Changes

Before touching anything, understand what changed:

```bash
# See what commits are incoming
git -C lib/battlescribe-spec fetch origin main
git -C lib/battlescribe-spec log HEAD..origin/main --oneline

# List changed spec YAML files
git -C lib/battlescribe-spec diff HEAD origin/main --name-only -- specs/
```

Read changed YAML files to classify changes:
- **New specs**: new feature or behavior being tested → likely need engine implementation
- **Modified specs**: adjusted expectations → may be API renames or behavior fixes
- **Removed specs**: deleted or folded into another spec → remove associated `[Fact(Skip=...)]` annotations
- **TestKit/API changes**: modified C# files in `src/BattleScribeSpec.TestKit/` → check for breaks

**Present a summary to the user** before proceeding. Flag anything that looks like a large
new feature (e.g. new spec category) that warrants discussion.

## Phase 1 — Update Submodule

```bash
git -C lib/battlescribe-spec checkout origin/main
# Verify the new HEAD
git -C lib/battlescribe-spec log -1 --oneline
```

Then create a branch off the current feature branch and commit the submodule bump:

```bash
git checkout -b spec-update/{short-description}
git add lib/battlescribe-spec
git commit -m "chore: update battlescribe-spec submodule to {new-sha-short}"
```

## Phase 2 — Fix API Breaks

Build first to surface compilation errors before running tests:

```bash
dotnet build
```

Common break patterns are documented in
[references/api-breaks.md](references/api-breaks.md). Fix all compilation errors
before proceeding — test output is misleading when the project doesn't compile.

## Phase 3 — Run Conformance Tests + Triage

```bash
dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/
```

Collect failing test IDs and group them by **spec category** (the directory under
`lib/battlescribe-spec/specs/`). Within each category, sort by likely cause:

| Cause | Signal |
|---|---|
| API rename / compile artifact | Failure message mentions missing member or cast |
| New spec (unimplemented feature) | Multiple failures in a new spec category |
| Regression from submodule bump | Spec previously passing now fails |
| Spec contradiction | Two specs conflict; wham can't satisfy both defaults |

Run a single spec to get a detailed error:
```bash
dotnet test tests/WarHub.ArmouryModel.RosterEngine.Tests/ --filter "DisplayName~{spec-id}"
```

## Phase 4 — Fix Implementation Issues

Work failures category by category, simplest first. Wham-specific patterns:

- **Selection ordering**: wham sorts by effective (post-modifier) name — see `WhamRosterEngine.cs`
- **Entry-id prefix**: link selections use `"linkId::targetId"` format; propagation via `JoinPrefix`
- **Collective semantics**: per-model multiply/divide; see `ConstraintEvaluator.cs` and `WhamRosterEngine.cs`
- **Scope traversal**: `ModifierEvaluator` scope methods mirror the `descendIntoSelections/Forces` flags
- **StateMapper**: reads only `ISymbol` public API — no `SourceNode` access; `Name` is non-nullable

Commit after each logical fix group. Run full suite frequently to catch regressions.

## Phase 5 — Handle Spec Contradictions

When two specs conflict such that wham cannot satisfy both defaults, see
[references/spec-contradictions.md](references/spec-contradictions.md) for the
decision flow: add a temporary wham engine override, file an upstream issue,
open a fix PR, then remove the override once the upstream PR merges and the
submodule is bumped again.

## Phase 6 — PR Lifecycle

Once all (or all addressable) conformance tests pass:

```bash
# Push the branch
git push -u origin HEAD

# Open draft PR targeting the current feature branch
gh pr create --draft \
  --base {current-feature-branch} \
  --title "chore: update battlescribe-spec to {date/version}" \
  --body "..."
```

PR body should include:
- New submodule SHA and a link to the diff in battlescribe-spec
- Summary of spec changes (new specs, modified specs, removed specs)
- List of implementation changes in wham
- Any outstanding wham engine overrides and their upstream tracking issues

Iterate: fix → test → commit → push until CI is green.
Mark ready when all checks pass:

```bash
gh pr ready {number}
```

**Do not merge** — use the **finish-pr** skill for the final merge step.

## Decision Quick-Reference

| Situation | Action |
|---|---|
| New spec category (e.g. `collective/`) | Implement the feature; it's unlikely to be a bug |
| Spec that was `KnownUndefinedBehavior` now promoted | Remove `Skip` annotation; fix if failing |
| Two specs conflict on same behavior | See spec-contradictions.md; add wham override + file upstream issue |
| Spec removed upstream | Delete associated `[Fact(Skip=...)]` in `ConformanceTests.cs` |
| TestKit API renamed | Fix call sites in `ConformanceTests.cs`, `SpecRosterEngineAdapter.cs`, `StateMapper.cs` |
| Conformance test passes but CI fails | Check non-conformance test projects: `dotnet test` |
| Upstream battlescribe-spec PR just merged | Re-run Phase 0 to evaluate, then Phase 1 to bump submodule |

## Reference Files

- [references/api-breaks.md](references/api-breaks.md) — Recurring API break patterns from past updates
- [references/spec-contradictions.md](references/spec-contradictions.md) — Detecting, overriding, and fixing spec self-contradictions
