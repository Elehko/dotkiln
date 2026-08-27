# ADR 0002: Updates Run In Isolation

## Status

Accepted

## Context

Dotkiln's main safety promise is that a failed package update should not alter the user's active branch.

## Decision

The update workflow applies package changes in an isolated workspace. The default implementation uses a temporary copy that excludes `.git`, `.idea`, `bin`, and `obj`. A git-worktree isolator is also available for workflows that want branch-based isolation.

## Consequences

The real project is not touched during update verification. Successful updates can later be promoted into reviewable diffs or pull requests.
