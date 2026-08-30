# Safety and Recovery

Dotkiln is designed to make package changes visible and reversible.

## Preview Changes

Use `--dry-run` before mutating commands:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --dry-run
```

Example `apply --dry-run` output:

```text
Would run: dotnet add "C:\repo\MyApp\MyApp.csproj" package Serilog.AspNetCore --version 8.0.*
Would run: dotnet add "C:\repo\MyApp\MyApp.csproj" package Swashbuckle.AspNetCore --version 6.*
```

Example `update --dry-run` output:

```text
Planning updates (2 groups)...
  logging: Would update 2 packages in isolation.
  api-docs: Would update 1 packages in isolation.
```

Dry-run output is intentionally command-like. It should tell you what Dotkiln intends to do before any project file is touched.

## Undoing Apply

`apply` changes the target `.csproj` through `dotnet add package`. To undo, use normal source control:

```powershell
git diff
git restore path/to/project.csproj
```

If restore generated `obj` files, those are build artifacts and should usually be ignored by Git.

Example review after `apply`:

```powershell
git diff MyApp.csproj
```

Example diff:

```diff
+ <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
+ <PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
```

Meaning: these are normal project-file changes. Keep them by committing, or discard them with git if they are not wanted.

## Clean Working Tree Requirement

`apply` and `update` require a clean git working tree by default.

If uncommitted changes are present, Dotkiln stops with:

```text
The git working tree has uncommitted changes.
Dotkiln relies on git for rollback safety and will not modify files
while uncommitted changes are present.

Commit or stash your changes, or re-run with --force to proceed anyway
(not recommended).
```

Recommended workflow:

```powershell
git status
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml MyApp.csproj
git diff
```

If you do not want the result:

```powershell
git checkout -- MyApp.csproj
```

If the change was already committed:

```powershell
git revert <commit-sha>
```

## Failed Updates

`update` applies package changes in an isolated workspace. If the isolated build or tests fail, the real project is not modified.

Failure logs are written as:

```text
Dotkiln-update-<group>.log
```

Example:

```text
Planning updates (1 groups)...
  ef-core: Verification failed. No changes made to your branch.
    See C:\repo\Dotkiln-update-ef-core.log
```

Meaning: build or tests failed in the isolated workspace. The log contains the command output needed to debug the update.

## Backup Recommendations

Use Dotkiln in a Git repository when possible. Commit or stash unrelated work before running mutating commands so package changes are easy to review.
