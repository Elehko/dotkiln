# Dotkiln

Dotkiln is a package stack manager and safe update engine for .NET. It lets teams define reusable NuGet package stacks, apply them to projects, check drift, and plan grouped updates for packages that should move together.

This repository is at the initial implementation stage. The current CLI supports:

- `dotkiln new <stack> <project-name>`
- `dotkiln apply <stack-file-or-url> [project.csproj]`
- `dotkiln validate <stack-file>`
- `dotkiln status <stack-file> <project.csproj>`
- `dotkiln update <stack-file> [--group name]`
- `dotkiln registry search <term>`
- `dotkiln registry publish <stack-file>`

## Quick Start

```powershell
dotnet build
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml --group ef-core
```

Every mutating command supports `--dry-run`, and every command supports `--json` for automation.

## Project Layout

- `src/Elehko.Dotkiln.Core` - stack models, parsing, and validation
- `src/Elehko.Dotkiln.Engine` - project inspection and stack apply planning
- `src/Elehko.Dotkiln.Updates` - grouped update planning and verification primitives
- `src/Elehko.Dotkiln.Cli` - command-line entry point
- `src/Elehko.Dotkiln.Registry.Api` - optional lightweight stack registry API
- `stacks` - built-in stack definitions
- `docs` - contributor and architecture documentation

## License

MIT
