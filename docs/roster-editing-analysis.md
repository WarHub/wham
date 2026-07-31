# Roster Editing Architecture Analysis & Refactoring Plan

> **Audience**: Project author + AI agents. Optimized for precision and traceability.
> **Scope**: Roster change operations across WhamRosterEngine, EditorServices, Effective
> Symbols, and the Compilation pipeline. Excludes XML serialization, source generators,
> CLI tool, and benchmarks.

---

## Part 1: Architecture Analysis

### 1. Layer Inventory

The roster editing architecture spans seven layers. Dependencies flow downward;
no layer references a layer above it.

| Layer | Project | Key Files | Role |
|-------|---------|-----------|------|
| **7. Workspace** | `EditorServices` | `WhamWorkspace.cs`, `CompilationTracker.cs`, `DocumentId.cs` | Multi-roster orchestrator, owns all mutations, change events |
| **6. Editor** | `EditorServices` | `RosterEditor.cs`, `IRosterOperation.cs`, `RosterOperations.cs` | Undo/redo stack, SourceNode-based operation commands |
| **5. State** | `EditorServices` | `RosterState.cs`, `SourceNodeExtensions.cs` | Immutable (Compilation) record, node transforms |
| **4. Engine** | `RosterEngine` | `WhamRosterEngine.cs`, `EntryResolver.cs` | ISymbol-based functional roster API (conformance-tested) |
| **3. Spec Adapter** | `RosterEngine.Spec` | `SpecRosterEngineAdapter.cs`, `StateMapper.cs`, `ProtocolConverter.cs` | BattleScribe TestKit ↔ Engine bridge |
| **2. Effective Symbols** | `Concrete.Extensions` | `Effective/` (13 files), `ConstraintEvaluator.cs` | Modifier-applied wrappers, constraint validation |
| **1. Compilation** | `Extensions` + `Concrete.Extensions` | `Compilation.cs`, `WhamCompilation.cs`, `Binder.cs`, 89 symbol files | 4-layer Roslyn model: DTO → SourceNode → Symbol → ISymbol |

### 2. Mutation Flow Map

Two independent mutation paths exist. They share `RosterState` and `WhamCompilation`
but produce roster nodes through different mechanisms.

#### 2a. EditorServices Path (SourceNode-based)

```
UI / Consumer
  │
  ▼
WhamWorkspace.ApplyOperation(rosterId, operation)        ← workspace.cs:257
  │  lock(syncRoot)
  ▼
RosterEditor.ApplyOperation(operation)                   ← RosterEditor.cs:28
  │  push (newState, operation) onto undo stack
  ▼
IRosterOperation.Apply(baseState)                        ← each concrete op
  │
  ├─ RosterOperationBase.TransformRoster(state)          ← SourceNode transforms
  │    returns transformedRoster.WithUpdatedCostTotals() ← cost walk on SourceNode tree
  │
  ▼
RosterState.ReplaceRoster(newRosterNode)                 ← RosterState.cs:40
  │  oldTree → newTree → Compilation.ReplaceSourceTree()
  ▼
new RosterState(newCompilation)
  │
  ▼
Workspace updates CompilationTracker                     ← workspace.cs:445
  fires WorkspaceChanged event
```

**Operations available** (`RosterOperations.cs`):
- `CreateRosterOperation` — creates roster from gamesystem cost types
- `AddForceOperation` — adds force from `ForceEntryNode` (no categories, TODO)
- `AddSelectionOperation` — adds selection from `SelectionEntryNode` + `ForceNode` (no sub-selections, TODO)
- `AddSelectionFromLinkOp` — adds selection from link (no categories, TODO)
- `AddRootEntryFromSymbol` — symbol-based add (resolves via `SymbolKey`, builds costs/categories)
- `RemoveForceOperation`, `RemoveSelectionOperation`, `ChangeSelectionCountOperation`
- `ChangeCostLimitOperation`, `ChangeRosterNameOperation`
- `LambdaOperation` — ad-hoc transform wrapper
- `ChainedRosterOperation` — composes operations sequentially

#### 2b. RosterEngine Path (ISymbol-based)

```
SpecRosterEngineAdapter (IRosterEngine)                  ← conformance test entry
  │  index → ISymbol mapping
  ▼
WhamRosterEngine.SelectEntry(state, forceIndex, entry)   ← WhamRosterEngine.cs:167
  │  resolves ISymbol → SelectionEntryNode declaration
  │  builds entryId path (linkId::targetId format)
  │  builds costs, categories from ISymbol API
  │  auto-selects children with min ≥ 1 constraints
  ▼
state.ReplaceRoster(newRoster.WithUpdatedCostTotals())
  ▼
new RosterState ← returned functionally (no workspace, no undo stack)
```

**Operations available** (`WhamRosterEngine`):
- `CreateRoster(catalogCompilation)` — creates roster with costs/limits from gamesystem
- `AddForce(state, forceEntry, catalogue)` — adds force with resolved categories
- `RemoveForce(state, forceIndex)`
- `SelectEntry(state, forceIndex, entry, sourceGroup?)` — full selection with auto-children
- `SelectChildEntry(state, forceIndex, selectionIndex, childEntry, sourceGroup?)`
- `DeselectSelection(state, forceIndex, selectionIndex)`
- `DuplicateSelection(state, forceIndex, selectionIndex)`
- `SetCostLimit(state, costTypeId, value)`
- `GetAvailableEntries(state, forceIndex)` — via `EntryResolver`
- `GetChildEntries(entry)` — via `EntryResolver`

#### 2c. Where the Paths Diverge

| Concern | EditorServices Path | RosterEngine Path |
|---------|--------------------|--------------------|
| **Addressing** | Raw SourceNode references (stale after mutation) | Index-based + ISymbol resolution |
| **EntryId format** | Plain `SelectionEntry.Id` | BattleScribe `linkId::targetId` format |
| **Categories** | TODOs, incomplete | Full resolution via `ICategoryEntrySymbol.ReferencedEntry` |
| **Sub-selections** | TODOs, not implemented | Recursive auto-children with min constraint check |
| **Profiles/Rules** | Not added to selections | Not added to selections (by design — effective symbols provide these) |
| **Cost calculation** | `WithUpdatedCostTotals()` walks SourceNode tree | Same `WithUpdatedCostTotals()` call |
| **Integration** | Workspace + undo/redo | Standalone functional, no undo |
| **Conformance** | Not tested | 304/304 specs |

**Key observation**: `AddRootEntryFromSymbol` in EditorServices is the only operation that
bridges toward the Engine path (uses `SymbolKey` resolution, builds proper costs/categories).
The older `AddSelectionOperation` and `AddSelectionFromLinkOp` are effectively dead code for
correct roster building.

### 3. Effective Symbols

#### 3a. What They Provide

Effective symbols are Roslyn-style wrapper symbols (analogous to `SubstitutedSymbol`) that
present modifier-applied values while delegating unchanged properties to the original symbol.

| Effective Type | Wraps | Computed Properties |
|----------------|-------|---------------------|
| `EffectiveContainerEntrySymbol` (abstract base) | `IContainerEntrySymbol` | Name, IsHidden, Costs, Constraints, Resources, Page, PublicationReference |
| `EffectiveEntrySymbol` | `ISelectionEntryContainerSymbol` | Above + Categories, PrimaryCategory |
| `EffectiveForceEntrySymbol` | `IForceEntrySymbol` | Above (container subset) + Resources |
| `EffectiveProfileSymbol` | `IProfileSymbol` | Name, IsHidden, Characteristics, Page, PublicationId |
| `EffectiveRuleSymbol` | `IRuleSymbol` | Name, Description, IsHidden, Page, PublicationId |
| `EffectiveCostSymbol` | `ICostSymbol` | Value |
| `EffectiveConstraintSymbol` | `IConstraintSymbol` | Query.ReferenceValue |
| `EffectiveQuerySymbol` | `IQuerySymbol` | ReferenceValue |
| `EffectiveCharacteristicSymbol` | `ICharacteristicSymbol` | Value |
| `EffectivePublicationReferenceSymbol` | `IPublicationReferenceSymbol` | Page |

These are **context-sensitive**: the same catalogue entry can produce different effective
values depending on which selection/force context it appears in (because modifiers can be
conditioned on roster state).

#### 3b. Computation Lifecycle (CompletionPart Pipeline)

Symbol completion follows a 4-phase pipeline managed by `SymbolCompletionState`:

```
Phase 1: Members                 ← collect/initialize child symbols
Phase 2: EffectiveEntries        ← compute modifier-applied values (roster-only)
Phase 3: CheckReferences         ← force-access bound fields, report diagnostics
Phase 4: CheckConstraints        ← evaluate constraint diagnostics (roster-only)
```

Bound reference fields (e.g. `Type`, `Publication`, `SourceEntry`) are
**self-completing** — each property getter lazily binds on first access using
`Interlocked.CompareExchange`. There is no separate `BindReferences` phase.
`CheckReferences` force-accesses all bound fields to ensure diagnostics are reported.

Phases 2 and 4 are **roster-only** — catalogue symbols auto-complete these phases
immediately since effective entries require roster context for condition evaluation.

**Trigger**: `ForceComplete()` on any symbol cascades through all phases. Accessing
`ISelectionSymbol.EffectiveSourceEntry` triggers completion up through Phase 2.
`GetDiagnostics()` triggers full completion including Phase 4.

#### 3c. EffectiveEntryCache Architecture

```
RosterSymbol                                ← owns cache
  │
  ├── GetOrCreateEffectiveEntryCache()      ← CAS-protected singleton
  │
  ▼
EffectiveEntryCache                         ← per-roster, thread-safe
  │
  ├── ConcurrentDictionary<EffectiveEntryKey, EffectiveEntrySymbol>
  │     key = (entry, selection?, force?)   ← context-sensitivity
  │
  ├── ModifierEvaluator                     ← internal, applies modifiers
  │     evaluates IEffectSymbol conditions
  │     resolves scopes (parent, ancestor, force, roster)
  │     applies operations (set, increment, decrement, append)
  │     ⚠ casts to concrete SelectionSymbol in several places
  │       (ModifierEvaluator.cs:619-631, 790-793, 944-949, 1088-1090)
  │       — effective evaluation is NOT truly abstract over ISelectionSymbol
  │
  └── CollectEffectiveResources()           ← 4-pass resource traversal
        (1) direct profiles
        (2) direct rules
        (3) InfoLinks
        (4) inline InfoGroups
        — matches BattleScribe output ordering
```

**Access patterns**:
- `ISelectionSymbol.EffectiveSourceEntry` → `RosterSymbol.GetEffectiveEntry(declaredEntry, selection, force)`
  → cache lookup/create
- `IForceSymbol.EffectiveSourceEntry` → similar, with force context
- `StateMapper` reads `sel.EffectiveSourceEntry` to build Protocol output

#### 3d. Relationship to Editor Views

Currently, effective symbols are the **only** mechanism for presenting modifier-applied state.
However, they are consumed only by:
1. `StateMapper` (conformance output) — maps to BattleScribe Protocol types
2. `ConstraintEvaluator` (validation) — reads effective constraints/costs

**No editor view layer exists**. A UI consumer wanting to display a selection's effective
name, costs, profiles, and rules must navigate the ISymbol API directly:
```csharp
var sel = rosterSymbol.Forces[0].Selections[0];
var eff = sel.EffectiveSourceEntry;
// eff.Name, eff.IsHidden, eff.Costs, eff.Resources, eff.Categories...
```

There is no "EntryOptionView" that combines an available entry with its constraint status
(e.g., "you must select at least 1 of this") or "SelectionDetailView" that bundles
a selection's effective state with its children, profiles, rules, and diagnostics.

### 4. Pain Points

#### 4a. Inconsistent Selection Identity Usage

Selections **do** have generated node IDs: `NodeFactory.Selection` (`NodeFactory.cs:485-502`)
assigns a GUID, and `SourceDeclaredSymbol` inherits it (`SourceDeclaredSymbol.cs:19-35`).
Most edits preserve node identity because replacements operate on existing nodes
(`SourceNodeExtensions.cs:8-35, 55-65`). The problem is **inconsistent use** of that
identity across the two mutation paths, plus `DuplicateSelection()` reusing IDs:

| Path | Addressing | Stability |
|------|-----------|-----------|
| Old EditorServices ops | Raw `SourceNode` object references | **Stale after any mutation** — immutable tree replacement invalidates prior node references (see `AddSelectionFromLinkOp` line 177: looks up force by ID each iteration as workaround) |
| WhamRosterEngine | Positional indices (`forceIndex`, `selectionIndex`) | **Fragile across insertions/removals** — indices shift when siblings are added/removed |
| `AddRootEntryFromSymbol` | `SymbolKey` (kind + ID + containingModule + containingEntry) | **Most stable** — survives tree mutations, resolves via compilation |

`DuplicateSelection` (`WhamRosterEngine.cs:247-250`) explicitly re-adds the original node
unchanged — duplicate selections carry the same IDs, which breaks ID-based addressing for
siblings and their descendants.

Current `SymbolKey` uses `(kind, module, id, containingEntryId)` via `SymbolIndex`
(`SymbolIndex.cs:46-83`). This encodes **catalogue-symbol identity**, not
**roster-instance identity**: when subtrees are duplicated, both the selection ID and
containing-selection ID are reused, making descendants ambiguous.

**Impact**: Views, commands, diff/sync, and undo targeting will remain fragile until
identity usage is unified. The first step should be surfacing existing selection node IDs
through APIs/views consistently. Only invent a new GUID/path model if current node IDs
prove insufficient after `DuplicateSelection` is fixed to mint fresh IDs. Options:
- Fix `DuplicateSelection` to assign fresh node IDs, then use existing node IDs consistently
- Path-based (e.g., `force[0]/selection[2]/selection[0]` — stable within a single tree version)
- Dedicated selection-instance GUID (assigned at creation, preserved across edits)

#### 4b. Duplicate Mutation Logic

**Two implementations of roster mutation** exist in parallel:

| Operation | EditorServices (`RosterOperations.cs`) | RosterEngine (`WhamRosterEngine.cs`) |
|-----------|---------------------------------------|--------------------------------------|
| Create roster | `CreateRosterOperation` (line 63) | `CreateRoster()` (line 33) |
| Add force | `AddForceOperation` (line 112, no categories) | `AddForce()` (line 72, full categories) |
| Add selection | `AddSelectionOperation` (line 139, TODO) | `SelectEntry()` (line 167, more complete but no admissibility validation) |
| Remove selection | `RemoveSelectionOperation` (line 241) | `DeselectSelection()` (line 218) |
| Change count | `ChangeSelectionCountOperation` (line 251) | Not implemented |

The Engine path is **a better foundation**: it produces BattleScribe-format entryIds,
resolves categories through symbol links, and recursively auto-selects children. The
EditorServices operations have 10 `TODO` comments marking missing functionality.

However, the engine is **not yet fully correct**: it seeds costs and categories from
declared/link values, not modifier-applied effective values (`WhamRosterEngine.cs:416-441`),
and `DuplicateSelection` reuses subtree IDs (`WhamRosterEngine.cs:247-252`). The engine
is the right foundation to build on, not an already-correct source of truth.

`AddRootEntryFromSymbol` (line 189) is a hybrid that uses `SymbolKey` resolution but
still lives in EditorServices — it's a bridge that should eventually be replaced by
routing through `WhamRosterEngine`.

#### 4c. Logic Bypassing the Effective/Runtime Semantic Model

The sharper framing: logic bypasses the **effective (modifier-applied) semantic model**,
not merely "lives outside Compilation". This causes modifier/constraint drift.

Several pieces of roster logic read raw (non-effective) values:

1. **Cost aggregation** (`SourceNodeExtensions.WithUpdatedCostTotals()`, line 43):
   Walks the `RosterNode` tree directly, summing `CostNode.Value` fields. Does not use
   effective (modifier-applied) cost values from `EffectiveCostSymbol`.

2. **Cost scaling** (`SourceNodeExtensions.WithUpdatedNumberAndCosts()`, line 55):
   Scales cost node values by count ratio (`value * newCount / oldCount`). This is
   a SourceNode-level operation that ignores modifier-applied cost values.

3. **Constraint inspection** in WhamRosterEngine (`GetMinSelectionCount()`, line 511):
   Reads `IConstraintSymbol.Query.ReferenceValue` directly — these are raw (non-effective)
   constraint values. Should use effective constraints if modifiers can change min values.

4. **Duplicate constraint inspection** in SpecRosterEngineAdapter
   (`GetMinConstraintAutoSelect()`, line 242): Reads raw `ConstraintNode` declarations
   via `constraint.GetDeclaration()` instead of using the ISymbol API at all.

5. **Entry resolution** for child selections (`FindEntrySymbolForSelection()`,
   `SpecRosterEngineAdapter.cs` line 263): Brute-force searches all catalogues by ID
   instead of using `SymbolKey` or the Binder. **This is a correctness bug**, not just
   a brute-force smell: engine-created linked selections store `entryId` as
   `linkId::targetId` (`WhamRosterEngine.cs:402-409`), but `FindEntrySymbolForSelection()`
   compares the full composite ID against single symbol IDs. Link-backed selections can
   fail child resolution or resolve the wrong symbol when multiple links share a target.
   The rest of the runtime already treats `::` as a multi-segment path
   (`ConstraintEvaluator.cs:806-860`). **Fix**: resolve from bound `SourceEntryPath`/binder
   semantics, not global ID search.

6. **Derived-data divergence**: Source-node costs and effective costs can disagree.
   `WhamRosterEngine.CreateSelectionNode()` seeds source-node costs from declared/link
   costs (`WhamRosterEngine.cs:342-358, 416-441`), not modifier-applied effective costs.
   But `StateMapper` computes protocol totals from effective symbol costs
   (`StateMapper.cs:34-35, 186-225`). Once modifiers affect costs, the roster source tree,
   diagnostics, and projected UI/protocol state will disagree.

#### 4d. CompilationTracker / RosterState Divergence

After `WhamWorkspace.ApplyOperation()`:
1. `RosterEditor.State.Compilation` holds the post-operation compilation (built by the operation)
2. `CompilationTracker` gets a new roster tree and lazily rebuilds its own compilation

This means **two compilations can exist** for the same roster state: the one in the editor's
undo stack and the one lazily built by the tracker. In practice they produce equivalent
results, but the duplication wastes memory and could lead to subtle inconsistencies if the
construction differs.

This divergence exists **from construction time**, not just after edits: `OpenRoster()` for
existing rosters seeds `RosterEditor` from `tracker.GetCompilation()` (`WhamWorkspace.cs:193-199`),
while new rosters create `rosterState` first and then a separate `CompilationTracker`
(`WhamWorkspace.cs:217-225`). Any proposed catalogue-rebase or undo design must account for
this from-birth dual-compilation pattern.

**Root cause**: `CompilationTracker` was designed for lazy compilation from catalogue + roster
tree, but `RosterState` already carries a fully-built compilation. The tracker redundantly
rebuilds what the editor already has.

#### 4e. Missing Editor Abstractions

| Missing Abstraction | What It Would Provide | Current Workaround |
|----|----|----|
| **EntryOptionView** | Available entry + constraint status (min/max, must-select flag) + hidden state | Consumer manually calls `EntryResolver.GetAvailableEntries()` + inspects constraints |
| **SelectionDetailView** | Selection's effective name, costs, profiles, rules, categories, children, diagnostics | Consumer navigates `ISelectionSymbol.EffectiveSourceEntry` + filters diagnostics by location |
| **ForceView** | Force's effective entries, categories, selections, constraint summary | Consumer assembles from `IForceSymbol` properties manually |
| **RosterSummaryView** | Total costs, cost limits, force count, validation error count | Consumer aggregates from `IRosterSymbol` + `GetDiagnostics()` |
| **DiffView** | Changes between saved and current roster state | Does not exist |

#### 4f. Auto-Selection Entanglement

Auto-selection (creating child selections for entries with `min ≥ 1` constraints) is
embedded in two places:

1. `WhamRosterEngine.CreateSelectionWithAutoChildren()` — called during `SelectEntry()`
2. `SpecRosterEngineAdapter.AutoSelectRootEntries()` — called after `AddForce()`

These cannot be invoked separately. A UI that wants to show "what auto-selections would
happen" before applying them has no API for that. The auto-selection logic is also
duplicated between the engine (uses ISymbol constraints) and the adapter (uses raw
declaration constraints).

#### 4g. Roster Loading: No Convergence

When a roster saved against old catalogue data is opened:

1. `WhamWorkspace.OpenRoster(rosterNode)` wraps it in a compilation immediately
2. The Binder resolves entryIds against **current** catalogue symbols
3. If entries were removed or renamed in the new catalogue, the Binder produces
   diagnostics (unresolved references)
4. But there is **no diff** — the consumer cannot see what changed
5. Catalogue changes in `ReplaceCatalogue()` reset all roster editors, losing undo history
6. There is no read-only view showing the roster as it was saved

**The loading path and the editing path are entirely separate code paths**:
- Loading: `OpenRoster()` → `CompilationTracker` → `RosterEditor(initialState)`
- Editing: `ApplyOperation()` → `IRosterOperation.Apply()` → `RosterEditor.Push()`

An "update to latest catalogue" operation does not exist. A user must manually reconcile.

Note: `OpenRoster()` for *new* rosters already goes through `CreateRoster()`
(`WhamWorkspace.cs:211-229`), so this gap only affects existing rosters loaded from files.

#### 4h. Engine Parity Gap

`WhamRosterEngine` is **not yet a drop-in replacement** for EditorServices operations:

| Operation | EditorServices | WhamRosterEngine | Gap |
|-----------|---------------|-----------------|-----|
| Rename roster | `ChangeRosterNameOperation` | ❌ Missing | Must add |
| Change selection count | `ChangeSelectionCountOperation` | ❌ Missing | Must add |
| Change cost limits | `ChangeCostLimitOperation` | `SetCostLimit()` | ✅ Parity |
| Add selection | Multiple ops (incomplete) | `SelectEntry()` (complete) | Engine ahead |
| Set selection count (spec) | N/A | `SetSelectionCount` is a **no-op** in adapter | Spec gap |

The adapter's `SetSelectionCount` (`SpecRosterEngineAdapter.cs:144-148`) does nothing —
root entries use `SelectEntry` to add instances instead. This means count-change semantics
for existing selections are unimplemented in the engine path.

The count-change gap is deeper than just "missing API": current EditorServices count-change
(`SourceNodeExtensions.WithUpdatedNumberAndCosts()`, line 55) only rescales local costs
(`value * newCount / oldCount`) and leaves subselections as a TODO. Full count semantics
require: mandatory children scaling, collective selection behavior, and constraint
reevaluation — none of which are defined yet.

**Impact**: "Replace old ops with engine-based ones" requires both filling the API gap
and defining correct count-change semantics first.

#### 4i. Snapshot Memory and Recomputation Cost

Every roster edit creates a new `WhamCompilation` (`RosterState.ReplaceRoster()` →
`Compilation.ReplaceSourceTree()`). The undo stack stores full `RosterState` snapshots
(`RosterEditor.cs:9-12`), each carrying its own compilation.

- Incremental compilation (ADR-0007) mitigates per-edit cost (25 µs, shared catalogue)
- But long undo chains accumulate compilations in memory
- Every mutation also runs `WithUpdatedCostTotals()` — a full source-tree walk
- Adding on-demand view projections adds another computation per render cycle

This is acceptable at current scale but becomes a concern with:
- Large rosters (many selections)
- Long editing sessions (deep undo stacks)
- Real-time UI re-rendering on every keystroke

**Mitigation options**: Compilation pooling, lazy undo snapshots (store operations + base
state instead of full snapshots), memoized views keyed by compilation version.

#### 4j. Nested Selection Mutation Not Modeled

The engine only targets top-level selections within a force: `SelectEntry()` takes
`forceIndex`, `DeselectSelection()` takes `forceIndex + selectionIndex`, and
`DuplicateSelection()` takes `forceIndex + selectionIndex`
(`WhamRosterEngine.cs:167-253`). There is no API for mutating grandchildren or arbitrary
selection paths (e.g., "the 3rd option within the 2nd upgrade group within the 1st unit").

`SelectChildEntry()` in the adapter (`SpecRosterEngineAdapter.cs:78-110`) reconstructs
child paths ad hoc, but this is adapter-level code, not a general engine capability.

**Impact**: Rich tree views propose nested selection editing (§6a `SelectionView.Children`,
`AvailableChildEntries`), but the mutation API cannot target nested selections. A real
selection-path locator model (see §4a) is needed before deep-tree editing is possible.

---

## Part 2: Refactoring Plan

### 5. Design Principles

Following the Roslyn workspace model as reference:

1. **Compilation is truth**: All roster state questions are answered by the compilation.
   No logic should bypass symbols to read SourceNodes directly.
2. **Operations are pure functions**: `(RosterState, Params) → OperationResult`. No side
   effects. The result carries the new state, a change manifest, and diagnostics.
   Undo/redo is a stack of states, not a stack of inverses.
3. **Views are projections**: Computed from compilation, memoized per compilation version.
   Stale views are impossible because they're derived from the current compilation and
   invalidated when the compilation changes.
4. **Multi-step operations are explicit**: Auto-selection, catalogue sync, and alignment are
   separate user-triggered steps, not side effects of other operations.
5. **Workspace is an optional envelope**: The core editing model works without a workspace.
   Workspace adds multi-document management, identity, and events.
6. **Stable identity**: Forces and selections are addressed by stable identifiers (not
   positional indices or stale SourceNode references). Current `SymbolKey` encodes
   **catalogue-symbol identity**, not roster-instance identity — it cannot distinguish
   duplicated sibling selections. A dedicated selection-instance key (GUID or path-based)
   is needed; `SymbolKey` maps that to catalogue symbols as needed.

#### Source-of-Truth Invariants

| State | Authoritative Source | Must Stay In Sync With |
|-------|---------------------|------------------------|
| Roster tree (SourceNodes) | `RosterState.Compilation.SourceTrees` | Nothing — this IS the source |
| Effective values | Lazy computation from compilation symbols | Automatically derived |
| Undo history | `RosterEditor.stateStack` | Nothing — each entry is self-contained |
| Catalogue data | `WhamWorkspace.catalogueCompilation` | All roster `CompilationTracker` instances |
| Document identity | `WhamWorkspace.rosterStates` keys | Stable across mutations |

### 6. Concern A: Editor Views

#### 6a. Recommended Approach: Immutable Projections from Compilation

Create view types that project from `IRosterSymbol` + effective entries. These are
pure functions: `Compilation → View`, memoized per compilation version number.

**Proposed view types**:

```
RosterView
├── Name: string
├── Costs: IReadOnlyList<CostView>          ← (name, typeId, value, limit)
├── Forces: IReadOnlyList<ForceView>
├── DiagnosticSummary: (errors: int, warnings: int)
│
ForceView
├── InstanceId: string                                  ← runtime force instance locator
├── Name: string
├── CatalogueId: string
├── Categories: IReadOnlyList<CategoryView>
├── Selections: IReadOnlyList<SelectionView>
├── AvailableEntries: IReadOnlyList<EntryOptionView>   ← see caveat below
├── Profiles: IReadOnlyList<ProfileView>                ← effective
├── Rules: IReadOnlyList<RuleView>                      ← effective
│
EntryOptionView
├── EntryKey: SymbolKey                                  ← stable locator, NOT raw symbol
├── SourceGroupKey: SymbolKey?
├── EffectiveName: string
├── EffectiveIsHidden: bool
├── MinCount: int                                       ← from effective constraints
├── MaxCount: int?                                      ← from effective constraints
├── MustSelect: bool                                    ← min ≥ 1
├── Costs: IReadOnlyList<CostView>
│
SelectionView
├── InstanceId: string                                  ← runtime selection instance locator
├── Name: string                                        ← effective
├── EntryId: string
├── Type: string                                        ← unit/model/upgrade
├── Count: int
├── IsHidden: bool                                      ← effective
├── Costs: IReadOnlyList<CostView>                      ← effective × count
├── Categories: IReadOnlyList<CategoryView>
├── Profiles: IReadOnlyList<ProfileView>                ← effective
├── Rules: IReadOnlyList<RuleView>                      ← effective
├── Children: IReadOnlyList<SelectionView>
├── AvailableChildEntries: IReadOnlyList<EntryOptionView>
├── Diagnostics: IReadOnlyList<DiagnosticView>          ← filtered to this selection
├── Page: string?
├── PublicationId: string?
```

**Implementation location**: New project `WarHub.ArmouryModel.EditorServices.Views` or
within `EditorServices` as a `Views/` namespace. Views reference only the public ISymbol
API (`Extensions` project) — no dependency on `Concrete.Extensions` internals.

> **Concrete coupling risk**: While views target the public `ISymbol` API,
> `ModifierEvaluator` internally casts to concrete `SelectionSymbol` in several places
> (see §3c). This means effective evaluation is not truly abstract over `ISelectionSymbol`.
> Refactoring views around "public ISymbol only" is safe for **reading** effective values,
> but any future alternative symbol implementations would need to address
> `ModifierEvaluator`'s concrete dependencies. For now this is acceptable since there is
> only one concrete symbol implementation.

> **Why views carry locators, not raw `ISymbol` refs**: Every edit creates a new
> `WhamCompilation` (`RosterState.ReplaceRoster()`), and effective-symbol caches are only
> stable within one immutable compilation (`EffectiveEntryCache`). If views carried raw
> `ISelectionEntryContainerSymbol` objects, a cached view from compilation N could be used
> to drive mutations on compilation N+1 with stale catalogue semantics. Views carry
> `SymbolKey` (or a future instance-key) instead, which the operation resolves against the
> **current** compilation at mutation time.

> **`EntryResolver` caveat**: `EntryResolver` is **catalogue-only** — it takes
> `ICatalogueSymbol` / `ISelectionEntryContainerSymbol` and structurally flattens entries
> (`EntryResolver.cs:45-84,108-180`). It has no roster, force, or selection context, so it
> cannot model runtime-conditioned availability, modifier-driven hidden state, or
> context-sensitive min/max. `AvailableEntries` in views must distinguish "structural
> candidates" (from `EntryResolver`) from "runtime-available options" (requires
> roster-context-aware availability service that consults effective entries and constraints).
> This availability service does not yet exist and is needed for Phase 2.

**Construction**: A `RosterViewBuilder` takes a `WhamCompilation` and produces a
`RosterView`. Internally it:
1. Gets `IRosterSymbol` from compilation
2. For each force: gets effective entry, resolves available entries via `EntryResolver`
3. For each selection: gets `EffectiveSourceEntry`, maps resources
4. Filters diagnostics by owner entryId

This is structurally identical to what `StateMapper` does today for Protocol types,
but produces editor-friendly view types instead.

#### 6b. Alternative: Mutable ViewModel State

Create mutable view-model objects that observe compilation changes and update incrementally.

**Pros**: Potentially faster for UI frameworks that need change notifications.
**Cons**: Complex synchronization, stale state bugs, much more code.

**Recommendation**: Start with immutable projections memoized per compilation version.
Memoization ensures views are recomputed only when the compilation changes (not on every
UI query). Add incremental view diffing only if profiling shows view construction is a
bottleneck.

> **Cost note**: The 25µs benchmark (ADR-0007) measures **compilation tree replacement**
> (`ReplaceSourceTree`), not full effective-entry/resource traversal. View construction hits
> `EffectiveEntryCache`, `ModifierEvaluator`, and constraint evaluation — these are
> separate costs that scale with roster complexity. Profile view construction separately
> from compilation update to understand the real per-edit cost.

### 7. Concern B: Explicit Multi-Step Operations

#### 7a. OperationResult Pattern

Operations should return a rich result, not just a new state. This enables preview,
change tracking, and atomic undo:

```csharp
public record OperationResult
{
    public required RosterState NewState { get; init; }
    public IReadOnlyList<ChangeDescription> AppliedChanges { get; init; } = [];
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = [];
    public IReadOnlyList<SuggestedAction> SuggestedFollowUps { get; init; } = [];
}

public record ChangeDescription(ChangeKind Kind, string Description, SymbolKey? Target);
public record SuggestedAction(string Label, IRosterOperation Operation);
```

This keeps editing logic out of `Compilation` itself while providing full transparency:
- **Preview**: Run the operation, inspect `AppliedChanges` without committing
- **Undo**: The entire `OperationResult` is one atomic undo unit
- **Chaining**: `SuggestedFollowUps` lets the UI offer "also do X?" after an operation

#### 7b. Factor Auto-Selection Out of SelectEntry

Currently `WhamRosterEngine.SelectEntry()` calls `CreateSelectionWithAutoChildren()`
which recursively auto-selects children with `min ≥ 1` constraints.

**Proposed change**:

```csharp
// Engine: SelectEntry no longer auto-selects children
public RosterState SelectEntry(
    RosterState state, int forceIndex,
    ISelectionEntryContainerSymbol entry,
    ISelectionEntryGroupSymbol? sourceGroup = null,
    bool autoSelectChildren = true)  // ← backward compat default

// New: inspect what auto-selections would happen
public IReadOnlyList<AutoSelectAction> GetPendingAutoSelections(RosterState state)

// New: apply auto-selections as a separate step
public RosterState ApplyAutoSelections(RosterState state)
```

`AutoSelectAction` describes what would be created (entry, force, parent, count) without
actually creating it. This enables UI preview.

#### 7c. New Multi-Step Operation Types

| Operation | Input | Behavior |
|-----------|-------|----------|
| `ApplyAutoChanges` | Current `RosterState` | Inspects all forces/selections, auto-selects entries with `min ≥ 1` constraints that are not yet present. Reports what was added. |
| `SyncCatalogueData` | Current `RosterState` + catalogue `WhamCompilation` | Rebuilds roster compilation against new catalogue. Compares effective values (costs, names, rule text, profiles). Produces a `SyncReport` listing changes. **Applies only reference-level updates** to SourceNode tree (e.g., entryIds, category assignments); effective values (costs, profiles, rules) remain projection-only and are NOT written back into the persisted source tree. |
| `AlignRoster` | Current `RosterState` | Broader reconciliation: removes selections for entries that no longer exist, updates entryIds for renamed links, re-resolves categories. Superset of SyncCatalogue. |

> **Why not sync effective values into the source tree**: Effective values (costs, profiles,
> rules) are context-dependent — the same entry produces different effective values in
> different roster/force/selection contexts. Writing derived, context-dependent values into
> the persisted source tree mixes projection with storage, creates migration risk if
> modifier logic changes, and breaks the invariant that SourceNodes are input data and
> effective symbols are computed output. Keep effective values projection-only.

These become `IRosterOperation` implementations that use `WhamRosterEngine` internally:

```csharp
public record ApplyAutoChangesOperation : IRosterOperation
{
    public RosterState Apply(RosterState baseState)
    {
        var engine = new WhamRosterEngine();
        return engine.ApplyAutoSelections(baseState);
    }
}
```

#### 7d. Integration with Workspace

`WhamWorkspace` gains methods for multi-step operations:

```csharp
// Preview what auto-changes would do (no mutation)
public IReadOnlyList<AutoSelectAction> PreviewAutoChanges(DocumentId rosterId);

// Apply auto-changes (mutates, undoable)
public RosterState ApplyAutoChanges(DocumentId rosterId);

// Preview catalogue sync (no mutation)
public SyncReport PreviewCatalogueSync(DocumentId rosterId);

// Apply catalogue sync (mutates, undoable)
public RosterState ApplyCatalogueSync(DocumentId rosterId);
```

### 8. Concern C: Roster Loading Convergence

#### 8a. Proposed Loading Flow

```
OpenRoster(rosterNode)
  │
  ├── [1] Create read-only snapshot compilation
  │     RosterState with saved roster tree + current catalogue
  │     Binder resolves what it can; unresolved entries produce diagnostics
  │
  ├── [2] Compute RosterLoadReport
  │     Compare saved roster state against current catalogue:
  │     - Removed entries (entryId no longer resolvable)
  │     - Changed values (costs, names, rule text differ from effective)
  │     - New diagnostics (constraints now violated)
  │     - Orphaned categories (category entry removed)
  │
  ├── [3] Present read-only view to user
  │     RosterView with annotations: "this entry no longer exists",
  │     "cost changed from X to Y", etc.
  │
  └── [4] User triggers "Update" operation
        ApplyCatalogueSync(rosterId)
        │
        ├── Removes selections whose entries are gone
        ├── Updates costs/names/profiles from effective symbols
        ├── Produces diff of what changed
        └── Enters normal editing pipeline (undoable)
```

#### 8b. Convergence with Normal Editing

The key insight: **a "stale" roster is mostly a roster with diagnostics** — but not entirely.

- The Binder already handles unresolved references (produces diagnostics)
- Effective symbols already compute current catalogue values
- The difference between "editing a fresh roster" and "updating a stale roster" is largely
  which diagnostics are present and whether the user has been informed of changes

**However**: selections persist **copied** source-node costs and categories at creation time
(`WhamRosterEngine.cs:342-358, 416-499`), not live references to catalogue data. A roster
can bind successfully against a new catalogue yet still carry stale denormalized data
(old costs, old category assignments) with **no diagnostic**. The effective-symbol layer
computes correct values, but the source tree itself contains stale copies. This means
loading convergence requires not just checking diagnostics but also **semantic
re-materialization** of copied runtime data where effective values differ from stored values.

**Caveat**: `SyncCatalogueData` is harder without stable runtime identities (see §4a).
If catalogue entry IDs/paths move, structural diff alone may not tell you which selection
maps to which entry. The operation should be conservative: flag ambiguous cases for manual
resolution rather than guessing.

Therefore, `ApplyCatalogueSync` uses the same `WhamRosterEngine` operations:
```
For each selection with unresolved entry:
  → DeselectSelection (removes it)
For each selection with changed costs:
  → (rebuild selection node with current effective costs)
For each force with changed categories:
  → (rebuild force categories from current catalogue)
```

> **Migration risk**: The engine currently supports create/add/remove/duplicate/set-cost-limit
> but has no "update existing selection from new effective entry" primitive
> (`WhamRosterEngine.cs:33-279`). Syncing changed costs/names/categories may degenerate
> into remove/recreate, which breaks selection identity and makes undo semantics messy.
> Explicit sync/update primitives (e.g., `UpdateSelectionFromEntry(state, selectionLocator,
> newEntrySymbol)`) must be defined before promising convergence onto the current engine API.

This reuses the existing mutation infrastructure. No separate code path needed.

#### 8c. Catalogue Change Handling

Current behavior: `ReplaceCatalogue()` resets all roster editors (undo history lost).

**Proposed improvement**: Instead of resetting, treat catalogue change as a new
baseline. The workspace:
1. Rebuilds `CatalogueCompilation`
2. For each open roster: creates `RosterLoadReport` (diff against new catalogue)
3. Fires `WorkspaceChangeKind.CatalogueChanged` event with the report
4. Consumer can show the diff and trigger `ApplyCatalogueSync`
5. Undo history is preserved (the sync is a normal undoable operation)

> **Caveat — undo preservation requires unifying tracker/editor first (§4d).**
> Currently `WhamWorkspace` exposes two compilation sources: `Editor.State.Compilation`
> and `CompilationTracker.GetCompilation()` (`WhamWorkspace.cs:349-370`).
> `UpdateTrackerFromEditorLocked()` rebuilds the tracker against the tracker's current
> catalogue compilation (`WhamWorkspace.cs:445-450`). After catalogue swap, undoing into
> an old state leaves `GetRosterState()` on the old catalogue compilation while
> `GetRosterCompilation()` uses the new one. **Don't promise preserved undo until the
> dual-compilation source is collapsed** (Phase 1 prerequisite). Until then, the current
> reset-on-catalogue-change behavior is the safer default.

This requires: (a) collapsing `CompilationTracker` and `RosterEditor.State` into a single
source of truth (§4d fix), and (b) `CompilationTracker.WithCatalogueCompilation()` being
used to rebase the compilation rather than creating a brand-new `RosterEditor`.

### 9. WhamWorkspace: Keep vs. Lighten

#### Option A: Keep as Orchestrator (Recommended)

`WhamWorkspace` remains the top-level entry point. Changes:

1. **Replace old operations**: Phase out `AddSelectionOperation`, `AddSelectionFromLinkOp`,
   `AddForceOperation` in favor of new operations that delegate to `WhamRosterEngine`.
2. **Integrate engine**: Workspace owns a `WhamRosterEngine` instance (or creates one per
   operation). Operations become thin wrappers.
3. **Add view API**: `GetRosterView(DocumentId)` → `RosterView` projection.
4. **Add multi-step API**: `PreviewAutoChanges()`, `PreviewCatalogueSync()`, etc.
5. **Fix tracker divergence**: `CompilationTracker` is updated to reuse the compilation
   from `RosterEditor.State` instead of rebuilding, or is removed entirely if the editor's
   compilation is always authoritative.

**Pros**: Minimal disruption, workspace already has the threading/eventing model.
**Cons**: Workspace grows in surface area.

#### Option B: Lighter Workspace

Reduce `WhamWorkspace` to a document container. Move operations to standalone functions.

```csharp
// Workspace is just identity + tree storage
class WhamWorkspace
{
    DocumentId AddCatalogue(SourceNode node);
    DocumentId OpenRoster(SourceNode node);
    void CloseRoster(DocumentId id);
    WhamCompilation GetCatalogueCompilation();
    SourceTree GetRosterTree(DocumentId id);
    void SetRosterTree(DocumentId id, SourceTree tree);
    event WorkspaceChanged;
}

// Operations are standalone
static class RosterOperations
{
    static RosterState SelectEntry(RosterState state, ...);
    static RosterState ApplyAutoChanges(RosterState state);
    static RosterView CreateView(WhamCompilation compilation);
}

// Undo/redo is a separate concern
class UndoStack<T> { ... }
```

**Pros**: Cleaner separation, easier to test operations in isolation, workspace doesn't
need to know about every operation type.
**Cons**: Consumers must wire undo/redo themselves, threading model moves to consumer,
event firing must be explicit.

#### Recommendation

**Option A** for now. The current workspace design is sound (per ADR-0008) and the
conformance test infrastructure already validates the engine. The primary work is:
1. Delete/deprecate the incomplete SourceNode-based operations
2. Create new operations that wrap `WhamRosterEngine` calls
3. Add view projection API
4. Add multi-step operation API
5. **Collapse `CompilationTracker` and `RosterEditor.State` into a single source of truth**
   — this is the highest-priority fix to eliminate the dual-compilation issue (§4d)

Option B makes sense only if/when a second consumer (e.g., a web API) needs a different
orchestration model. At that point, extract the functional core.

### 10. Extensibility

#### 10a. New Selection Attributes / Modifier Types / Node Types

The 4-layer Roslyn model handles this well:
1. New node types → add `*Core` DTO + `SourceNode` wrapper (code-generated)
2. New symbol → add `INewSymbol` interface, concrete `NewSymbol` class
3. New effective wrapper → add `EffectiveNewSymbol` in `Effective/`
4. New modifier type → extend `ModifierEvaluator.Apply()`
5. View types automatically include new data (they project from ISymbol)

**No architectural changes needed** — the extension points are already in the model.

#### 10b. Catalogue Editor Support

A catalogue editor would operate on `WhamCompilation` (catalogue mode, no roster reference).
The workspace already manages catalogue documents via `AddCatalogue`/`RemoveCatalogue`/
`ReplaceCatalogue`.

What's needed:
1. **Catalogue operations**: `IRosterOperation`-style operations for catalogue mutations
   (add entry, modify modifier, etc.). These would be `ICatalogueOperation` with
   `CatalogueState Apply(CatalogueState)`.
2. **Catalogue editor**: Undo/redo stack for catalogue state (same pattern as `RosterEditor`).
3. **Catalogue views**: Projections showing entry trees, modifier chains, constraint summaries.
4. **Live preview**: Edit catalogue → see effects on open rosters (workspace already fires
   `CatalogueChanged` events).

The refactoring proposed in this plan is compatible: the workspace can manage both
catalogue and roster editors. The view projection pattern works for both.

### 11. Migration Path

#### Phase 1: Unify Operations

**Prerequisites** (engine parity — must complete first):
- Add `RenameRoster(state, name)` to `WhamRosterEngine`
- Add `ChangeSelectionCount(state, forceIndex, selectionIndex, newCount)` to `WhamRosterEngine`
- Implement `SetSelectionCount` in `SpecRosterEngineAdapter` (currently a no-op)
- Define stable selection identity model (extend `SymbolKey` or introduce path-based locators)

Then:
1. Create `EngineBackedOperations` that wrap `WhamRosterEngine` calls as `IRosterOperation`:
   - `EngineAddForceOperation(SymbolKey forceEntryKey, SymbolKey catalogueKey)`
   - `EngineSelectEntryOperation(SymbolKey entryKey, SymbolKey forceKey, bool autoSelect = false)`
   - `EngineSelectChildEntryOperation(...)`
   etc.
2. Deprecate old SourceNode-based operations (`AddSelectionOperation`, `AddForceOperation`, `AddSelectionFromLinkOp`)
3. Keep `AddRootEntryFromSymbol` temporarily (it already uses SymbolKey) but route it through the engine
4. Test: all existing workspace tests pass with new operations

#### Phase 2: Add View Projections

1. Create `RosterView`, `ForceView`, `SelectionView`, `EntryOptionView` types (with instance locators)
2. Create roster-context-aware **availability service** above `EntryResolver` that consults
   effective entries and constraints for runtime-conditioned hidden/min/max state.
   `AvailableEntries`/`AvailableChildEntries` must use this, not raw `EntryResolver` output.
3. Create `RosterViewBuilder` that constructs views from `WhamCompilation`
4. Add `WhamWorkspace.GetRosterView(DocumentId)` convenience method
5. Test: view output matches `StateMapper` Protocol output for conformance data

#### Phase 3: Multi-Step Operations

1. Factor auto-selection out of `WhamRosterEngine.SelectEntry()`
2. Implement `ApplyAutoChangesOperation`, `SyncCatalogueDataOperation`
3. Add preview APIs to workspace
4. Test: conformance specs still pass (auto-selection behavior preserved via default parameter)

#### Phase 4: Roster Loading Convergence

1. Implement `RosterLoadReport` (diff between saved and current state)
2. Modify `OpenRoster()` to produce report
3. Implement `ApplyCatalogueSync` operation
4. Change `ReplaceCatalogue()` to preserve undo history
5. Test: round-trip open → sync → edit → undo → redo

#### Phase 5: Cleanup

1. Remove deprecated operations
2. Remove `CompilationTracker` if redundant (or simplify to reuse editor compilation)
3. Update AGENTS.md architecture section
4. Update docs/roster-engine.md

---

## Appendix: File Reference Index

| File | Lines | Role in Architecture |
|------|-------|---------------------|
| `src/EditorServices/WhamWorkspace.cs` | 492 | Multi-roster orchestrator, owns mutations |
| `src/EditorServices/RosterEditor.cs` | 66 | Undo/redo stack |
| `src/EditorServices/RosterState.cs` | 46 | Immutable state record |
| `src/EditorServices/RosterOperations.cs` | 262 | SourceNode-based ops (partially obsolete) |
| `src/EditorServices/IRosterOperation.cs` | 11 | Operation interface |
| `src/EditorServices/RosterOperationKind.cs` | 19 | Operation type enum |
| `src/EditorServices/RosterOperationExtensions.cs` | 21 | Operation chaining |
| `src/EditorServices/LambdaOperation.cs` | 10 | Ad-hoc operation wrapper |
| `src/EditorServices/CompilationTracker.cs` | 48 | Lazy per-roster compilation |
| `src/EditorServices/DocumentId.cs` | 10 | Synthetic document identity |
| `src/EditorServices/SourceNodeExtensions.cs` | 134 | Node transforms, cost aggregation |
| `src/EditorServices/WorkspaceChangeKind.cs` | 15 | Change event types |
| `src/EditorServices/WorkspaceChangedEventArgs.cs` | 21 | Versioned event data |
| `src/RosterEngine/WhamRosterEngine.cs` | 631 | ISymbol-based functional API |
| `src/RosterEngine/EntryResolver.cs` | 203 | Entry flattening + group resolution |
| `src/RosterEngine.Spec/SpecRosterEngineAdapter.cs` | 330 | BattleScribe TestKit bridge |
| `src/RosterEngine.Spec/StateMapper.cs` | 293 | Symbol → Protocol mapper |
| `src/RosterEngine.Spec/ProtocolConverter.cs` | 473 | Protocol → SourceNode converter |
| `src/Concrete.Extensions/WhamCompilation.cs` | ~280 | Compilation: create, replace, diagnostics |
| `src/Concrete.Extensions/Symbols/Effective/EffectiveEntryCache.cs` | ~200 | Per-roster effective entry cache |
| `src/Concrete.Extensions/Symbols/Effective/EffectiveContainerEntrySymbol.cs` | ~150 | Abstract effective base |
| `src/Concrete.Extensions/Symbols/Effective/EffectiveEntrySymbol.cs` | ~80 | Selection entry wrapper |
| `src/Concrete.Extensions/Symbols/Effective/EffectiveForceEntrySymbol.cs` | ~60 | Force entry wrapper |
| `src/Concrete.Extensions/Symbols/Effective/ModifierEvaluator.cs` | ~400 | Modifier application engine |
| `src/Concrete.Extensions/Symbols/ConstraintEvaluator.cs` | ~300 | Constraint validation |
| `src/Concrete.Extensions/Symbols/RosterSymbol.cs` | ~150 | Roster: owns effective cache |
| `src/Concrete.Extensions/Symbols/SelectionSymbol.cs` | ~120 | Selection: lazy effective entry |
| `src/Concrete.Extensions/Symbols/ForceSymbol.cs` | ~100 | Force: lazy effective entry |

> **File paths**: All paths in this document use the abbreviated form
> `src/ProjectName/File.cs` corresponding to
> `src/WarHub.ArmouryModel.ProjectName/File.cs` in the repository.
