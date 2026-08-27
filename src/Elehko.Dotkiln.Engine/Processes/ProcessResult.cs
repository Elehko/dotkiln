namespace Elehko.Dotkiln.Engine.Processes;

/// <summary>
/// Represents a completed process invocation.
/// </summary>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>
    /// Gets combined process output.
    /// </summary>
    public string Output => string.Concat(StandardOutput, StandardError);
}
