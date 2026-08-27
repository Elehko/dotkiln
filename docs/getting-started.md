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
