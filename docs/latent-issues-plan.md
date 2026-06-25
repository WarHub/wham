# Plan: Addressing Latent Engine Issues

This document outlines a plan for addressing latent issues identified during
code review. These are NOT triggered by the current 303 conformance specs
but represent correctness gaps for edge cases.

**Status**: Plan only — no implementation yet.

## Issue 1: Ancestor Scope Only Checks Immediate Parent

### Problem
`scope="ancestor"` conditions and `instanceOf` checks only examine
`ParentSelection`, not the full ancestor chain. Deeply nested selections
won't match ancestor-based constraints.

### wham Changes
- Add a `Parent` reference to `RosterSelection` (or pass ancestor chain
  through `EvalContext`)
- Modify `GetAncestorScope()` to walk the full parent chain
- Modify `CheckAncestorInstanceOf()` to check all ancestors

### Spec Changes (battlescribe-spec)
- Create `specs/scope/scope-ancestor-deep.yaml`: Test with 3+ levels of
  nesting where a condition references a grandparent entry
- Create `specs/condition/condition-instance-of-ancestor-deep.yaml`: Test
  instanceOf with grandparent matching

### Engine Mode Consideration
- **newrecruit**: Likely handles full ancestor chain correctly
- **battlescribe**: Behavior unknown — needs testing with BS to determine
  if BS itself handles deep ancestors
- **wham**: Should match newrecruit behavior

---

## Issue 2: Child Constraint Modifiers Lack Parent Context

### Problem
`ValidateChildConstraints` creates an `EvalContext` with
`ParentSelection = null`. Any modifier/condition scoped to `parent` or
`ancestor` resolves against force-level selections instead of the actual
parent.

### wham Changes
- Pass the parent selection when evaluating child constraint modifiers:
  ```csharp
  var ctx = new EvalContext { ..., ParentSelection = parentSel };
  ```
- This is closely related to Issue 1 (ancestor chain)

### Spec Changes
- Create `specs/constraint/constraint-child-with-parent-modifier.yaml`:
  Constraint on a child entry whose value is modified by a condition that
  checks the parent entry's properties
- Add wham-specific expectations if behavior differs from BS default

---

## Issue 3: Cost Conditions Use Raw Costs

### Problem
Conditions and constraints that count cost fields read `sel.Entry.Costs`
directly, not the effective (modified) costs after modifiers are applied.

### wham Changes
- Route cost counting in conditions/constraints through
  `evaluator.GetEffectiveCosts()` — the same path used for roster totals
- Requires passing the evaluator context into `GetSelectionCostValue()`

### Spec Changes
- Create `specs/cost/cost-condition-with-modifier.yaml`: A selection has a
  cost modifier (e.g., "set pts to 0") and a condition that checks the
  cost value — the condition should see the modified cost
- Create `specs/constraint/constraint-cost-field-modified.yaml`: Similar
  setup but for constraint validation

### Engine Mode Consideration
- **battlescribe**: Likely uses modified costs in conditions (needs testing)
- **newrecruit**: Should use modified costs
- **wham**: Align with newrecruit; add wham override if BS behavior differs

---

## Issue 4: Linked Child Selection Identity

### Problem
`GetEffectiveId()` returns `sel.Entry.Id`, ignoring `SourceLink`. Child
selections from entry links may be counted under the target entry ID
instead of the link ID, causing incorrect constraint validation.

### wham Changes
- Update `GetEffectiveId()` to return `sel.SourceLink?.Id ?? sel.Entry.Id`
- Audit all callers to ensure consistent ID usage
- Update `ValidateChildConstraints` count aggregation

### Spec Changes
- Create `specs/constraint/constraint-linked-child-count.yaml`: Two
  different entry links point to the same shared entry. A parent-scoped
  constraint should count each link's selections independently.

---

## Implementation Order

Recommended order (by impact and complexity):

1. **Issue 2** (child constraint parent context) — smallest change, enables
   Issue 1
2. **Issue 1** (ancestor scope) — builds on Issue 2's parent tracking
3. **Issue 4** (linked child identity) — isolated change to ID resolution
4. **Issue 3** (cost conditions) — requires threading evaluator through
   counting paths

## Engine Mode Strategy

For all changes, the approach should be:

1. **Default behavior** should match **newrecruit** — the "correct"
   reference implementation
2. If **BattleScribe** behavior differs, document the difference in spec
   tags (`design-difference` or `battlescribe-bug`)
3. Add `engines.wham` overrides in specs where wham intentionally diverges
4. Consider a future "battlescribe-compat" mode if strict BS compatibility
   is needed (e.g., for data migration scenarios)

### Current `battlescribe` vs `newrecruit` Alignment

| Area | wham aligns with | Notes |
|------|-----------------|-------|
| scope=parent on uncategorised | newrecruit | Generates error (BS doesn't) |
| Constraint error format | battlescribe | Uses BS error format conventions |
| EntryLink merge behavior | battlescribe | `merged.Id = target.Id` |
| Auto-select | both | Same behavior |
| Shared constraint merging | battlescribe | Absorbs most restrictive value |

## Spec Organization

New specs should follow the existing category structure:
- `specs/scope/` — for scope-related tests
- `specs/condition/` — for condition evaluation
- `specs/constraint/` — for validation constraints
- `specs/cost/` — for cost-related tests

Each spec should include:
- `tags: [design-difference]` if behavior varies between engines
- `engines.wham:` override if wham's behavior differs from the default
- `engines.newrecruit:` override if NR's behavior differs
- Clear `description` explaining what's being tested
