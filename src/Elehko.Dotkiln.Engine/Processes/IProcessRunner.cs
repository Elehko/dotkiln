namespace Elehko.Dotkiln.Engine.Processes;

/// <summary>
/// Runs external commands.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a process and captures output.
    /// </summary>
    Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default);
}
