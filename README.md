# Dotkiln

Dotkiln is a package stack manager and safe update engine for .NET. It lets teams define reusable NuGet package stacks, apply them to projects, check drift, and plan grouped updates for packages that should move together.

This repository is at the initial implementation stage. The current CLI supports:

- `dotkiln validate <stack-file>`
- `dotkiln status <stack-file> <project.csproj>`
- `dotkiln update <stack-file> [--group name]`

## Quick Start

```powershell
dotnet build
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml --group ef-core
```

## Project Layout

- `src/Elehko.Dotkiln.Core` - stack models, parsing, and validation
- `src/Elehko.Dotkiln.Engine` - project inspection and stack apply planning
- `src/Elehko.Dotkiln.Updates` - grouped update planning and verification primitives
- `src/Elehko.Dotkiln.Cli` - command-line entry point
- `stacks` - built-in stack definitions
- `docs` - contributor and architecture documentation

## License

MIT
