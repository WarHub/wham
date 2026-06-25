# ADR-0001: Roslyn-inspired compilation and symbol model

**Status**: Accepted

## Context

The wham project needs to analyze BattleScribe datafiles and build semantic
models that resolve cross-references between entries, profiles, rules, and
constraints. This is structurally identical to compiler problems: a set of
source files define entities that reference each other by name/ID, and we need
to bind those references to their declarations and report errors.

This architecture was originally developed in the
[phalanx](https://github.com/WarHub/phalanx) project and ported to wham.

## Decision

Model after Roslyn's compiler architecture with a four-layer design:

### Layer 1: DTO (Data Transfer Objects)
`*Core` types from `WarHub.ArmouryModel.Source` provide direct XML
deserialization — plain C# objects mirroring the BattleScribe XML schema.

### Layer 2: SourceNode (Immutable Syntax Tree)
`SourceNode` wrappers add parent references, enabling upward navigation
(analogous to Roslyn's "red tree"). Each root is wrapped in a `SourceTree`
held by a `Compilation`.

### Layer 3: Symbol (Bound Semantic Model)
Internal `Symbol` classes in `WarHub.ArmouryModel.Concrete.Extensions` know
their declaration (SourceNode), containing symbol (parent), members (lazy),
and referenced symbols (lazy). Binding uses a `Binder` chain of
responsibility with namespace, catalogue, entry, and force scopes.

### Layer 4: ISymbol (Public Interface)
40+ `ISymbol` interfaces in `WarHub.ArmouryModel.Extensions` provide the
public API for consumers. The internal `Symbol` implementation can change
without breaking callers.

### Key Components

- **`Compilation`**: Top-level container holding source trees (immutable)
- **`Binder` chain**: Specialized binders resolving names in scope
- **`DiagnosticInfo`/`Diagnostic`/`DiagnosticBag`**: Error reporting

## Consequences

### Positive
- Proven architecture from Roslyn's battle-tested implementation
- Enables incremental analysis (replacing one source tree invalidates only
  affected symbols)
- Rich diagnostics with full context
- Clean separation of concerns across four layers

### Negative
- Complexity overhead — steep learning curve for contributors
- Large symbol surface (89 concrete symbol files)
- Lazy binding complexity (completion state machine designed for
  multi-threading, not strictly necessary for current use cases)

### Current State in wham
The ported code lives in:
- `src/WarHub.ArmouryModel.Extensions/` — ISymbol interfaces and infrastructure
- `src/WarHub.ArmouryModel.Concrete.Extensions/` — Symbol implementations
- `src/WarHub.ArmouryModel.EditorServices/` — Roster operations, formatting

These projects are marked `IsPackable=false` and `AnalysisMode=Default`
while they are being stabilized. They originate from phalanx and have not
yet been fully cleaned up for wham's strict analysis settings.
