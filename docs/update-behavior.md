# Update Behavior

`update` is the safe grouped update workflow.

## Lifecycle

Dotkiln currently runs this lifecycle:

1. load and validate the stack
2. group packages by the stack `group` field
3. optionally filter to one group with `--group`
4. create an isolated workspace using a temporary copy
5. apply the group stack inside the isolated workspace
6. run `dotnet build`
7. run discovered test projects when present
8. report success or write a failure log

## Does Update Modify The Real Project?

The real project is not modified by `update`. Package changes are applied inside the isolated workspace.

The current implementation reports verification results. It does not yet promote successful isolated changes back into the real branch or open a pull request.

## Dry Run

Use `--dry-run` to show grouped update intent without creating an isolated workspace:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml path/to/project.csproj --group ef-core --dry-run
```

Example output:

```text
Planning updates (1 groups)...
  ef-core: Would update 2 packages in isolation.
```

## Failure Behavior

If package application, build, or tests fail inside the isolated workspace:

- Dotkiln exits with code `1`
- the real branch is not modified
- a log file is written, such as `Dotkiln-update-ef-core.log`

## Rollback

Rollback of the real branch is not needed for the current `update` workflow because changes are made in a disposable isolated workspace. If future versions promote successful changes back into the working tree or pull request branch, rollback behavior will be documented with that feature.
