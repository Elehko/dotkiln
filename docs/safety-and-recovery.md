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

## Undoing Apply

`apply` changes the target `.csproj` through `dotnet add package`. To undo, use normal source control:

```powershell
git diff
git restore path/to/project.csproj
```

If restore generated `obj` files, those are build artifacts and should usually be ignored by Git.

## Failed Updates

`update` applies package changes in an isolated workspace. If the isolated build or tests fail, the real project is not modified.

Failure logs are written as:

```text
Dotkiln-update-<group>.log
```

## Backup Recommendations

Use Dotkiln in a Git repository when possible. Commit or stash unrelated work before running mutating commands so package changes are easy to review.
