# ADR 0001: Stack Files Use YAML

## Status

Accepted

## Context

Stack definitions should be easy to read, diff, and write by hand.

## Decision

Dotkiln stack files use YAML with a small schema: stack metadata, direct package references, optional groups, and an optional snippet path.

## Consequences

The CLI can validate stack files before applying them. The parser currently supports Dotkiln's limited YAML subset and can later be replaced by a full YAML parser without changing the public model.
