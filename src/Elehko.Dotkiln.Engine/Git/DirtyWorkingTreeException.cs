namespace Elehko.Dotkiln.Engine.Git;

/// <summary>
/// Raised when a mutating command is blocked by uncommitted git changes.
/// </summary>
public sealed class DirtyWorkingTreeException(string message) : Exception(message);
