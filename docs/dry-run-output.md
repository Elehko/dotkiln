# Dry-run Output

`--dry-run` previews what a mutating command would do without changing project files.

Dry-run is supported by:

- `new`
- `apply`
- `update`
- `registry publish`

`status`, `validate`, and `registry search` are read-only and do not need `--dry-run`.

## apply --dry-run

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- apply stacks/aspnet-webapi-standard.dotkiln.yaml MyApp.csproj --dry-run
```

Example output:

```text
Would run: dotnet add "C:\repo\MyApp\MyApp.csproj" package Serilog.Sinks.Console --version 6.*
Would run: dotnet add "C:\repo\MyApp\MyApp.csproj" package FluentValidation.AspNetCore --version 11.*
Would run: dotnet add "C:\repo\MyApp\MyApp.csproj" package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.*
```

Meaning: Dotkiln has detected missing or out-of-range packages and is showing the `dotnet add package` commands it would run.

If the project already matches the stack:

```text
Project already matches stack.
```

## update --dry-run

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- update stacks/aspnet-webapi-standard.dotkiln.yaml MyApp.csproj --group ef-core --dry-run
```

Example output:

```text
Planning updates (1 groups)...
  ef-core: Would update 2 packages in isolation.
```

Meaning: Dotkiln found one update group and would run that group through the isolated update workflow. Dry-run does not create the isolated workspace, restore packages, build, or test.

## new --dry-run

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- new stacks/console-tool-standard.dotkiln.yaml WorkerTool --template console --dry-run
```

Example output:

```text
Would run: dotnet new console -n WorkerTool
```

Meaning: Dotkiln would create a new project using the `console` template, then apply the stack. Current dry-run output shows the project creation command.

## registry publish --dry-run

Command:

```powershell
dotnet run --project src/Elehko.Dotkiln.Cli -- registry publish path/to/custom.dotkiln.yaml --registry-dir stacks --dry-run
```

Example output:

```text
Would publish custom-stack to stacks\custom-stack.dotkiln.yaml
```

Meaning: Dotkiln validated the stack and would copy it into the registry directory using the stack name as the file name.

## Dirty Working Tree

Dry-run commands do not require a clean git working tree because they do not mutate project files.

Non-dry-run `apply` and `update` require a clean git working tree unless `--force` is supplied.
