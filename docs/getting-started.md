# Getting Started

Build the solution:

```powershell
dotnet build
```

Validate a stack file:

```powershell
dotnet run --project src/Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.Dotkiln.yaml
```

Inspect drift for a project:

```powershell
dotnet run --project src/Dotkiln.Cli -- status stacks/aspnet-webapi-standard.Dotkiln.yaml path/to/project.csproj
```
