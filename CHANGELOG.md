# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
* Support for BattleScribe v2.03 data format ([#47])
* "Latest" channel (folder) for Catalogue.xsd
* `wham --info` command that displays more detailed program info ([#64]).

### Changed
* Current version of schema changed to v2.03 (latest)
* `NodeFactory` in `WarHub.ArmouryModel.Source` namespace was rewritten to provide
  much more defaults, use other Nodes as value providers, and add more methods ([#58]).
* `INodeWithCore<TCore>` is now covariant on `TCore` parameter, updating it's
  signature to `interface INodeWithCore<out TCore>` ([#63]).

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


[Unreleased]: https://github.com/WarHub/wham/compare/v0.6.17...HEAD
[0.6.17]: https://github.com/WarHub/wham/compare/v0.3.0...v0.6.17
