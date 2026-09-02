# Getting Started

Install Dotkiln from NuGet:

```powershell
dotnet tool install --global Elehko.Dotkiln.Cli
```

The examples below use stack files and sample projects from this repository. Run them from the repository root:

```powershell
cd C:\path\to\Dotkiln
```

Build the solution:

```powershell
dotnet build
```

Validate a stack file:

```powershell
dotkiln validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

Inspect drift for a project:

```powershell
dotkiln status stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj
```

Apply a stack to an existing project:

```powershell
dotkiln apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

Create a new project from a stack:

```powershell
dotkiln new stacks/aspnet-webapi-standard.dotkiln.yaml ClientPortal.Api
```

Review grouped updates safely:

```powershell
dotkiln update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --group ef-core --dry-run
```

During local development, you can still run the CLI from source:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- <command>
```

Stack paths are resolved relative to your current directory. If you run `dotkiln validate stacks/aspnet-webapi-standard.dotkiln.yaml` from `C:\Users\you`, Dotkiln looks for `C:\Users\you\stacks\aspnet-webapi-standard.dotkiln.yaml`. Use `cd` first or pass an absolute stack path.

## More Documentation

- [Stack schema](stack-schema.md)
- [Versioning rules](versioning-rules.md)
- [Drift detection](drift-detection.md)
- [Apply behavior](apply-behavior.md)
- [Update behavior](update-behavior.md)
- [Package groups](package-groups.md)
- [CLI reference](cli-reference.md)
- [JSON output](json-output.md)
- [Dry-run output](dry-run-output.md)
- [Ignore and exclusion rules](ignore-and-exclusion-rules.md)
- [Safety and recovery](safety-and-recovery.md)
- [URL stack sources and caching](url-stack-sources.md)
- [Registry workflows](registry-workflows.md)
- [CI/CD examples](ci-cd-examples.md)
- [Migration guide](migration-guide.md)
- [FAQ](faq.md)
