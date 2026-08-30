# FAQ

## What is drift?

Drift is when a stack package is missing from the project or the installed package version does not satisfy the stack version expression.

## What happens when a package is missing?

`status` reports it as drift. `apply` adds it to the project. `update` applies it only inside an isolated workspace.

## Does Dotkiln automatically modify project files?

`apply` modifies `.csproj` files by running `dotnet add package`. `status` and `validate` do not modify project files. `update` modifies an isolated workspace, not the real project.

## Does Dotkiln remove extra packages?

No. Extra project packages are allowed and are not removed by `apply` or `update`.

## Can packages be ignored?

Not yet. Ignore rules and suppression rules are not implemented.

## Can I rollback updates?

The current `update` workflow runs in a disposable isolated workspace, so failed updates do not need rollback in the real project. For `apply`, use Git to review and revert changes.

## Can I use private NuGet feeds?

`dotnet add package` uses the NuGet configuration available to your .NET SDK. Dotkiln's current version resolver targets nuget.org, so private-feed resolution is not fully implemented.

## Are prerelease versions installed by default?

No. Wildcard and range expressions resolve stable versions by default. A stack must explicitly request a prerelease version to allow one.

## Does Dotkiln support Central Package Management?

Not yet. Dotkiln currently focuses on direct `PackageReference` entries in `.csproj` files.
