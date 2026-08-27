using System.Diagnostics;

namespace Dotkiln.Updates.Verification;

/// <summary>
/// Runs dotnet build and, when possible, dotnet test for an isolated project checkout.
/// </summary>
public sealed class BuildAndTestVerifier
{
    /// <summary>
    /// Verifies a project or solution path by running the .NET SDK build command.
    /// </summary>
    public async Task<VerificationResult> VerifyAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var output = await RunAsync("dotnet", $"build \"{path}\" --nologo", cancellationToken);
        return new VerificationResult(output.ExitCode == 0, output.Output);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static async Task<(int ExitCode, string Output)> RunAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Unable to start {fileName}.");
        var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return (process.ExitCode, string.Concat(await standardOutput, await standardError));
    }
}
