using Elehko.Dotkiln.Engine.Processes;

namespace Elehko.Dotkiln.Updates.Isolation;

/// <summary>
/// Creates an isolated update workspace using git worktree.
/// </summary>
public sealed class GitWorktreeIsolator(IProcessRunner processRunner) : IProjectIsolator
{
    /// <inheritdoc />
    public async Task<ProjectIsolation> IsolateAsync(string projectPath, string groupName, CancellationToken cancellationToken = default)
    {
        var projectFullPath = Path.GetFullPath(projectPath);
        var repositoryRoot = await GetRepositoryRootAsync(projectFullPath, cancellationToken);
        var worktree = Path.Combine(Path.GetTempPath(), $"dotkiln-worktree-{Sanitize(groupName)}-{Guid.NewGuid():N}");
        var branch = $"dotkiln/update-{Sanitize(groupName)}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var result = await processRunner.RunAsync("git", $"worktree add -b {branch} \"{worktree}\"", repositoryRoot, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(result.Output);
        }

        var relativeProject = Path.GetRelativePath(repositoryRoot, projectFullPath);
        return new ProjectIsolation(worktree, Path.Combine(worktree, relativeProject), IsDisposable: true);
    }

    private async Task<string> GetRepositoryRootAsync(string projectPath, CancellationToken cancellationToken)
    {
        var workingDirectory = Path.GetDirectoryName(projectPath) ?? Environment.CurrentDirectory;
        var result = await processRunner.RunAsync("git", "rev-parse --show-toplevel", workingDirectory, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("The project is not inside a git repository.");
        }

        return result.StandardOutput.Trim();
    }

    private static string Sanitize(string value)
    {
        return string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
    }
}
