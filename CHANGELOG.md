# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Roster engine, symbol model and workspace.** A Roslyn-inspired layer that
  turns BattleScribe data into a bound, queryable symbol graph, and an engine
  that builds and mutates rosters against it. Landed as a stack of eight
  reviewable layers, split out of the long-running `feature/roster-engine`
  branch:
  - `WarHub.ArmouryModel.Extensions` — the `ISymbol` abstraction, binders,
    diagnostics and compilation entry points. ([#317])
  - `WarHub.ArmouryModel.Concrete.Extensions` — concrete symbols, binding,
    effective values and `WhamCompilation`, with incremental compilation via a
    references model. ([#319])
  - `WarHub.ArmouryModel.Concrete.Extensions.Generators` — source generator
    emitting reference-checking boilerplate from `[Bound]` annotations. ([#318])
  - `WarHub.ArmouryModel.EditorServices` — `WhamWorkspace`, roster editing
    operations and Handlebars-based formatting. ([#320])
  - `WarHub.ArmouryModel.RosterEngine` — roster construction, force management,
    entry discovery and selection mutation over the symbol model. ([#321])
  - `WarHub.ArmouryModel.RosterEngine.Spec` — adapter for the BattleScribe-spec
    conformance suite, added as a git submodule. 410 specs, 362 passing.
    ([#322])
  - `Phalanx.SampleDataset` — sample BattleScribe data used by tests and
    benchmarks. ([#315])

  Design decisions are recorded in `docs/adrs/`.
- `WarHub.ArmouryModel.Source.Yaml` — reader for BattleScribe-schema data
  expressed as YAML, plus file-based setup on the spec adapter. ([#325])
- Support for the NewRecruit extensions to the BattleScribe data format: new
  nodes, attributes and enum values across `ModifierKind`, `ConditionKind`,
  `ConditionGroupKind`, `ConstraintKind` and `SelectionEntryKind`, modelled over
  BattleScribe v2.03. ([#308])
- Doc comments and round-trip/schema tests for the string-valued
  `ModifierKind`s (`prepend`, `append`, `replace`). The `prepend` modifier was
  originally contributed by [@The4D6] in [#221] and independently reimplemented
  in [#308]; this restores the documentation and adds the test coverage that
  came with the original contribution. ([#326])

### Changed

- **Breaking**: `NodeFactory.Force` takes an optional `catalogue` parameter in
  second position: `Force(ForceEntryNode forceEntry, CatalogueBaseNode? catalogue = null, string? id = null)`.
  This is source- and binary-breaking for callers that passed `id`
  positionally — `Force(entry, "some-id")` no longer compiles and must become
  `Force(entry, id: "some-id")`. Callers using named arguments, or passing only
  the force entry, are unaffected. ([#316])

  Previously the catalogue was always inferred from the force entry's nearest
  `CatalogueBaseNode` ancestor. That is wrong when a force entry is defined in
  the gamesystem but the resulting force should reference a different
  catalogue; the inference is still the default when `catalogue` is omitted.
- `IsXmlZipped(this XmlDocumentKind)` is retained but now reports whether *every*
  file of that kind is zipped, which is only true of `RepoDistribution`. Use
  `IsXmlZippedPath(this string)` to test an actual file. ([#324])
- deps: update Microsoft.CodeAnalysis.CSharp to 5.6.0,
  Microsoft.CodeAnalysis.Analyzers to 5.6.0, Serilog to 4.4.0,
  System.CommandLine to 2.0.10, Microsoft.NET.Test.Sdk to 18.8.1, NSubstitute to
  6.0.0, coverlet.collector to 10.0.1, Nerdbank.GitVersioning to 3.10.91 and
  FluentAssertions to 7.2.2. FluentAssertions is deliberately held on the 7.x
  line — 8.0.0 relicensed from Apache-2.0 to a commercial licence. YamlDotNet is
  held at 17.1.0 because 18.1.0 breaks the conformance suite's spec loader.
  ([#327])
- build: add an empty `Directory.Build.targets` so the MSBuild search stops at
  the repository root. ([#313])

### Fixed

- Zipped BattleScribe datafiles (`.catz`, `.gstz`, `.rosz`, `.bsi`) can be read
  again. `IsXmlZipped` asked whether a document *kind*'s extension was zipped,
  but a kind's extension list is built as `(plain, zipped)` and the check only
  ever looked at the first entry — so it was always `false` and every zipped
  file was handed to the XML reader as raw ZIP bytes, failing with
  `XmlException: Data at the root level is invalid`. Zipped-ness is a property
  of the path, not the kind, so `LoadSourceAuto` now decides from the file
  extension via the new `IsXmlZippedPath`. This affected `wham publish` and
  `wham convert xml` on any source directory containing zipped datafiles.
  ([#311], [#324])
- `XmlDocumentKind.Unknown.IsXmlZipped()` no longer throws
  `KeyNotFoundException`. ([#324])
- The spec adapter's `ProtocolConverter` no longer guesses at unrecognised wire
  values. Its parsers each ended in a silent fallback, so an unknown modifier
  kind became `Set` and an unknown constraint kind became `Minimum` — a fixture
  using `prepend` was evaluated as `set`, and `exactly` as `min`, with no error.
  The maps are now derived from the `[XmlEnum]` attributes that define these
  names, so they stay complete as the model grows, and a genuinely unknown value
  throws. ([#322])
- The conformance harness no longer discards specs it cannot load. A bare
  `catch { continue; }` was silently removing them from the run, hiding 48
  fixtures that the pinned spec revision cannot deserialize. They are now
  reported as skipped. ([#322])

[#221]: https://github.com/WarHub/wham/pull/221
[#308]: https://github.com/WarHub/wham/pull/308
[#311]: https://github.com/WarHub/wham/issues/311
[#313]: https://github.com/WarHub/wham/pull/313
[#315]: https://github.com/WarHub/wham/pull/315
[#316]: https://github.com/WarHub/wham/pull/316
[#317]: https://github.com/WarHub/wham/pull/317
[#318]: https://github.com/WarHub/wham/pull/318
[#319]: https://github.com/WarHub/wham/pull/319
[#320]: https://github.com/WarHub/wham/pull/320
[#321]: https://github.com/WarHub/wham/pull/321
[#322]: https://github.com/WarHub/wham/pull/322
[#324]: https://github.com/WarHub/wham/pull/324
[#325]: https://github.com/WarHub/wham/pull/325
[#326]: https://github.com/WarHub/wham/pull/326
[#327]: https://github.com/WarHub/wham/pull/327
[@The4D6]: https://github.com/The4D6

## [0.14.0] - 2026-02-28

### Changed

- chore: Target `net10.0` TFM. ([#259])
- deps: update dependencies and build tools. ([#259])
- deps: update System.CommandLine to 2.0.x. ([#259])
- deps: update Serilog to 4.x, Serilog.Sinks.Console to 6.x.
- deps: update Microsoft.CodeAnalysis.CSharp to 5.0.0. ([#259])
- refactor: migrate source generator to IIncrementalGenerator. ([#260])
- refactor: remove morelinq dependency, replace with local implementations.
- refactor: remove Optional dependency, replace with nullable references.
- refactor: code cleanup — remove warning suppressions, fix sync-over-async,
  move InternalsVisibleTo to csproj.

[#259]: https://github.com/WarHub/wham/pull/259
[#260]: https://github.com/WarHub/wham/pull/260

## [0.13.0] - 2021-09-16

### Changed

- `ListNode<T>`-derived classes (e.g. `ItemListNode`) now have covariant return with self type
  in `WithNodes` method override (e.g. `ItemListNode ItemListNode.WithNodes(nodes)`). ([#133])
- `wham publish` fixed to work on repositories with no `gst` file. ([#135])
  
[#133]: https://github.com/WarHub/wham/pull/133
[#135]: https://github.com/WarHub/wham/pull/135

## [0.12.0] - 2020-11-12

### Added

- Feat: Add support for roster migrations ([#131]).

### Changed

- deps: Use .NET 5 SDK for build, target only .NET 5 with NuGet packages.
- Fix: RosterTag name in v2.03 XSD schema (was `tags`, is `tag`) ([#121]).
- Refactor: Core types are now C# 9 nominal records ([#125]).
- Refactor: Xml serializers are now manually crafted using C# 9/.NET 5 Source Generators;
  this allows Xml serialization to work with C#9 records and (primarily) ImmutableArrays,
  as well as greatly reduce warmup-time in deployments like Blazor WASM, *and* remove
  any Reflection from that process ([#130]).

[#121]: https://github.com/WarHub/wham/pull/121
[#125]: https://github.com/WarHub/wham/pull/125
[#130]: https://github.com/WarHub/wham/pull/130
[#131]: https://github.com/WarHub/wham/pull/131

## [0.11.0] - 2020-07-15

### Added

- Added some base classes:
  - added QueryFilteredBase (QueryBase with ChildId), inherits from QueryBase
  - added SelectionParentBase, base of Force and Selection
  - added ModifierBase, base of Modifier and ModifierGroup
- Added descriptive comments to ModifierKind
- Added RosterTag type
- Added missing fields to be fully 2.03 schema compliant:
  - added CostType.Hidden
  - added ModifierGroup.ModifierGroups
  - added many fields to Publication:
    - ShortName
    - Publisher
    - PublicationDate
    - PublisherUrl
  - added CustomNotes and Tags to Roster
  - added CustomName and CustomNotes to RosterElementBase

### Changed

- CharacteristicType now inherits Commentable
- renamed SelectorBase to QueryBase
- BSv2.03 schema with the above changes

## [0.10.0] - 2020-06-03

### Added

- `readme` field on datafile root elements (gamesystem and catalogue) in code
  and in 2.03 schema ([#115]).

### Changed

- In `WarHub.ArmouryModel.ProjectModel.IDatafileInfo` interface
  `SourceNode? GetData()` changed to `async Task<SourceNode?> GetDataAsync()`;
  this also results in some APIs changing to be `async` as well, especially in
  `Workspaces.BattleScribe` namespace ([#117]).

[#115]: https://github.com/WarHub/wham/pull/115
[#117]: https://github.com/WarHub/wham/pull/117

## [0.9.0] - 2020-05-21

### Added

- C# 8.0 Nullable Reference Types support (NRTs) ([#111]).
- `netstandard2.1` targets in libraries to support NRT-enabled TFMs.

### Removed

- `WhamNodeCoreAttribute` class from `.Source` library (added by accident in v0.8).

[#111]: https://github.com/WarHub/wham/pull/111

## [0.8.0] - 2020-05-14

### Added

- `NodeList<T>.Slice(int, int)` method to support ranges in C#8 ([#89]).
- `comment` field on data elements (and in 2.03 schema) ([#108]).

### Changed

- Renamed `BattleScribeVersion` static well-known values from `V0_00` to `V0x00` ([#86]).
- Renamed `Resources` to `XmlResources` (`.Source` library) ([#86]).
- Changed `NodeList<T>.GetEnumerator()` and `ListNode<T>.GetEnumerator()`
  return type to custom enumerator `NodeList<T>.Enumerator` that's optimized
  for performance ([#89]).
- Changed all parameter names across the board to be camelCased. Also changed
  parameter names of `With` methods to `value` to mirror setters ([#90]).
- All Node `With` methods for collection properties are now extension methods,
  with the exception the ones where parameter name is the same as the property's
  that's being modified ([#90]).
- Renamed a couple of Source Core/Node properties ([#91]):
  - Category: IsPrimary -> Primary
  - CategoryLink: IsPrimary -> Primary
  - EntryBase: IsHidden -> Hidden
  - Repeat:
    - Repeats -> RepeatCount
    - IsRoundUp -> RoundUp
  - SelectorBase: PercentValue -> IsValuePercentage
  - SelectionEntryBase: Import -> Exported

[#86]: https://github.com/WarHub/wham/pull/86
[#89]: https://github.com/WarHub/wham/pull/89
[#90]: https://github.com/WarHub/wham/pull/90
[#91]: https://github.com/WarHub/wham/pull/91
[#108]: https://github.com/WarHub/wham/pull/108

## [0.7.0] - 2019-11-05

### Added

- Support for BattleScribe v2.03 data format ([#47])
- "Latest" channel (folder) for `Catalogue.xsd`.
- `wham --info` command that displays more detailed program info ([#64]).
- EntryLink now has `SelectionEntries`, `SelectionEntryGroups` and `EntryLinks` lists ([#77]).

### Changed

- Current version of schema changed to v2.03 (latest)
- `NodeFactory` in `WarHub.ArmouryModel.Source` namespace was rewritten to provide
  much more defaults, use other Nodes as value providers, and add more methods ([#58]).
- `INodeWithCore<TCore>` is now covariant on `TCore` parameter, updating it's
  signature to `interface INodeWithCore<out TCore>` ([#63]).
- Cores and Nodes' With and Update methods now check for equality of old and new,
  and when they're equal, return current instance ([#75]).
- `SourceRewriter` implementation fixed to actually work ([#75]).
- EntryLink now inherits from SelectionEntryBase instead of EntryBase ([#77]).

## Removed

- `SelectionEntryNode.CategoryEntryId` property was removed. It was a leftover from old format, pre-2.01 ([#59]).
- `SourceNode.Core` property was removed (#59). All other classes that
  previously declared it still have it.
- `SourceNode(NodeCore core, SourceNode parent)` constructor was replaced with
  a new one: `SourceNode(SourceNode parent)` since the `Core` property is no
  longer part of this type  ([#63]).
- `SourceNode` no longer implements `INodeWithCore<NodeCore>`  ([#63]).

[#47]: https://github.com/WarHub/wham/pull/47
[#58]: https://github.com/WarHub/wham/pull/58
[#59]: https://github.com/WarHub/wham/pull/59
[#63]: https://github.com/WarHub/wham/pull/63
[#64]: https://github.com/WarHub/wham/pull/64
[#75]: https://github.com/WarHub/wham/pull/75
[#77]: https://github.com/WarHub/wham/pull/77

## [0.6.17] - 2019-08-16

### Added

- Support for BattleScribe v2.02 data format ([#39])
- CLI tool `wham` installable via `dotnet install tool -g wham`
- XSD for `catalogue`, `roster` and `game system` XML, accessible via
  `WarHub.AmouryModel.Source.XmlFormat.Resources` class
- Migration XSL transforms for `game system` and `catalogue` XML files,
  accessible via `WarHub.AmouryModel.Source.XmlFormat.Resources` class. Supported
  BattleScribe versions:
  - 1.15
  - 2.00
  - 2.01
  - 2.02
- Migrations can be applied via `WarHub.ArmourtModel.Source.BattleScribe.DataVersionManagement`
  type. This type has methods that allow applying single migration XSL, as well as applying
  migrations that take given input to newest version known.

[#39]: https://github.com/WarHub/wham/pull/39

[Unreleased]: https://github.com/WarHub/wham/compare/v0.14.0...HEAD
[0.14.0]: https://github.com/WarHub/wham/compare/v0.13.0...v0.14.0
[0.13.0]: https://github.com/WarHub/wham/compare/v0.12.0...v0.13.0
[0.12.0]: https://github.com/WarHub/wham/compare/v0.11.0...v0.12.0
[0.11.0]: https://github.com/WarHub/wham/compare/v0.10.0...v0.11.0
[0.10.0]: https://github.com/WarHub/wham/compare/v0.9.0...v0.10.0
[0.9.0]: https://github.com/WarHub/wham/compare/v0.8.0...v0.9.0
[0.8.0]: https://github.com/WarHub/wham/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/WarHub/wham/compare/v0.6.17...v0.7.0
[0.6.17]: https://github.com/WarHub/wham/compare/v0.3.0...v0.6.17
