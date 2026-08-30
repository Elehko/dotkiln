# Drift Detection

Drift is the difference between a project's direct `PackageReference` entries and the package requirements declared by a Dotkiln stack.

## What Counts As Drift

Dotkiln currently reports drift when:

- a package exists in the stack but is missing from the project
- a package exists in the stack but the installed version does not satisfy the stack version expression

Dotkiln currently does not report drift when:

- a project contains extra packages that are not listed in the stack
- the project target framework differs from the stack `targetFramework`
- a package would belong to a different group, because group membership exists only in the stack file, not in `.csproj`

## Missing Packages

If a stack package is missing from the project, `status` reports it as `missing` and exits with code `1`.

Missing packages are not added by `status`. They are added by `apply`, or by `update` inside an isolated workspace.

Example:

```text
Stack: console-tool-standard
  hosting      drift detected
    missing      Microsoft.Extensions.Hosting (missing) -> 8.0.*
```

## Version Drift

If a direct package reference is present but its version does not satisfy the stack version expression, `status` reports it as `out-of-range` and exits with code `1`.

Example:

```text
Stack: aspnet-webapi-standard
  logging      drift detected
    out-of-range Serilog.AspNetCore 7.0.0 -> 8.0.*
```

## Extra Packages

Extra project packages are currently allowed and ignored by drift detection. Dotkiln does not remove extra packages during `apply` or `update`.

This is intentional for the current implementation: a stack defines a required baseline, not an exclusive allow-list.

## Remediation

Use `apply` to add missing packages or update out-of-range package versions:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj
```

Use `--dry-run` first to preview changes:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```
