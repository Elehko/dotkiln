using System.Xml.Linq;

namespace Dotkiln.Engine.ProjectFiles;

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

    private static string ReadRequiredAttribute(XElement element, string name)
    {
        return element.Attribute(name)?.Value
            ?? throw new InvalidOperationException($"PackageReference is missing '{name}'.");
    }
}
