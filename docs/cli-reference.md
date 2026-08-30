# CLI Reference

During development, run commands from source:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- <command>
```

After tool packaging, the command name is intended to be:

```powershell
dotkiln <command>
```

For machine-readable output details, see [JSON output](json-output.md). For preview-mode examples, see [Dry-run output](dry-run-output.md).

## validate

Parses and validates a stack file.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Example output:

```text
Stack 'aspnet-webapi-standard' is valid (6 packages).
```

Use this before publishing or sharing a stack. A validation failure means the stack should not be applied until the reported errors are fixed.

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

Example output with no drift and one informational extra:

```text
Stack: aspnet-webapi-standard
  api-docs     up to date
  ef-core      up to date
  logging      up to date
  validation   up to date

Extra packages not in stack (informational):
  Microsoft.AspNetCore.OpenApi 10.0.11

No drift detected. 1 extra packages found (not flagged).
```

Example output with drift:

```text
Stack: aspnet-webapi-standard
  api-docs     drift detected
    missing      Swashbuckle.AspNetCore (missing) -> 6.*
  logging      drift detected
    out-of-range Serilog.AspNetCore 7.0.0 -> 8.0.*

Drift detected. No extra packages to report.
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

Example dry-run output:

```text
Would run: dotnet add "C:\repo\MyApi\MyApi.csproj" package Serilog.AspNetCore --version 8.0.*
Would run: dotnet add "C:\repo\MyApi\MyApi.csproj" package Serilog.Sinks.Console --version 6.*
```

Example output when the project already satisfies the stack:

```text
Project already matches stack.
```

Example dirty working tree output:

```text
The git working tree has uncommitted changes.
Dotkiln relies on git for rollback safety and will not modify files
while uncommitted changes are present.

Commit or stash your changes, or re-run with --force to proceed anyway
(not recommended).
```

Options:

- `--dry-run`
- `--force`
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

Example dry-run output:

```text
Planning updates (1 groups)...
  ef-core: Would update 2 packages in isolation.
```

Example successful verification output:

```text
Planning updates (1 groups)...
  ef-core: Build and tests passed in isolation.
```

Example failure output:

```text
Planning updates (1 groups)...
  ef-core: Verification failed. No changes made to your branch.
    See C:\repo\Dotkiln-update-ef-core.log
```

Options:

- `--group <name>`
- `--dry-run`
- `--force`
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

Example dry-run output:

```text
Would run: dotnet new webapi -n ClientPortal.Api
```

Use `--template` to choose another `dotnet new` template:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- new stacks/console-tool-standard.dotkiln.yaml WorkerTool --template console
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

Example output:

```text
aspnet-webapi-standard.dotkiln.yaml
```

Options:

- `--registry-dir <path>` defaults to `stacks`
- `--json`

## registry publish

Validates and copies a stack file into a registry directory.

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish path/to/custom.dotkiln.yaml
```

Example dry-run output:

```text
Would publish custom-stack to stacks\custom-stack.dotkiln.yaml
```

Options:

- `--registry-dir <path>` defaults to `stacks`
- `--dry-run`
- `--json`
