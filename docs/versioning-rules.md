# Versioning Rules

Dotkiln supports a small set of version expressions for stack package entries.

## Exact Versions

```yaml
version: "8.0.30"
```

Exact versions require the project package reference to match exactly.

## Wildcards

```yaml
version: "8.0.*"
```

```yaml
version: "8.*"
```

Wildcards match versions with the same prefix. When applying a package, Dotkiln resolves the latest stable NuGet version that matches the wildcard.

## Ranges

```yaml
version: "[8.0.0,9.0.0)"
```

Basic NuGet-style ranges are supported:

- `[` means inclusive lower bound
- `(` means exclusive lower bound
- `]` means inclusive upper bound
- `)` means exclusive upper bound

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

## Floating Versions

NuGet floating version expressions beyond Dotkiln's wildcard support are not fully implemented yet. Prefer exact versions, `8.0.*`-style wildcards, or explicit ranges.
