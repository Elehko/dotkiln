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

Even though package changes happen in isolation, `update` still requires a clean git working tree by default. This keeps Dotkiln's rollback story consistent across mutating workflows and avoids running update checks on top of uncommitted local edits.

Use `--force` to override this guard.

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

Meaning: Dotkiln found the `ef-core` group and would test that group as one unit. Because this is a dry run, no temp workspace is created and no package restore is performed.

## Successful Verification

Example output:

```text
Planning updates (1 groups)...
  ef-core: Build and tests passed in isolation.
```

Meaning: Dotkiln applied the group inside a disposable workspace, then `dotnet build` and discovered tests passed there. The real project was not modified.

## Failure Behavior

If package application, build, or tests fail inside the isolated workspace:

- Dotkiln exits with code `1`
- the real branch is not modified
- a log file is written, such as `Dotkiln-update-ef-core.log`

Example output:

```text
Planning updates (1 groups)...
  ef-core: Verification failed. No changes made to your branch.
    See C:\repo\Dotkiln-update-ef-core.log
```

Meaning: the isolated update failed build or tests. Open the log to inspect compiler errors or test failures. The project you ran the command against has not been changed by `update`.

## Rollback

Rollback of the real branch is not needed for the current `update` workflow because changes are made in a disposable isolated workspace. If future versions promote successful changes back into the working tree or pull request branch, rollback behavior will be documented with that feature.
