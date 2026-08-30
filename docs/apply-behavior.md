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

By default, `apply` refuses to run when the target project is inside a git repository with uncommitted changes. Dotkiln relies on git for rollback safety, so it requires a clean working tree before mutating files.

Use `--force` to override the clean-tree guard:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --force
```

This is not recommended unless you have reviewed the working tree and are comfortable resolving or reverting mixed changes yourself.

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

## Typical Run

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml MyApp.csproj --force
```

Shortened example output:

```text
info : Adding PackageReference for package 'Serilog.AspNetCore' into project 'C:\repo\MyApp\MyApp.csproj'.
info : PackageReference for package 'Serilog.AspNetCore' version '8.0.3' added to file 'C:\repo\MyApp\MyApp.csproj'.
log  : Restored C:\repo\MyApp\MyApp.csproj.
```

Meaning: Dotkiln delegated the package write to `dotnet add package`. The .NET SDK updated the project file and restored packages.

## Preview Mode

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

Example output:

```text
Would run: dotnet add "C:\repo\MyApi\MyApi.csproj" package Serilog.AspNetCore --version 8.0.*
```

Meaning: Dotkiln found a package operation it would run, but it did not modify the project because `--dry-run` was used.

## Already Matching Project

If every stack package is already installed and in range, `apply` exits successfully:

```text
Project already matches stack.
```
