# Versioning Rules

Dotkiln supports a small set of version expressions for stack package entries.

## Exact Versions

```yaml
version: "8.0.30"
```

Exact versions require the project package reference to match exactly.

Example:

```text
Requested: 8.0.30
Installed: 8.0.30
Result: up to date
```

```text
Requested: 8.0.30
Installed: 8.0.29
Result: out-of-range
```

## Wildcards

```yaml
version: "8.0.*"
```

```yaml
version: "8.*"
```

Wildcards match versions with the same prefix. When applying a package, Dotkiln resolves the latest stable NuGet version that matches the wildcard.

Examples:

```text
Requested: 8.0.*
Installed: 8.0.30
Result: up to date
```

```text
Requested: 8.0.*
Installed: 8.1.0
Result: out-of-range
```

## Ranges

```yaml
version: "[8.0.0,9.0.0)"
```

Basic NuGet-style ranges are supported:

- `[` means inclusive lower bound
- `(` means exclusive lower bound
- `]` means inclusive upper bound
- `)` means exclusive upper bound

Examples:

```text
Requested: [8.0.0,9.0.0)
Installed: 8.0.30
Result: up to date
```

```text
Requested: [8.0.0,9.0.0)
Installed: 9.0.0
Result: out-of-range
```

## Prerelease Versions

Prerelease versions are excluded by default for wildcard and range expressions.

For example, this does not match `8.0.3-dev-00346`:

```yaml
version: "8.0.*"
```

To allow a prerelease, the stack must explicitly include a prerelease label:

```yaml
version: "8.0.3-preview.1"
```

This prevents a wildcard such as `8.0.*` from silently selecting nightly or development builds.

## Floating Versions

NuGet floating version expressions beyond Dotkiln's wildcard support are not fully implemented yet. Prefer exact versions, `8.0.*`-style wildcards, or explicit ranges.
