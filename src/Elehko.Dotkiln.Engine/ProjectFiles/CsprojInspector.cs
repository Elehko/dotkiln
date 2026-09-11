using System.Xml.Linq;

namespace Elehko.Dotkiln.Engine.ProjectFiles;

/// <summary>
/// Reads direct package references from SDK-style project files.
/// </summary>
public sealed class CsprojInspector
{
    private const string CentralPackagesFileName = "Directory.Packages.props";

    /// <summary>
    /// Returns direct PackageReference entries from the supplied project file.
    /// </summary>
    public IReadOnlyList<InstalledPackage> GetInstalledPackages(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        var resolvedProjectPath = Path.GetFullPath(projectPath);
        var centralPackageVersions = LoadCentralPackageVersions(resolvedProjectPath);
        var document = XDocument.Load(resolvedProjectPath);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(reference => new InstalledPackage(
                ReadRequiredAttribute(reference, "Include"),
                ReadPackageVersion(reference, centralPackageVersions)))
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

    private static string ReadPackageVersion(XElement packageReference, IReadOnlyDictionary<string, string> centralPackageVersions)
    {
        var localVersion = ReadAttributeOrElement(packageReference, "Version");
        if (!string.IsNullOrWhiteSpace(localVersion))
        {
            return localVersion;
        }

        var versionOverride = ReadAttributeOrElement(packageReference, "VersionOverride");
        if (!string.IsNullOrWhiteSpace(versionOverride))
        {
            return versionOverride;
        }

        var packageId = ReadRequiredAttribute(packageReference, "Include");
        return centralPackageVersions.TryGetValue(packageId, out var centralVersion)
            ? centralVersion
            : string.Empty;
    }

    private static string? ReadAttributeOrElement(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? element.Elements().FirstOrDefault(child => child.Name.LocalName == name)?.Value;
    }

    private static IReadOnlyDictionary<string, string> LoadCentralPackageVersions(string projectPath)
    {
        var centralPackagesPath = FindNearestCentralPackagesFilePath(projectPath);
        if (centralPackagesPath is null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var document = XDocument.Load(centralPackagesPath);
        var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var packageVersion in document.Descendants().Where(element => element.Name.LocalName == "PackageVersion"))
        {
            var packageId = packageVersion.Attribute("Include")?.Value
                ?? packageVersion.Attribute("Update")?.Value;
            var version = ReadAttributeOrElement(packageVersion, "Version");

            if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(version))
            {
                versions[packageId] = version;
            }
        }

        return versions;
    }

    /// <summary>
    /// Finds the nearest Directory.Packages.props file by walking up from the project directory.
    /// </summary>
    public string? FindNearestCentralPackagesFile(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        return FindNearestCentralPackagesFilePath(Path.GetFullPath(projectPath));
    }

    private static string? FindNearestCentralPackagesFilePath(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath);
        if (projectDirectory is null)
        {
            return null;
        }

        for (var directory = new DirectoryInfo(projectDirectory); directory is not null; directory = directory.Parent)
        {
            var centralPackagesPath = Path.Combine(directory.FullName, CentralPackagesFileName);
            if (File.Exists(centralPackagesPath))
            {
                return centralPackagesPath;
            }
        }

        return null;
    }
}
