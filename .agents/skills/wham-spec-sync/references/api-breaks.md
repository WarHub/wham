# API Break Patterns

Recurring break patterns when bumping `lib/battlescribe-spec`. Check these first
after a submodule update causes compilation errors.

## Namespace Renames

| Old | New | Files affected |
|---|---|---|
| `BattleScribeSpec` | `BattleScribeSpec.Roster` | `ConformanceTests.cs`, `SpecRosterEngineAdapter.cs`, `StateMapper.cs` |

## Class / Type Renames

| Old | New | Files affected |
|---|---|---|
| `SpecRunner` | `RosterRunner` | `ConformanceTests.cs` |

Search for the old name with grep; there should be no survivors after the fix.

## Removed `KnownUndefinedBehavior` Entries

Specs that were previously annotated as undefined behavior get promoted to
regular specs when the spec repo adds a clear expectation. When this happens:

1. The test that had `[Fact(Skip = "KnownUndefinedBehavior: ...")]` will now
   **compile but the Skip is gone** — or the spec file itself is deleted/merged.
2. Remove the `Skip` annotation (or the entire `[Fact]` override if the spec was deleted).
3. Run the test — it likely passes once the `Skip` is removed; if not, there is
   a real implementation gap.

## New Protocol Field Types

When the spec introduces a new field or changes a field type on a `Protocol*`
record (e.g. `double` → `decimal` for cost/limit values), `StateMapper.cs` will
fail to compile. Update the relevant mapping expression to match the new type.

Pattern to check in `StateMapper.cs`:
```csharp
// Before (double)
Value = cost.Value,  // double
// After (decimal)
Value = (double)cost.Value,  // if Protocol type kept double
// OR
Value = cost.Value,  // if Protocol type changed to decimal too
```

Verify by checking the Protocol record definition in
`lib/battlescribe-spec/src/BattleScribeSpec.TestKit/Protocol/`.

## New Protocol Types (New Features)

When a new spec category appears (e.g. `specs/collective/`) and the TestKit adds
new `Protocol*` types, `StateMapper` must be extended to populate them.
`SpecRosterEngineAdapter` may also need changes to wire the new feature into
`IRosterEngine` calls.

Steps:
1. Check which new `Protocol*` types were added in `BattleScribeSpec.TestKit/Protocol/`
2. Find where the spec expects them to appear in `IRosterState`
3. Add population logic in `StateMapper.cs`

## ConformanceTests.cs Registration

Engine name must remain `"wham"` — changing it invalidates all `engines.wham:` overrides:

```csharp
// ConformanceTests.cs
runner.RegisterEngine("wham", new WhamRosterEngineAdapter(...));
```

Do not rename or add aliases.
