using Elehko.Dotkiln.Engine.Processes;

namespace Elehko.Dotkiln.Engine.Git;

/// <summary>
/// Checks whether a git working tree is clean before mutating commands run.
/// </summary>
public sealed class GitWorkingTreeGuard(IProcessRunner processRunner)
{
    /// <summary>
    /// Ensures the repository containing the path has no uncommitted changes.
    /// </summary>
    public async Task EnsureCleanAsync(string path, bool force, CancellationToken cancellationToken = default)
    {
        if (force)
        {
            return;
        }

        var workingDirectory = Directory.Exists(path)
            ? Path.GetFullPath(path)
            : Path.GetDirectoryName(Path.GetFullPath(path)) ?? Environment.CurrentDirectory;

        var repositoryCheck = await processRunner.RunAsync("git", "rev-parse --is-inside-work-tree", workingDirectory, cancellationToken);
        if (repositoryCheck.ExitCode != 0)
        {
            return;
        }

        var status = await processRunner.RunAsync("git", "status --porcelain", workingDirectory, cancellationToken);
        if (status.ExitCode != 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(status.StandardOutput))
        {
            throw new DirtyWorkingTreeException("""
                The git working tree has uncommitted changes.
                Dotkiln relies on git for rollback safety and will not modify files
                while uncommitted changes are present.

                Commit or stash your changes, or re-run with --force to proceed anyway
                (not recommended).
                """);
        }
    }
}
