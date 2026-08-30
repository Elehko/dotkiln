# CLI Reference

During development, run commands from source:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- <command>
```

After tool packaging, the command name is intended to be:

```powershell
dotkiln <command>
```

## validate

Parses and validates a stack file.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Options:

- `--json`

Exit codes:

- `0` valid
- `2` invalid stack or usage error
- `3` environment or load failure

## status

Shows drift between a project and a stack.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj
```

Options:

- `--json`

Exit codes:

- `0` no drift
- `1` drift found
- `2` usage error
- `3` environment or load failure

## apply

Adds missing stack packages and updates out-of-range package references.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj
```

Options:

- `--dry-run`
- `--json`

Exit codes:

- `0` apply succeeded
- `2` usage or validation error
- `3` package restore, network, or process failure

## update

Runs grouped package updates inside an isolated workspace and verifies build/tests.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --group ef-core
```

Options:

- `--group <name>`
- `--dry-run`
- `--json`

Exit codes:

- `0` every planned group passed
- `1` one or more groups failed verification
- `2` usage or validation error
- `3` environment, load, network, or process failure

## new

Creates a new .NET project and applies a stack.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- new stacks/aspnet-webapi-standard.dotkiln.yaml ClientPortal.Api
```

Options:

- `--template <dotnet-template-name>` defaults to `webapi`
- `--dry-run`
- `--json`

## registry search

Searches local stack files in a registry directory.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry search webapi
```

Options:

- `--registry-dir <path>` defaults to `stacks`
- `--json`

## registry publish

Validates and copies a stack file into a registry directory.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish path/to/custom.dotkiln.yaml
```

Options:

- `--registry-dir <path>` defaults to `stacks`
- `--dry-run`
- `--json`
