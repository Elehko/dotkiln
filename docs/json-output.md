# JSON Output

Every Dotkiln command accepts `--json`.

The current CLI uses `System.Text.Json` defaults, so JSON property names are PascalCase for typed result objects. Some error payloads use lowercase names because they are command-level error objects.

JSON output is intended for scripts and CI. Exit codes still matter; do not rely only on the JSON body.

## Common Error Shape

If a command fails after argument parsing and `--json` is present, Dotkiln writes:

```json
{
  "succeeded": false,
  "error": "The git working tree has uncommitted changes."
}
```

Fields:

| Field | Type | Meaning |
| --- | --- | --- |
| `succeeded` | boolean | Always `false` for this error shape. |
| `error` | string | Human-readable failure message. |

## validate

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml --json
```

Example:

```json
{
  "IsValid": true,
  "Errors": [],
  "Name": "aspnet-webapi-standard",
  "PackageCount": 6
}
```

Schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `IsValid` | boolean | Whether stack validation passed. |
| `Errors` | string[] | Validation errors. Empty when valid. |
| `Name` | string | Stack name. |
| `PackageCount` | number | Number of package entries in the stack. |

Exit codes:

- `0` valid
- `2` invalid stack or usage error
- `3` load or environment failure

## status

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --json
```

Example:

```json
{
  "StackName": "aspnet-webapi-standard",
  "Groups": [
    {
      "Name": "logging",
      "Packages": [
        {
          "Id": "Serilog.AspNetCore",
          "RequestedVersion": "8.0.*",
          "InstalledVersion": "8.0.3",
          "Group": "logging",
          "State": "up-to-date"
        }
      ],
      "IsUpToDate": true
    }
  ],
  "ExtraPackages": [
    {
      "Id": "Microsoft.AspNetCore.OpenApi",
      "Version": "10.0.11"
    }
  ],
  "HasDrift": false,
  "HasExtraPackages": true
}
```

Top-level schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `StackName` | string | Name of the stack used for comparison. |
| `Groups` | GroupDrift[] | Stack packages grouped by update group. |
| `ExtraPackages` | ExtraPackage[] | Direct project packages not declared in the stack and not suppressed by `.dotkilnignore`. |
| `HasDrift` | boolean | `true` when any stack package is missing or out of range. |
| `HasExtraPackages` | boolean | `true` when informational extra packages are present. |

`GroupDrift` schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Name` | string | Group name. |
| `Packages` | PackageDrift[] | Stack package status entries in this group. |
| `IsUpToDate` | boolean | `true` when every package in the group is `up-to-date`. |

`PackageDrift` schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Id` | string | NuGet package ID. |
| `RequestedVersion` | string | Version expression from the stack. |
| `InstalledVersion` | string or null | Version in the project, or `null` when missing. |
| `Group` | string | Effective update group. |
| `State` | string | `up-to-date`, `missing`, or `out-of-range`. |

`ExtraPackage` schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Id` | string | Extra package ID. |
| `Version` | string | Installed package version. |

Exit codes:

- `0` no drift, even if extras exist
- `1` drift found
- `2` usage error
- `3` load or environment failure

## apply

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/console-tool-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --dry-run --json
```

Example:

```json
{
  "Succeeded": true,
  "Messages": [
    "Would run: dotnet add \"C:\\repo\\TestApp\\TestApp.csproj\" package Microsoft.Extensions.Hosting --version 8.0.*",
    "Would run: dotnet add \"C:\\repo\\TestApp\\TestApp.csproj\" package Serilog.Extensions.Hosting --version 8.0.*"
  ]
}
```

Schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Succeeded` | boolean | Whether the apply operation succeeded. |
| `Messages` | string[] | Process output or dry-run command previews. |

In raw JSON, quotes in command strings are escaped by JSON, for example `\u0022` or `\"` depending on serializer behavior and display context.

## update

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --group ef-core --dry-run --json
```

Example:

```json
[
  {
    "Group": {
      "Name": "ef-core",
      "Packages": [
        {
          "Id": "Microsoft.EntityFrameworkCore.SqlServer",
          "Version": "8.0.*",
          "Group": "ef-core"
        }
      ]
    },
    "Succeeded": true,
    "Message": "Would update 2 packages in isolation.",
    "LogPath": null
  }
]
```

Schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Group` | UpdateGroup | Group that was planned or run. |
| `Succeeded` | boolean | Whether this group succeeded. |
| `Message` | string | Human-readable result. |
| `LogPath` | string or null | Failure log path, when available. |

`UpdateGroup` schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Name` | string | Group name. |
| `Packages` | PackageEntry[] | Packages included in the group. |

`PackageEntry` schema:

| Field | Type | Meaning |
| --- | --- | --- |
| `Id` | string | NuGet package ID. |
| `Version` | string | Stack version expression. |
| `Group` | string or null | Declared group from the stack. |

## new

Dry-run command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- new stacks/aspnet-webapi-standard.dotkiln.yaml ClientPortal.Api --dry-run --json
```

Example:

```json
{
  "succeeded": true,
  "command": "dotnet new webapi -n ClientPortal.Api",
  "stack": "aspnet-webapi-standard"
}
```

Non-dry-run success shape:

```json
{
  "Succeeded": true,
  "projectPath": "C:\\repo\\ClientPortal.Api\\ClientPortal.Api.csproj",
  "Messages": [
    "Project already matches stack."
  ]
}
```

## registry search

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry search webapi --json
```

Example:

```json
[
  "aspnet-webapi-standard.dotkiln.yaml"
]
```

## registry publish

Dry-run command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish path/to/custom.dotkiln.yaml --dry-run --json
```

Example:

```json
{
  "Name": "custom-stack",
  "destination": "stacks\\custom-stack.dotkiln.yaml"
}
```
