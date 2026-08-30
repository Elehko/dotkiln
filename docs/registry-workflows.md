# Registry Workflows

Dotkiln currently has two registry-related surfaces:

- CLI commands for a local registry directory
- an optional minimal HTTP API project

The current registry is intentionally lightweight. It is suitable for local development, demos, and early internal workflows. Production authentication, rate limiting, object storage, moderation, and vulnerability policy enforcement are not implemented yet.

## Local Registry Directory

By default, CLI registry commands use the `stacks` directory.

Search:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry search webapi
```

Example output:

```text
aspnet-webapi-standard.dotkiln.yaml
```

Search another directory:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry search webapi --registry-dir C:\stacks
```

Publish dry-run:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish custom.dotkiln.yaml --registry-dir stacks --dry-run
```

Example output:

```text
Would publish custom-stack to stacks\custom-stack.dotkiln.yaml
```

Publish:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish custom.dotkiln.yaml --registry-dir stacks
```

Example output:

```text
Published custom-stack to stacks\custom-stack.dotkiln.yaml
```

What publish does:

- loads the stack file
- validates it
- creates the registry directory if needed
- copies the stack file to `<registry-dir>\<stack-name>.dotkiln.yaml`

What publish does not do yet:

- authenticate the user
- scan packages for vulnerabilities
- sign the stack
- publish to a remote service
- preserve previous versions

## Minimal Registry API

The optional API project lives at:

```text
src/Elehko.Dotkiln.Registry.Api
```

Run it:

```powershell
dotnet run --project src/Elehko.Dotkiln.Registry.Api
```

Configure the backing directory:

```powershell
$env:DOTKILN_REGISTRY_ROOT = "C:\stacks"
dotnet run --project src/Elehko.Dotkiln.Registry.Api
```

If `DOTKILN_REGISTRY_ROOT` is not set, the API uses a `stacks` directory relative to the API content root.

## API Endpoints

Health:

```http
GET /health
```

Response:

```json
{
  "status": "ok"
}
```

List stacks:

```http
GET /stacks
```

Response:

```json
[
  "aspnet-webapi-standard.dotkiln"
]
```

Search stacks:

```http
GET /stacks/search?term=webapi
```

Response:

```json
[
  "aspnet-webapi-standard.dotkiln.yaml"
]
```

Get one stack:

```http
GET /stacks/aspnet-webapi-standard
```

Response shape:

```json
{
  "Name": "aspnet-webapi-standard",
  "Description": "Opinionated baseline for a production ASP.NET Core minimal API",
  "TargetFramework": "net8.0",
  "Packages": [
    {
      "Id": "Serilog.AspNetCore",
      "Version": "8.0.*",
      "Group": "logging"
    }
  ],
  "SchemaVersion": "0.1",
  "Snippet": "setup/program-cs-additions.txt"
}
```

Publish stack:

```http
POST /stacks
Content-Type: text/plain

schemaVersion: "0.1"
name: custom-stack
description: Example stack
targetFramework: net8.0

packages:
  - id: Serilog.AspNetCore
    version: "8.0.*"
    group: logging
```

Success response:

```json
{
  "Name": "custom-stack"
}
```

Validation failure response:

```json
[
  "Package 'Serilog.AspNetCore' must declare a version."
]
```

## Recommended Workflow For Teams Today

For early team adoption:

1. Keep approved stack files in your repository under `stacks`.
2. Review stack changes through normal pull requests.
3. Use `dotkiln validate` in CI.
4. Use `dotkiln status` in CI for projects that must follow a stack.
5. Use the local registry commands only when you want to copy validated stack files into a shared folder.

This keeps the source of truth visible and versioned until the remote registry layer is production-ready.
