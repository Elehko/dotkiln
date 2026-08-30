# Stack Schema

Dotkiln stack files use the `.dotkiln.yaml` extension.

## Required Fields

```yaml
name: aspnet-webapi-standard
description: Production ASP.NET Core baseline
targetFramework: net8.0
packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
```

| Field | Required | Description |
| --- | --- | --- |
| `name` | Yes | Stable stack name. Also used by local registry publish. |
| `description` | No | Human-readable stack description. |
| `targetFramework` | Yes | Intended target framework, such as `net8.0`. Currently documented and parsed, but not enforced against project files. |
| `packages` | Yes | List of direct NuGet package references required by the stack. |

## Optional Fields

| Field | Default | Description |
| --- | --- | --- |
| `schemaVersion` | `0.1` | Stack schema version. Current implementation supports `0.1`. |
| `snippet` | none | Relative path to a local starter snippet. Dotkiln prints this file after apply; it does not edit source files. |
| `group` | package ID | Optional package-level update group. |

## Package Fields

| Field | Required | Description |
| --- | --- | --- |
| `id` | Yes | NuGet package ID. |
| `version` | Yes | Exact version, wildcard, or basic range. |
| `group` | No | Update group name. |

## Validation Rules

Dotkiln currently validates:

- `name` is present
- `targetFramework` is present
- `schemaVersion` is supported
- at least one package exists
- every package has `id` and `version`
- package IDs are not duplicated
- version expressions are supported
- group names use lowercase letters, numbers, and hyphens

Unknown top-level fields are ignored by the current parser. Do not rely on unknown fields for behavior.

## Full Example

```yaml
schemaVersion: "0.1"
name: aspnet-webapi-standard
description: Production ASP.NET Core baseline
targetFramework: net8.0

packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
  - id: Serilog.Sinks.Console
    version: "6.*"
    group: logging
  - id: Microsoft.EntityFrameworkCore.SqlServer
    version: "8.0.*"
    group: ef-core

snippet: setup/program-cs-additions.txt
```
