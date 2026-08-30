# Writing a Stack

A Dotkiln stack is a YAML file with project metadata and direct NuGet packages.

```yaml
name: aspnet-webapi-standard
description: Opinionated baseline for a production ASP.NET Core minimal API
targetFramework: net8.0

packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
```

Use `group` for packages that must be upgraded together. For example, Entity Framework Core package references should usually share the same `ef-core` group.

Use exact versions when a stack is meant to be fully reproducible. Use patch or minor ranges when the stack should allow compatible updates within a package family.

Supported version expressions:

- Exact versions: `8.0.8`
- Wildcards: `8.0.*`, `6.*`
- Basic NuGet-style ranges: `[8.0.0,9.0.0)`

For the complete schema and validation rules, see [Stack schema](stack-schema.md). For version matching details, see [Versioning rules](versioning-rules.md).
