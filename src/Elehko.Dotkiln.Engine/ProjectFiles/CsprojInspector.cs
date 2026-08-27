using System.Xml.Linq;

namespace Elehko.Dotkiln.Engine.ProjectFiles;

/// <summary>
/// Reads direct package references from SDK-style project files.
/// </summary>
public sealed class CsprojInspector
{
    /// <summary>
    /// Returns direct PackageReference entries from the supplied project file.
    /// </summary>
    public IReadOnlyList<InstalledPackage> GetInstalledPackages(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var document = XDocument.Load(projectPath);
        return document
            .Descendants("PackageReference")
            .Select(reference => new InstalledPackage(
                ReadRequiredAttribute(reference, "Include"),
                reference.Attribute("Version")?.Value ?? string.Empty))
            .ToArray();
    }

    /// <summary>
    /// Finds a project file from a file or directory path.
    /// </summary>
    public string ResolveProjectPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFullPath(path);
        }

        if (!Directory.Exists(path))
        {
            throw new FileNotFoundException($"Project path '{path}' was not found.", path);
        }

        var projects = Directory.GetFiles(path, "*.csproj", SearchOption.TopDirectoryOnly);
        return projects.Length switch
        {
            1 => Path.GetFullPath(projects[0]),
            0 => throw new FileNotFoundException($"No .csproj file was found in '{path}'.", path),
            _ => throw new InvalidOperationException($"Multiple .csproj files were found in '{path}'. Pass one explicitly.")
        };
    }

    private static string ReadRequiredAttribute(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"PackageReference is missing '{name}'.");
    }
}
