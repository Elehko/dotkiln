# Getting Started

Build the solution:

```powershell
dotnet build
```

Validate a stack file:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Inspect drift for a project:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj
```

Apply a stack to an existing project:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

Create a new project from a stack:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- new stacks/aspnet-webapi-standard.dotkiln.yaml ClientPortal.Api
```

Review grouped updates safely:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --group ef-core --dry-run
```
