namespace Elehko.Dotkiln.Updates.Isolation;

/// <summary>
/// Creates an isolated update workspace by copying the repository or project directory to temp storage.
/// </summary>
public sealed class TempCopyIsolator : IProjectIsolator
{
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".idea",
        "bin",
        "obj"
    };

    /// <inheritdoc />
    public Task<ProjectIsolation> IsolateAsync(string projectPath, string groupName, CancellationToken cancellationToken = default)
    {
        var projectFullPath = Path.GetFullPath(projectPath);
        var sourceRoot = FindRepositoryRoot(projectFullPath) ?? Path.GetDirectoryName(projectFullPath)
            ?? throw new InvalidOperationException("Unable to resolve project directory.");

        var tempRoot = Path.Combine(Path.GetTempPath(), $"dotkiln-{Sanitize(groupName)}-{Guid.NewGuid():N}");
        CopyDirectory(sourceRoot, tempRoot, cancellationToken);

        var relativeProject = Path.GetRelativePath(sourceRoot, projectFullPath);
        return Task.FromResult(new ProjectIsolation(tempRoot, Path.Combine(tempRoot, relativeProject), IsDisposable: true));
    }

    private static void CopyDirectory(string source, string destination, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destination);

        foreach (var directory in Directory.GetDirectories(source))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = Path.GetFileName(directory);
            if (ExcludedDirectories.Contains(name))
            {
                continue;
            }

            CopyDirectory(directory, Path.Combine(destination, name), cancellationToken);
        }

        foreach (var file in Directory.GetFiles(source))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: true);
        }
    }

    private static string? FindRepositoryRoot(string path)
    {
        var directory = File.Exists(path) ? Directory.GetParent(path) : new DirectoryInfo(path);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string Sanitize(string value)
    {
        return string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
    }
}
