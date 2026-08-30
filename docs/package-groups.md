# Package Groups

Package groups define which stack packages should be treated as one update unit.

## Rules

- Group names are user-defined.
- A package can belong to one group.
- Groups are not nested.
- If a package has no `group`, its package ID is used as its update group.
- Group names are validated when present.
- Valid group names use lowercase letters, numbers, and hyphens.

## Example

```yaml
packages:
  - id: Microsoft.EntityFrameworkCore
    version: "8.0.*"
    group: ef-core

  - id: Microsoft.EntityFrameworkCore.SqlServer
    version: "8.0.*"
    group: ef-core
```

In this example, both packages are updated together when running:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --group ef-core
```

## Best Practices

Use a group when packages are released together, tested together, or commonly require matching major/minor versions.

Good group candidates:

- Entity Framework Core packages
- Serilog core and sink packages used as one logging baseline
- ASP.NET API documentation packages
- test framework packages that should move together

Avoid using one large group for unrelated packages. Smaller groups make failures easier to understand.
