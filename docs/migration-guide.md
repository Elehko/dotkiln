# Migration Guide

This guide describes how to evaluate Dotkiln in an existing .NET repository.

## Existing Repositories

Start by creating a stack that mirrors the package baseline you already expect.

1. Review direct `PackageReference` entries in the project.
2. Move the baseline packages into a `.dotkiln.yaml` file.
3. Group related packages, such as EF Core packages.
4. Run `validate`.
5. Run `status`.
6. Run `apply --dry-run`.

Example:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/my-service.dotkiln.yaml
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/my-service.dotkiln.yaml src/MyService/MyService.csproj
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/my-service.dotkiln.yaml src/MyService/MyService.csproj --dry-run
```

Expected `status` output during migration often looks like this:

```text
Stack: my-service
  logging      up to date
  validation   drift detected
    missing      FluentValidation.AspNetCore (missing) -> 11.*

Extra packages not in stack (informational):
  AutoMapper 13.0.1

Drift detected. 1 extra packages found (not flagged).
```

Interpretation:

- `FluentValidation.AspNetCore` is part of the baseline but missing, so it is drift.
- `AutoMapper` is installed but not part of the baseline, so it is informational.
- If `AutoMapper` is expected for this project, add it to `.dotkilnignore` or leave it visible for audit.

## Suggested First Stack

For an existing ASP.NET Core API, start small:

```yaml
schemaVersion: "0.1"
name: my-service-baseline
description: Baseline packages for MyService
targetFramework: net8.0

packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
  - id: Serilog.Sinks.Console
    version: "6.*"
    group: logging
```

Run `status` against one project first. Add more packages only after the initial baseline is easy to understand.

## Central Package Management

Dotkiln currently inspects direct `PackageReference` entries in `.csproj` files. Full `Directory.Packages.props` support is not implemented yet.

If your repository uses Central Package Management, evaluate Dotkiln carefully and prefer `--dry-run` until native support exists.

## Directory.Packages.props

Direct editing or drift detection against `Directory.Packages.props` is not currently implemented.

## Multi-project Solutions

Dotkiln commands currently operate on one project path at a time. For multi-project repositories, run `status` or `apply --dry-run` per project.

Example:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/my-service.dotkiln.yaml src/Api/Api.csproj
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/my-service.dotkiln.yaml src/Worker/Worker.csproj
```

This makes each project's drift and extra-package report explicit.

## Private NuGet Feeds

Dotkiln uses `dotnet add package` for package application, so package restore follows the NuGet configuration available to the .NET SDK. Resolution through Dotkiln's current NuGet resolver targets nuget.org.
