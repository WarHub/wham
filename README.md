# wham

[![NuGet packages](https://img.shields.io/nuget/v/WarHub.ArmouryModel.Source.svg?logo=nuget)](https://www.nuget.org/profiles/warhub)
[![CI GitHub Action](https://github.com/WarHub/wham/workflows/CI/badge.svg)](https://github.com/WarHub/wham/actions?query=workflow%3ACI+branch%3Amain)
[![License](https://img.shields.io/github/license/WarHub/wham.svg)](https://github.com/WarHub/wham/blob/main/LICENSE.md)

> `WarHub.ArmouryModel` library and `wham` tool

Foundational .NET library for wargame datafile tools, written in C#.

## Overview

This is the home of the `WarHub.ArmouryModel` library handling wargame roster and data files.
It consists of:

* `wham` - a CLI tool (Command Line) used to manage, convert and publish datafiles,
  distributed as .NET Global Tool (.NET SDK v10+ required)
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

All libraries, unless otherwise specifed, target `.NET 10`.

There are also test projects and `WarHub.ArmouryModel.Source.CodeGeneration` project which contains
code generator used to build `.Source` library. This code generator uses C# Source Generators.

## Usage

### `wham` installation

To install `wham` command line tool:

1. please install [`.NET SDK` v10](https://dotnet.microsoft.com/download)
  for your platform.
1. In your shell/command line run `dotnet tool install wham -g`
1. You can check if the tool is available: `wham --version` should show what version exactly is running.

This will install preview of `wham` CLI tool in your user-space (not system global),
and so it doesn't require root/admin permissions. (Although installation of .NET SDK can).

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

The development branch is the `main` branch. Stable releases are marked via `vX.Y.Z` tags.
This project uses `Nerdbank.GitVersioning` package that automatically generates version numbers
for assemblies and packages from git tree. It won't work if the git clone is *shallow* or otherwise
incomplete.

## Release Process

### Preparing a release

1. Create a PR that sets `version.json` to the stable version (e.g. `"0.14"`) and updates
   `CHANGELOG.md` with the release date. Merge the PR.
2. Tag the merged commit: `git tag vX.Y.Z && git push origin vX.Y.Z`
3. After tagging, create a PR to bump `version.json` to the next alpha version
   (e.g. `"0.15.0-alpha.{height}"`) and add an `[Unreleased]` section to `CHANGELOG.md`.

### Publishing packages

Packages are published via the **Publish** workflow (`.github/workflows/publish.yml`).

**Via GitHub CLI or Actions UI** (recommended):

```bash
# Publish to GitHub Packages only
gh workflow run publish.yml -f ref=vX.Y.Z -f push-to=github

# Publish to nuget.org only
gh workflow run publish.yml -f ref=vX.Y.Z -f push-to=nuget

# Publish to both registries
gh workflow run publish.yml -f ref=vX.Y.Z -f push-to=all
```

**Via ChatOps** (requires `SLASH_COMMAND_DISPATCH_TOKEN` secret):

Comment on any issue or PR:
```
/deploy ref=vX.Y.Z env=all
```

### NuGet trusted publishing

NuGet.org publishing uses [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
(OIDC) instead of long-lived API keys. The trusted publishing policy is configured on nuget.org for:

| Field              | Value              |
|--------------------|--------------------|
| Package Owner      | `warhub`           |
| Repository Owner   | `WarHub`           |
| Repository         | `wham`             |
| Workflow           | `publish.yml`      |
| Environment        | `package-release`  |

The `push` job in `publish.yml` uses `environment: package-release` which provides
environment protection rules (e.g. required reviewers) as a gate before publishing.

### Tagging

Tags can also be created via the **Tag** workflow:

```bash
gh workflow run tag-command.yml -f ref=main
```

Or via ChatOps: `/tag ref=main`

## Credits

The library is MIT licensed (license in repo root).
Created by Amadeusz Sadowski ([**@amis92**](https://github.com/amis92)).
[**BattleScribe**](https://battlescribe.net/) name is used without permission under fair-use laws.
