# Dotkiln

A package stack manager and safe update engine for .NET.

Dotkiln helps .NET developers define, reuse, apply, check, and safely update curated NuGet package stacks. Instead of remembering the same set of packages for every new project, you describe that set once in a `.dotkiln.yaml` file and let Dotkiln apply it consistently.

## Why Dotkiln Exists

Starting a new .NET project often means repeating the same setup:

- add logging
- add validation
- add Entity Framework Core
- add API documentation
- add test packages
- remember which package versions work together

That is tedious when creating a new project, and it becomes worse later when packages need updates. Standard dependency bots usually open one pull request per package, even when several packages should move together. Entity Framework Core packages, for example, are much safer to update as one group than as separate, unrelated changes.

Dotkiln treats a dependency baseline as a versioned stack: a small, readable file that says which packages belong together and which groups must be updated together.

## What Dotkiln Does

Dotkiln can:

- validate stack files before they are used
- apply a stack to a new or existing `.NET` project
- show drift between a project and a stack
- group related packages during update planning
- run updates in isolation so failed updates do not touch the real project
- search and publish stack files through a local/registry-style workflow
- output human-readable text or JSON for CI automation

## A Stack File

A stack is a `.dotkiln.yaml` file:

```yaml
schemaVersion: "0.1"
name: aspnet-webapi-standard
description: Opinionated baseline for a production ASP.NET Core minimal API
targetFramework: net8.0

packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
  - id: Serilog.Sinks.Console
    version: "6.*"
    group: logging
  - id: Microsoft.EntityFrameworkCore.SqlServer
    version: "8.0.*"
    group: ef-core
  - id: Microsoft.EntityFrameworkCore.Tools
    version: "8.0.*"
    group: ef-core
```

The `group` field matters. Packages in the same group are treated as one update unit, so related dependencies move together.

## Quick Start

Clone the repository and build it:

```powershell
dotnet build
```

Validate one of the built-in stacks:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Check whether a project matches a stack:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj
```

Preview applying a stack:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --dry-run
```

Run grouped update planning in dry-run mode:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --group ef-core --dry-run
```

## Installation

Dotkiln is currently run from source:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- <command>
```

The intended global tool installation shape is:

```powershell
dotnet tool install -g Elehko.Dotkiln.Cli
```

Once installed as a tool, commands will use:

```powershell
dotkiln <command>
```

## Commands

| Command | Purpose |
| --- | --- |
| `validate <stack>` | Parse and validate a stack file. |
| `new <stack> <project-name>` | Create a new .NET project and apply a stack. |
| `apply <stack> [project.csproj]` | Add or update direct package references from a stack. |
| `status <stack> [project.csproj]` | Show drift between the project and the stack. |
| `update <stack> [project.csproj] [--group name]` | Apply grouped updates in an isolated workspace and verify them. |
| `registry search <term>` | Search local/registry stack definitions. |
| `registry publish <stack-file>` | Publish a validated stack file into a registry directory. |

Every mutating command supports:

```powershell
--dry-run
```

Every command supports:

```powershell
--json
```

## Safety Model

Dotkiln is designed around a conservative update workflow.

When running `update`, Dotkiln:

1. groups packages by the stack's `group` field
2. creates an isolated workspace
3. applies package updates inside that workspace
4. runs `dotnet build`
5. runs discovered test projects when present
6. reports success or writes a failure log

If verification fails, the real project branch is not modified by the update workflow.

## Version Resolution

Dotkiln supports:

- exact versions, such as `8.0.30`
- wildcard versions, such as `8.0.*` or `6.*`
- basic NuGet-style ranges, such as `[8.0.0,9.0.0)`

Prerelease versions are excluded by default for wildcard and range expressions. A stack must explicitly request a prerelease version to allow one.

## Repository Layout

| Path | Purpose |
| --- | --- |
| `src/Elehko.Dotkiln.Core` | Stack models, parsing, validation, and version matching. |
| `src/Elehko.Dotkiln.Engine` | Project inspection, status calculation, NuGet resolution, and apply execution. |
| `src/Elehko.Dotkiln.Updates` | Grouped update planning, isolation, and verification. |
| `src/Elehko.Dotkiln.Cli` | Command-line entry point. |
| `src/Elehko.Dotkiln.Registry.Api` | Optional lightweight stack registry API. |
| `tests/Elehko.Dotkiln.Core.Tests` | Unit tests for core behavior and update planning. |
| `stacks` | Built-in example stack definitions. |
| `samples` | Sample projects used for local testing and demos. |
| `docs` | Guides and architecture decision records. |

## Project Status

Dotkiln is early-stage software. The local stack workflow is implemented, including validation, apply, status, grouped update planning, isolation, and build/test verification.

Still planned:

- production registry authentication and storage
- GitHub pull request creation for verified updates
- signed NuGet tool publishing
- broader integration and end-to-end test coverage
- migration from custom CLI parsing to `System.CommandLine`

## Development

Build everything:

```powershell
dotnet build Dotkiln.sln
```

Run tests:

```powershell
dotnet test Dotkiln.sln
```

Run the CLI from source:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- --help
```

## Contributing

Contributions are welcome. Start with [CONTRIBUTING.md](CONTRIBUTING.md), and check the docs in [docs](docs) for stack authoring and architecture notes.

## License

Dotkiln is licensed under the [MIT License](LICENSE).
