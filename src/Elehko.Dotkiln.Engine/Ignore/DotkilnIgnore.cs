namespace Elehko.Dotkiln.Engine.Ignore;

/// <summary>
/// Reads project-level Dotkiln ignore rules.
/// </summary>
public sealed class DotkilnIgnore
{
    /// <summary>
    /// Loads ignored package IDs from a .dotkilnignore file next to the project file.
    /// </summary>
    public IReadOnlySet<string> LoadForProject(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectPath)) ?? Environment.CurrentDirectory;
        var ignorePath = Path.Combine(projectDirectory, ".dotkilnignore");
        if (!File.Exists(ignorePath))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var ignored = File.ReadAllLines(ignorePath)
            .Select(StripComment)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return ignored;
    }

    private static string StripComment(string line)
    {
        var commentIndex = line.IndexOf('#');
        return commentIndex < 0 ? line : line[..commentIndex];
    }
}
