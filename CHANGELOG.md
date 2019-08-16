# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.6.16] - 2019-08-16

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


[Unreleased]: https://github.com/WarHub/wham/compare/v0.6.16...HEAD
[0.6.16]: https://github.com/WarHub/wham/compare/v0.3.0...v0.6.16
