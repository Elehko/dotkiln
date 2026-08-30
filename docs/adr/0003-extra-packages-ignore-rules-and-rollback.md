# ADR 0003: Extra Package Reporting, Ignore Rules, and Rollback Strategy

## Status

Accepted

## Context

Early feedback raised three related questions:

- how `status` should treat packages installed in a project but not declared by the stack
- whether users can suppress project-specific package noise
- whether Dotkiln should implement custom rollback or rely on git

## Decision

Extra packages are reported separately as informational output. They are not counted as drift and do not affect `status` exit codes.

Project-level ignore rules are implemented with a `.dotkilnignore` file placed next to the `.csproj`. Ignore rules suppress packages from the extra-package report.

Dotkiln does not implement a custom undo command. Mutating `apply` and `update` commands require a clean git working tree by default. Users can override this with `--force`.

## Consequences

`status` keeps a narrow meaning for drift: the stack's package promise is either met or not met. Extra packages remain visible for audit purposes without implying the project is wrong.

Project-specific exceptions stay beside the project instead of polluting shared stack files.

Rollback remains a standard git workflow. Dotkiln avoids competing with git and refuses to mutate files when there is no clean revert point, unless the user explicitly chooses `--force`.

## Example

```text
Stack: aspnet-webapi-standard
  api-docs     up to date
  ef-core      up to date
  logging      up to date
  validation   up to date

Extra packages not in stack (informational):
  AutoMapper 13.0.1
  Polly 8.2.0

No drift detected. 2 extra packages found (not flagged).
```
