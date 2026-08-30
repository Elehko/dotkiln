# Apply Behavior

`apply` brings an existing project closer to a stack definition.

## What Apply Does

`apply` currently:

- reads the target `.csproj`
- compares direct `PackageReference` entries with the stack
- adds missing stack packages
- updates out-of-range package versions
- uses `dotnet add package` instead of manually editing XML
- prints a starter snippet if the stack references one and the file exists locally

## What Apply Does Not Do

`apply` currently does not:

- remove packages that are not in the stack
- edit application source files such as `Program.cs`
- enforce target framework drift
- ask for confirmation before making changes

Use `--dry-run` to preview package commands before changing the project.

## Before And After

Before:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="7.0.0" />
```

Stack:

```yaml
packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
```

After:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
```

The exact version is resolved from NuGet. Wildcards and ranges resolve to the latest stable matching version by default.

## Preview Mode

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

Example output:

```text
Would run: dotnet add "C:\repo\MyApi\MyApi.csproj" package Serilog.AspNetCore --version 8.0.*
```
