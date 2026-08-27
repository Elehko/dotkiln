using Elehko.Dotkiln.Updates.Planning;

namespace Elehko.Dotkiln.Updates.Execution;

/// <summary>
/// Represents the result of one grouped update run.
/// </summary>
public sealed record UpdateRunResult(UpdateGroup Group, bool Succeeded, string Message, string? LogPath = null);
