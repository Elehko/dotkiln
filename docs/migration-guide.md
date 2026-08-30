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

## Central Package Management

Dotkiln currently inspects direct `PackageReference` entries in `.csproj` files. Full `Directory.Packages.props` support is not implemented yet.

If your repository uses Central Package Management, evaluate Dotkiln carefully and prefer `--dry-run` until native support exists.

## Directory.Packages.props

Direct editing or drift detection against `Directory.Packages.props` is not currently implemented.

## Multi-project Solutions

Dotkiln commands currently operate on one project path at a time. For multi-project repositories, run `status` or `apply --dry-run` per project.

## Private NuGet Feeds

Dotkiln uses `dotnet add package` for package application, so package restore follows the NuGet configuration available to the .NET SDK. Resolution through Dotkiln's current NuGet resolver targets nuget.org.
