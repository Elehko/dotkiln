# Dotkiln

Dotkiln is a package stack manager and safe update engine for .NET. It lets teams define reusable NuGet package stacks, apply them to projects, check drift, and plan grouped updates for packages that should move together.

This repository is at the initial implementation stage. The current CLI supports:

- `dotkiln validate <stack-file>`
- `dotkiln status <stack-file> <project.csproj>`
- `dotkiln update <stack-file> [--group name]`

## Quick Start

```powershell
dotnet build
dotnet run --project src/Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.Dotkiln.yaml
dotnet run --project src/Dotkiln.Cli -- update stacks/aspnet-webapi-standard.Dotkiln.yaml --group ef-core
```

## Project Layout

- `src/Dotkiln.Core` - stack models, parsing, and validation
- `src/Dotkiln.Engine` - project inspection and stack apply planning
- `src/Dotkiln.Updates` - grouped update planning and verification primitives
- `src/Dotkiln.Cli` - command-line entry point
- `stacks` - built-in stack definitions
- `docs` - contributor and architecture documentation

## License

MIT
