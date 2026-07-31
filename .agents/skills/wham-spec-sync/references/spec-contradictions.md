# Spec Contradictions

How to detect, work around, and resolve self-contradictions in `battlescribe-spec`.

## Detecting a Contradiction

A spec contradiction is when two specs have conflicting **default** expectations
for the same engine behavior, making it impossible for a new engine to satisfy both.

Signs:
- One spec fails with `expected X but got Y`; another spec expects `Y` for the same behavior
- Existing wham overrides flip exactly those two specs against each other
- BattleScribe and NewRecruit have engine-specific overrides going in opposite directions

Investigation steps:
1. Identify the failing spec and its expected value
2. Search other specs in the same category for the same behavior:
   ```bash
   grep -r "the-behavior-keyword" lib/battlescribe-spec/specs/ --include="*.yaml" -l
   ```
3. Check if the default expectations are internally inconsistent across specs

## Wham Engine Override Format

Add a `engines.wham:` section to the spec YAML with the wham-specific expectation.
Only override the fields that differ; everything else inherits from the spec default.

```yaml
# In lib/battlescribe-spec/specs/{category}/{id}.yaml
engines:
  wham:
    steps:
      - step: 3
        force:
          - selection:
              - name: Active Watcher  # wham effective-name sort order
              - name: Target Unit
```

The `errors:` and `errorsContain:` keys can also be overridden:
```yaml
engines:
  wham:
    errors:
      - on: 'ForceType forceId'
        from: 'entryId/constraintId'
```

Error format for wham: `on='ownerType ownerEntryId', from='entryId/constraintId'`
(`errors:` = exact set match; `errorsContain:` = subset match)

**Never add wham overrides to the battlescribe-spec repo** — they must be in a fork/PR
or the spec repo itself if the fix belongs there.

## When to Override vs When to Fix Upstream

| Situation | Action |
|---|---|
| Wham has a genuine implementation bug | Fix wham, do not add override |
| Spec default matches BattleScribe quirk (not "correct" behavior) | Add wham override that reflects correct behavior; file upstream issue |
| Two spec defaults conflict (wham can't satisfy both) | Add temporary wham override; file upstream issue + PR to fix the contradiction |
| Spec documents explicitly undefined behavior | Check if `KnownUndefinedBehavior` is still appropriate or was resolved |

## Filing an Upstream Issue in battlescribe-spec

Repository: `WarHub/battlescribe-spec`

Issue should include:
- The two (or more) conflicting spec IDs
- The exact conflicting default values
- Why a new engine cannot satisfy both
- What behavior wham implements and why it's correct

```bash
gh issue create --repo WarHub/battlescribe-spec \
  --title "Spec contradiction: {spec-a} vs {spec-b} on {behavior}" \
  --body-file /tmp/issue-body.md
```

## Opening a Fix PR in battlescribe-spec

1. Fork (or push to a branch if you have write access)
2. Edit the spec YAML — move one spec's default to match the intended behavior,
   add engine-specific overrides for BattleScribe/NewRecruit where they differ
3. Open a PR referencing the tracking issue

```bash
gh pr create --repo WarHub/battlescribe-spec \
  --title "Fix: align {spec-id} default to effective-name sort order" \
  --body "Fixes #{issue-number}. ..."
```

## After Upstream Merge: Removing Wham Overrides

Once the fix PR merges into battlescribe-spec `main`:

1. Bump the submodule (Phase 1 of the skill)
2. Delete the wham-specific override blocks from the affected spec YAMLs
   (they now live in `lib/battlescribe-spec/` which is read-only — if overrides
   were in a fork branch, that branch is no longer needed)
3. Run conformance tests to confirm no regressions
4. Update the PR body to note the upstream fix

## Known Historical Overrides

| Spec ID | Behavior | Resolution |
|---|---|---|
| `scope-child-id-filter` | Selection sort order (effective vs original name) | Fixed upstream in battlescribe-spec PR #220; wham override removed |
| `scope-primary-catalogue` | Selection sort order | Consistent with effective-name sort; no override needed |
