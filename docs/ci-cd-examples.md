# CI/CD Examples

Dotkiln can be used in CI to validate stacks and detect dependency drift.

## GitHub Actions

Validate stack files and build the solution:

```yaml
name: CI

on:
  pull_request:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x

      - run: dotnet restore Dotkiln.sln
      - run: dotnet build Dotkiln.sln --configuration Release --no-restore --nologo
      - run: dotnet test Dotkiln.sln --configuration Release --no-build --nologo
      - run: dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```

## Drift Check In Pull Requests

Example:

```yaml
- name: Check dependency drift
  run: dotnet run --project src/Elehko.Dotkiln.Cli -- status stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj
```

`status` exits with code `1` when drift is found, so this can block a pull request.

Example passing output:

```text
Stack: aspnet-webapi-standard
  api-docs     up to date
  ef-core      up to date
  logging      up to date
  validation   up to date

No drift detected. No extra packages to report.
```

Example failing output:

```text
Stack: aspnet-webapi-standard
  logging      drift detected
    out-of-range Serilog.AspNetCore 7.0.0 -> 8.0.*

Drift detected. No extra packages to report.
```

## Update Planning In CI

Use `--dry-run` for visibility without package changes:

```yaml
- name: Preview Dotkiln updates
  run: dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --dry-run
```

Example output:

```text
Planning updates (4 groups)...
  api-docs: Would update 1 packages in isolation.
  ef-core: Would update 2 packages in isolation.
  logging: Would update 2 packages in isolation.
  validation: Would update 1 packages in isolation.
```

## Other CI Systems

Azure DevOps, Jenkins, and TeamCity can use the same command sequence:

```powershell
dotnet restore Dotkiln.sln
dotnet build Dotkiln.sln --configuration Release --no-restore
dotnet test Dotkiln.sln --configuration Release --no-build
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```
