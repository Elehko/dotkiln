# Ignore and Exclusion Rules

Dotkiln does not currently implement ignore or exclusion rules.

## Current Behavior

- Specific packages cannot yet be ignored through stack configuration.
- Version mismatches cannot yet be suppressed.
- Project-level overrides are not yet supported.
- Temporary suppressions are not yet supported.
- Stack authors cannot yet define an `ignore` section.

## Extra Packages

Packages that exist in a project but are not declared in the stack are implicitly allowed today. They are not reported as drift and are not removed by `apply` or `update`.

## Proposed Future Shape

A future stack schema may support explicit ignore rules, for example:

```yaml
ignore:
  - Some.Experimental.Package
  - Legacy.Dependency
```

That syntax is not valid in the current implementation and should not be used yet.
