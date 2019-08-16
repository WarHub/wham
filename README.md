# wham - WarHub.ArmouryModel

Foundational .NET library for wargame datafile tools, written in C#.

[![NuGet package](https://img.shields.io/nuget/v/WarHub.ArmouryModel.Source.svg)](https://www.nuget.org/packages?q=wham)
[![Build Status](https://dev.azure.com/warhub/wham/_apis/build/status/WarHub.wham?branchName=master)](https://dev.azure.com/warhub/wham/_build/latest?definitionId=1&branchName=master)
[![MyGet package](https://img.shields.io/myget/warhub/v/WarHub.ArmouryModel.Source.svg?label=myget-ci)](https://www.myget.org/feed/Packages/warhub)
[![Join the chat at gitter!](https://img.shields.io/gitter/room/WarHub/wham.svg)](https://gitter.im/WarHub/wham?utm_source=badge&utm_medium=badge&utm_content=badge)
[![License](https://img.shields.io/github/license/WarHub/wham.svg)](https://github.com/WarHub/wham/blob/master/LICENSE.md)

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
* `WarHub.ArmouryModel.Workspaces.Gitree` implements project model for *gitree* where
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
  `dotnet tool install wham -g --version 0.6.16`
3. You can check if the tool is available: `wham --version` should show what version exactly is running.

This will install preview of `wham` CLI tool in your user-space (not system global),
and so it doesn't require root/admin permissions. (Although installation of .NET Core SDK does).

### `wham` features

* converts BattleScribe workspace (xml files: `.cat`/`.catz`/`.gst`/`.gstz`)
  into *gitree* workspace (mutliple small files in directory trees) that
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

## Development

The development branch is the `master` branch. Stable releases are marked via `vX.Y.Z` tags.
This project uses `Nerdbank.GitVersioning` package that automatically generates version numbers
for assemblies and packages from git tree. It won't work if the git clone is *shallow* or otherwise
incomplete.

## Credits

The library is MIT licensed (license in repo root).
Created by Amadeusz Sadowski ([**@amis92**](https://github.com/amis92)).
[**BattleScribe**](https://battlescribe.net/) name is used without permission under fair-use laws.
[`CodeGeneration.Roslyn`][CodeGenRoslyn] is created by Andrew Arnott. Thanks!
Continuous integration is provided by Azure DevOps. Much appreciated!


[CodeGenRoslyn]: https://github.com/AArnott/CodeGeneration.Roslyn
