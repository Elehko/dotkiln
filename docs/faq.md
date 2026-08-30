# FAQ

## What is drift?

Drift is when a stack package is missing from the project or the installed package version does not satisfy the stack version expression.

Example:

```text
missing      Serilog.Sinks.Console (missing) -> 6.*
out-of-range Serilog.AspNetCore 7.0.0 -> 8.0.*
```

## What happens when a package is missing?

`status` reports it as drift. `apply` adds it to the project. `update` applies it only inside an isolated workspace.

Example:

```text
missing      Swashbuckle.AspNetCore (missing) -> 6.*
```

## Does Dotkiln automatically modify project files?

`apply` modifies `.csproj` files by running `dotnet add package`. `status` and `validate` do not modify project files. `update` modifies an isolated workspace, not the real project.

Use `apply --dry-run` to preview changes before modifying a project.

## Does Dotkiln report extra packages?

Yes. `status` reports direct project packages that are not declared in the stack under "Extra packages not in stack (informational)." They are not counted as drift and do not affect the exit code.

Example:

```text
Extra packages not in stack (informational):
  AutoMapper 13.0.1

No drift detected. 1 extra packages found (not flagged).
```

## Does Dotkiln remove extra packages?

No. Extra project packages are allowed and are not removed by `apply` or `update`.

## Can packages be ignored?

Yes, for extra package reporting. Put a `.dotkilnignore` file next to the `.csproj` and list one package ID per line.

Dotkiln does not yet support ignoring missing stack packages or version mismatches.

Example:

```text
# .dotkilnignore
AutoMapper
Polly
```

## Can I rollback updates?

The current `update` workflow runs in a disposable isolated workspace, so failed updates do not need rollback in the real project. For `apply`, use Git to review and revert changes.

## Why does apply or update stop on a dirty git working tree?

Dotkiln relies on git for rollback safety. A clean working tree gives you a clean revert point before package changes are made. Use `--force` to override this guard.

Example:

```text
Commit or stash your changes, or re-run with --force to proceed anyway
(not recommended).
```

## Can I use private NuGet feeds?

`dotnet add package` uses the NuGet configuration available to your .NET SDK. Dotkiln's current version resolver targets nuget.org, so private-feed resolution is not fully implemented.

## Are prerelease versions installed by default?

No. Wildcard and range expressions resolve stable versions by default. A stack must explicitly request a prerelease version to allow one.

## Does Dotkiln support Central Package Management?

Not yet. Dotkiln currently focuses on direct `PackageReference` entries in `.csproj` files.
