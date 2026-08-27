namespace Elehko.Dotkiln.Engine.ProjectFiles;

/// <summary>
/// Finds project files under a directory tree.
/// </summary>
public sealed class ProjectDiscovery
{
    /// <summary>
    /// Finds project files recursively under a directory.
    /// </summary>
    public IReadOnlyList<string> FindProjects(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        if (File.Exists(rootDirectory) && rootDirectory.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return [Path.GetFullPath(rootDirectory)];
        }

        if (!Directory.Exists(rootDirectory))
        {
            return [];
        }

        return Directory.GetFiles(rootDirectory, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFullPath)
            .ToArray();
    }

    /// <summary>
    /// Returns whether the project name or path looks like a test project.
    /// </summary>
    public bool IsTestProject(string projectPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(projectPath);
        return fileName.Contains("Test", StringComparison.OrdinalIgnoreCase);
    }
}
