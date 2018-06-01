# wham - WarHub.ArmouryModel

C# cornerstone library for wargame datafile tools.

[![NuGet package](https://img.shields.io/nuget/v/Amadevus.RecordGenerator.svg)](https://www.nuget.org/packages?q=warhub+armourymodel)
[![Build status](https://img.shields.io/appveyor/ci/amis92/wham.svg)](https://ci.appveyor.com/project/amis92/wham/branch/master)
[![MyGet package](https://img.shields.io/myget/warhub/v/WarHub.ArmouryModel.Source.svg?label=myget-ci)](https://www.myget.org/feed/Packages/warhub)
[![Join the chat at gitter!](https://img.shields.io/gitter/room/WarHub/wham.svg)](https://gitter.im/WarHub/wham?utm_source=badge&utm_medium=badge&utm_content=badge)
[![License](https://img.shields.io/github/license/WarHub/wham.svg)](https://github.com/WarHub/wham/blob/master/LICENSE)

NuGet packages: [WarHub NuGet](https://www.nuget.org/profiles/warhub)

MyGet packages (Continuous Integration channel):
[WarHub MyGet](https://www.myget.org/feed/Packages/warhub)

## Overview

This is the home of the `WarHub.ArmouryModel` library handling wargame roster and data files.
It consists of:
* `wham` - a CLI tool (Command Line) used to manage, convert and publish datafiles,
  distributed as .NET Core Global Tool (.NET Core SDK v2.1+ required)
* `WarHub.ArmouryModel.Source` library provides API to manage and interact
  with wargaming data files (game systems, catalogues) and rosters.
* `WarHub.ArmouryModel.Source.BattleScribe` provides convenient methods to load and save
  BattleScribe XML datafiles.
* `WarHub.ArmouryModel.ProjectModel` library provides API to operate on wham workspaces (abstraction)
  and their configuration (`.whamproj` configuration files).
* `WarHub.ArmouryModel.Workspaces.BattleScribe` implements project model for BattleScribe format.
* `WarHub.ArmouryModel.Workspaces.JsonFolder` implements project model for *folder model* where
  directory and file structure is a part of datafile shape, building datafile from massive amounts
  of tiny files. It's mostly designed to work well with VCS (Version Control Systems) such as **git**.

All libraries, unless otherwise specifed, target `NETStandard 2.0`.

There are also test projects and `WarHub.ArmouryModel.Source.CodeGeneration` project which contains
code generator used to build `.Source` library. This code generator uses
[`CodeGeneration.Roslyn`][CodeGenRoslyn] tooling framework.

## Usage

### `wham` installation

To install `wham` command line tool:
1. please install [`.NET Core SDK` v2.1 or higher](https://www.microsoft.com/net/download)
  for your platform.
2. In your shell/command line run
  `dotnet tool install -g --add-source https://www.myget.org/F/warhub/api/v3/index.json
 --version 0.5.6-alpha-gab70099344 wham`
3. You can check if the tool is available: `wham version` should show what version exactly is running.

This will install preview of `wham` CLI tool in your user-space (not system global),
and so it doesn't require root/admin permissions. (Although installation of .NET Core SDK does).

### `wham` features

* converts BattleScribe workspace (xml files: `.cat`/`.catz`/`.gst`/`.gstz`)
  into *folder model* workspace (mutliple small files in directory trees) that
  behaves with **git** well.
* publishes BattleScribe format distribution files:
  * `.bsr` repository distribution package - zip archive containing datafiles (`.cat` and `.gst`)
    and DataIndex `index.xml` file.
  * `index.bsi` which is DataIndex `index.xml`-containing zip archive.
  * `index.xml` DataIndex.
  * `.cat`/`.gst` datafiles
  * `.catz`/`.gstz` zipped datafiles

### `wham` usage

You can always run `wham -?` or `wham -h` or `wham [action] -?` to get help about the tool/action.

* `wham publish [artifacts] [options]` publishes distribution files. Available artifact types
  (when building multiple, separate with comma and no spaces e.g. `wham publish bsr,zip`):
    * `bsr` - zipped datafile container with index;
    * `index` - `index.xml` DataIndex file;
    * `bsi` - `index.bsi` zipped DataIndex file;
    * `xml` - `.cat`/`.gst` xml datafiles;
    * `zip` - `.catz`/`.gstz` zipped xml datafiles.
   
  This command outputs generated artifacts in the `artifacts/` directory by default,
  unless the `.whamproj` configuration specifies otherwise. Both can be overridden 
  by using `-d <Destination>` option, passing another directory path.
* `wham convertxml [-s <Source>] [-d <Destination>]` converts xml workspace
  into *folder model* workspace. Parameters are directory paths
  (these default to working directory):
    * `Source` - containing XML/BattleScribe format workspace
    * `Destination` - where the *folder model* workspace will be rooted (and `.whamproj` written).
* `wham convertjson -s <Source> -d <Destination>` converts *folder model* workspace
  into xml/BattleScribe workspace model. Required parameters are directory paths:
    * `Source` - where the *folder model* workspace is rooted (and `.whamproj` exists)
      *or* the path to the `.whamproj` file directly.
    * `Destination` - directory to write XML/BattleScribe workspace format files in.

## Development

The development branch is the `master` branch.
`release` is for stable releases which are pushed to NuGet.
This project uses `Nerdbank.GitVersioning` package that automatically generates version numbers
for assemblies and packages from git tree. It won't work if the git clone is *shallow* or otherwise
incomplete.

## Credits

The library is MIT licensed (license in repo root).
Created by Amadeusz Sadowski ([**@amis92**](https://github.com/amis92)).
[**BattleScribe**](https://battlescribe.net/) name is used without permission under fair-use laws.
[`CodeGeneration.Roslyn`][CodeGenRoslyn] is created by Andrew Arnott. Thanks!
Continuous integration is provided by AppVeyor. Much appreciated!


[CodeGenRoslyn]: https://github.com/AArnott/CodeGeneration.Roslyn