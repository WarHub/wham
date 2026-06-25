# ADR-0005: Git submodule for battlescribe-spec dependency

**Status**: Accepted

## Context

The conformance test suite depends on `BattleScribeSpec.TestKit`, which is
a project in the `battlescribe-spec` repository. The TestKit is not published
as a NuGet package (publishing is on the battlescribe-spec roadmap).

The roster engine project (`WarHub.ArmouryModel.RosterEngine`) needs a
`ProjectReference` to the TestKit. Initially, this pointed to a sibling
directory (`../../../battlescribe-spec/...`), requiring developers to
manually clone battlescribe-spec next to wham.

## Decision

Use a **git submodule** at `lib/battlescribe-spec`:

```
wham/
├── lib/
│   └── battlescribe-spec/    ← git submodule
│       └── src/
│           └── BattleScribeSpec.TestKit/
├── src/
│   └── WarHub.ArmouryModel.RosterEngine/
│       └── WarHub.ArmouryModel.RosterEngine.csproj
│           → ProjectReference: ../../lib/battlescribe-spec/src/BattleScribeSpec.TestKit/...
```

### CI Configuration

CI workflows use `submodules: true` in the checkout step. Because
battlescribe-spec is currently a **private** repository, CI requires a
`GH_PAT` secret with `repo` scope:

```yaml
- uses: actions/checkout@v6
  with:
    fetch-depth: 0
    token: ${{ secrets.GH_PAT || github.token }}
    submodules: true
```

### Developer Setup

```bash
git clone --recurse-submodules https://github.com/WarHub/wham.git
# or, if already cloned:
git submodule update --init
```

## Consequences

### Positive
- Self-contained repository — all dependencies are within the repo
- Reproducible builds — submodule pins a specific commit
- Standard git workflow — `git submodule update` is well-understood

### Negative
- Submodule update is an extra step for developers
- Private repo requires PAT for CI (resolved when battlescribe-spec goes public)
- Submodule can get out of sync if not updated regularly

### Migration Path
When `BattleScribeSpec.TestKit` is published as a NuGet package:
1. Replace `ProjectReference` with `PackageReference`
2. Remove the git submodule
3. Remove `GH_PAT` token from CI workflows
