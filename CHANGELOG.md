# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Feat: Add support for roster migrations ([#131]).

### Changed
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
* `readme` field on datafile root elements (gamesystem and catalogue) in code
  and in 2.03 schema ([#115]).

### Changed
* In `WarHub.ArmouryModel.ProjectModel.IDatafileInfo` interface
  `SourceNode? GetData()` changed to `async Task<SourceNode?> GetDataAsync()`;
  this also results in some APIs changing to be `async` as well, especially in
  `Workspaces.BattleScribe` namespace ([#117]).

[#115]: https://github.com/WarHub/wham/pull/115
[#117]: https://github.com/WarHub/wham/pull/117

## [0.9.0] - 2020-05-21

### Added
* C# 8.0 Nullable Reference Types support (NRTs) ([#111]).
* `netstandard2.1` targets in libraries to support NRT-enabled TFMs.

### Removed
* `WhamNodeCoreAttribute` class from `.Source` library (added by accident in v0.8).

[#111]: https://github.com/WarHub/wham/pull/111

## [0.8.0] - 2020-05-14

### Added
* `NodeList<T>.Slice(int, int)` method to support ranges in C#8 ([#89]).
* `comment` field on data elements (and in 2.03 schema) ([#108]).

### Changed
* Renamed `BattleScribeVersion` static well-known values from `V0_00` to `V0x00` ([#86]).
* Renamed `Resources` to `XmlResources` (`.Source` library) ([#86]).
* Changed `NodeList<T>.GetEnumerator()` and `ListNode<T>.GetEnumerator()`
  return type to custom enumerator `NodeList<T>.Enumerator` that's optimized
  for performance ([#89]).
* Changed all parameter names across the board to be camelCased. Also changed
  parameter names of `With` methods to `value` to mirror setters ([#90]).
* All Node `With` methods for collection properties are now extension methods,
  with the exception the ones where parameter name is the same as the property's
  that's being modified ([#90]).
* Renamed a couple of Source Core/Node properties ([#91]):
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
* Support for BattleScribe v2.03 data format ([#47])
* "Latest" channel (folder) for `Catalogue.xsd`.
* `wham --info` command that displays more detailed program info ([#64]).
* EntryLink now has `SelectionEntries`, `SelectionEntryGroups` and `EntryLinks` lists ([#77]).

### Changed
* Current version of schema changed to v2.03 (latest)
* `NodeFactory` in `WarHub.ArmouryModel.Source` namespace was rewritten to provide
  much more defaults, use other Nodes as value providers, and add more methods ([#58]).
* `INodeWithCore<TCore>` is now covariant on `TCore` parameter, updating it's
  signature to `interface INodeWithCore<out TCore>` ([#63]).
* Cores and Nodes' With and Update methods now check for equality of old and new,
  and when they're equal, return current instance ([#75]).
* `SourceRewriter` implementation fixed to actually work ([#75]).
* EntryLink now inherits from SelectionEntryBase instead of EntryBase ([#77]).

## Removed
* `SelectionEntryNode.CategoryEntryId` property was removed. It was a leftover from old format, pre-2.01 ([#59]).
* `SourceNode.Core` property was removed (#59). All other classes that
  previously declared it still have it.
* `SourceNode(NodeCore core, SourceNode parent)` constructor was replaced with
  a new one: `SourceNode(SourceNode parent)` since the `Core` property is no
  longer part of this type  ([#63]).
* `SourceNode` no longer implements `INodeWithCore<NodeCore>`  ([#63]).


[#47]: https://github.com/WarHub/wham/pull/47
[#58]: https://github.com/WarHub/wham/pull/58
[#59]: https://github.com/WarHub/wham/pull/59
[#63]: https://github.com/WarHub/wham/pull/63
[#64]: https://github.com/WarHub/wham/pull/64
[#75]: https://github.com/WarHub/wham/pull/75
[#77]: https://github.com/WarHub/wham/pull/77

## [0.6.17] - 2019-08-16

### Added
* Support for BattleScribe v2.02 data format ([#39])
* CLI tool `wham` installable via `dotnet install tool -g wham`
* XSD for `catalogue`, `roster` and `game system` XML, accessible via
  `WarHub.AmouryModel.Source.XmlFormat.Resources` class
* Migration XSL transforms for `game system` and `catalogue` XML files,
  accessible via `WarHub.AmouryModel.Source.XmlFormat.Resources` class. Supported
  BattleScribe versions:
  - 1.15
  - 2.00
  - 2.01
  - 2.02
* Migrations can be applied via `WarHub.ArmourtModel.Source.BattleScribe.DataVersionManagement`
  type. This type has methods that allow applying single migration XSL, as well as applying
  migrations that take given input to newest version known.



[#39]: https://github.com/WarHub/wham/pull/39


[Unreleased]: https://github.com/WarHub/wham/compare/v0.11.0...HEAD
[0.11.0]: https://github.com/WarHub/wham/compare/v0.10.0...v0.11.0
[0.10.0]: https://github.com/WarHub/wham/compare/v0.9.0...v0.10.0
[0.9.0]: https://github.com/WarHub/wham/compare/v0.8.0...v0.9.0
[0.8.0]: https://github.com/WarHub/wham/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/WarHub/wham/compare/v0.6.17...v0.7.0
[0.6.17]: https://github.com/WarHub/wham/compare/v0.3.0...v0.6.17
