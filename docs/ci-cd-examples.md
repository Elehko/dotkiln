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

## Update Planning In CI

Use `--dry-run` for visibility without package changes:

```yaml
- name: Preview Dotkiln updates
  run: dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml samples/TestApp/TestApp.csproj --dry-run
```

## Other CI Systems

Azure DevOps, Jenkins, and TeamCity can use the same command sequence:

```powershell
dotnet restore Dotkiln.sln
dotnet build Dotkiln.sln --configuration Release --no-restore
dotnet test Dotkiln.sln --configuration Release --no-build
dotnet run --project src/Elehko.Dotkiln.Cli -- validate stacks/aspnet-webapi-standard.dotkiln.yaml
```
