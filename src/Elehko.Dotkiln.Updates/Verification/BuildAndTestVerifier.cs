using Elehko.Dotkiln.Engine.Processes;
using Elehko.Dotkiln.Engine.ProjectFiles;

namespace Elehko.Dotkiln.Updates.Verification;

/// <summary>
/// Runs dotnet build and, when possible, dotnet test for an isolated project checkout.
/// </summary>
public sealed class BuildAndTestVerifier(IProcessRunner? processRunner = null, ProjectDiscovery? projectDiscovery = null)
{
    /// <summary>
    /// Verifies a project, solution, or directory by running build and discovered tests.
    /// </summary>
    public async Task<VerificationResult> VerifyAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var runner = processRunner ?? new ProcessRunner();
        var discovery = projectDiscovery ?? new ProjectDiscovery();
        var build = await runner.RunAsync("dotnet", $"build \"{path}\" --nologo", Directory.Exists(path) ? path : Path.GetDirectoryName(path), cancellationToken);
        if (build.ExitCode != 0)
        {
            return new VerificationResult(false, build.Output, TestsWereRun: false);
        }

        var root = Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
        var testProjects = discovery.FindProjects(root).Where(discovery.IsTestProject).ToArray();
        if (testProjects.Length == 0)
        {
            return new VerificationResult(true, build.Output, TestsWereRun: false);
        }

        var outputs = new List<string> { build.Output };
        foreach (var testProject in testProjects)
        {
            var test = await runner.RunAsync("dotnet", $"test \"{testProject}\" --no-build --nologo", Path.GetDirectoryName(testProject), cancellationToken);
            outputs.Add(test.Output);
            if (test.ExitCode != 0)
            {
                return new VerificationResult(false, string.Join(Environment.NewLine, outputs), TestsWereRun: true);
            }
        }

        return new VerificationResult(true, string.Join(Environment.NewLine, outputs), TestsWereRun: true);
    }
}
