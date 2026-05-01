# ADR-0004: BattleScribe-spec conformance testing

**Status**: Accepted

## Context

BattleScribe's roster editing behavior is complex and largely undocumented.
Multiple engines (BattleScribe, New Recruit, phalanx/wham) need to agree on
how modifiers, conditions, constraints, scopes, costs, and entry links work.

The [battlescribe-spec](https://github.com/WarHub/battlescribe-spec) project
provides 303 declarative YAML conformance specs covering:
- Selections (81+), Modifiers (54+), Constraints (40+), Conditions (34+)
- Costs (23+), Scopes (14+), Forces (13+), Roster operations (10+)
- Auto-select (5), Catalogues (5), Entry groups (4), Entry links (3)

Each spec defines a game system setup, a sequence of actions, and expected
roster state at each step.

## Decision

Use the BattleScribe-spec TestKit as the primary correctness oracle:

1. **Reference as project** — `BattleScribeSpec.TestKit` is referenced via
   `ProjectReference` from a git submodule at `lib/battlescribe-spec`

2. **Engine name "wham"** — registered in `ConformanceTests.cs` via
   `new SpecRunner(engine, engineName: "wham")`

3. **Per-engine expectations** — Specs can have engine-specific overrides
   in the `engines:` section. When no `wham` override exists, the default
   expectation is used. This allows tracking known behavioral differences.

4. **Error matching** — The `errors:` field requires an EXACT set match
   (count + content). The `errorsContain:` field allows subset matching.
   Error format: `on='ownerType ownerEntryId', from='entryId/constraintId'`

### Test Infrastructure

```csharp
// ConformanceTests.cs
[Theory]
[MemberData(nameof(AllSpecs))]
public void Spec(string id, string resourceName)
{
    var engine = new WhamRosterEngine();
    var runner = new SpecRunner(engine, engineName: "wham");
    runner.Run(spec);
}
```

## Consequences

### Positive
- **303 specs** provide comprehensive behavioral coverage
- **Declarative YAML** — specs are readable and maintainable
- **Cross-engine** — same specs validate BattleScribe, New Recruit, and wham
- **Regression detection** — any engine change is immediately validated

### Negative
- **Private dependency** — battlescribe-spec is currently private, requiring
  a `GH_PAT` secret for CI (see ADR-0005)
- **BattleScribe quirks** — some default expectations match BattleScribe bugs
  rather than "correct" behavior; wham uses engine-specific overrides for these
- **No unit tests for internals** — conformance tests are integration tests;
  individual components (EntryResolver, ModifierEvaluator, etc.) lack focused
  unit tests

### BattleScribe Behavioral Quirks Documented

1. **scope=parent on uncategorised entries**: BattleScribe doesn't validate
   scope=parent constraints on entries without categoryLinks. Tagged as
   `design-difference` in specs. wham generates the error (correct behavior)
   and has a wham-specific override in `scope-parent` spec.

2. **Null childId conditions**: When a condition has an empty childId and
   scope != "self", BattleScribe returns NaN, causing the condition to
   evaluate to false.

3. **Shared constraints with merged links**: BattleScribe attributes errors
   to the shared constraint ID but uses the most restrictive constraint value
   across all constraints of the same field+type.
