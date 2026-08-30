# Ignore and Exclusion Rules

Dotkiln supports project-level ignore rules through a `.dotkilnignore` file.

Place `.dotkilnignore` next to the `.csproj` file.

## Syntax

Use one package ID per line. Blank lines are ignored. Comments start with `#`.

```text
# .dotkilnignore
# These packages are project-specific and should not appear
# in the extra-package report.

AutoMapper
Polly
```

Package matching is case-insensitive.

## Current Behavior

`.dotkilnignore` suppresses packages from the informational "extra packages not in stack" section of `status`.

It does not currently:

- ignore missing stack packages
- ignore version mismatches for stack packages
- define temporary suppressions
- support stack-level ignore rules inside `.dotkiln.yaml`

## Extra Packages

Packages that exist in a project but are not declared in the stack are allowed. They are reported as informational extras unless listed in `.dotkilnignore`.

Without `.dotkilnignore`, a project with `AutoMapper` and `Polly` installed outside the stack shows:

```text
Extra packages not in stack (informational):
  AutoMapper 13.0.1
  Polly 8.2.0

No drift detected. 2 extra packages found (not flagged).
```

After adding this file beside `MyApp.csproj`:

```text
# .dotkilnignore
AutoMapper
Polly
```

The same `status` command shows:

```text
No drift detected. No extra packages to report.
```

Meaning: `.dotkilnignore` does not uninstall packages and does not change the stack. It only hides known project-specific extras from the informational report.

## Case-insensitive Matching

These entries are equivalent:

```text
AutoMapper
automapper
AUTOMAPPER
```

Use the normal NuGet package casing when possible so the file remains easy to read.

## Proposed Future Shape

A future stack schema may support explicit ignore rules, for example:

```yaml
ignore:
  - Some.Experimental.Package
  - Legacy.Dependency
```

That syntax is not valid in the current implementation and should not be used yet.
