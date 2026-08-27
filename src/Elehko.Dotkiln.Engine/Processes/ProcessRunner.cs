using System.Diagnostics;

namespace Elehko.Dotkiln.Engine.Processes;

/// <summary>
/// Default process runner backed by <see cref="Process" />.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(string fileName, string arguments, string? workingDirectory = null, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Unable to start '{fileName}'.");
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(process.ExitCode, await output, await error);
    }
}
