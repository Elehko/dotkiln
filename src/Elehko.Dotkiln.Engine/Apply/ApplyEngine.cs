using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Versions;
using Elehko.Dotkiln.Engine.NuGetResolution;
using Elehko.Dotkiln.Engine.Processes;
using Elehko.Dotkiln.Engine.ProjectFiles;
using System.Xml.Linq;

namespace Elehko.Dotkiln.Engine.Apply;

/// <summary>
/// Computes and applies stack changes to .NET project files.
/// </summary>
public sealed class ApplyEngine(
    CsprojInspector inspector,
    INuGetVersionResolver? versionResolver = null,
    IProcessRunner? processRunner = null)
{
    /// <summary>
    /// Creates an apply plan by comparing a stack with the project's direct package references.
    /// </summary>
    public ApplyPlan Plan(string projectPath, StackDefinition stack)
    {
        ArgumentNullException.ThrowIfNull(stack);

        var installed = inspector.GetInstalledPackages(projectPath)
            .ToDictionary(package => package.Id, StringComparer.OrdinalIgnoreCase);

        var missing = new List<PackageEntry>();
        var outOfRange = new List<PackageEntry>();

        foreach (var package in stack.Packages)
        {
            if (!installed.TryGetValue(package.Id, out var installedPackage))
            {
                missing.Add(package);
                continue;
            }

            if (!VersionMatcher.Matches(package.Version, installedPackage.Version))
            {
                outOfRange.Add(package);
            }
        }

        return new ApplyPlan(missing, outOfRange);
    }

    /// <summary>
    /// Applies package changes to the project.
    /// </summary>
    public async Task<ApplyResult> ApplyAsync(string projectPath, StackDefinition stack, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var resolvedProject = inspector.ResolveProjectPath(projectPath);
        var plan = Plan(resolvedProject, stack);
        var messages = new List<string>();

        if (!plan.HasChanges)
        {
            messages.Add("Project already matches stack.");
            return new ApplyResult(true, plan, messages);
        }

        var centralPackagesPath = inspector.FindNearestCentralPackagesFile(resolvedProject);
        if (centralPackagesPath is not null)
        {
            return await ApplyWithCentralPackageManagementAsync(resolvedProject, centralPackagesPath, plan, messages, dryRun, cancellationToken);
        }

        foreach (var package in plan.PackagesToApply)
        {
            if (dryRun)
            {
                var previewCommand = $"add \"{resolvedProject}\" package {package.Id} --version {package.Version}";
                messages.Add($"Would run: dotnet {previewCommand}");
                continue;
            }

            var version = await ResolveVersionAsync(package, cancellationToken);
            var command = $"add \"{resolvedProject}\" package {package.Id} --version {version}";
            var result = await (processRunner ?? new ProcessRunner()).RunAsync("dotnet", command, Path.GetDirectoryName(resolvedProject), cancellationToken);
            messages.Add(result.Output.Trim());
            if (result.ExitCode != 0)
            {
                return new ApplyResult(false, plan, messages);
            }
        }

        return new ApplyResult(true, plan, messages);
    }

    private async Task<ApplyResult> ApplyWithCentralPackageManagementAsync(
        string projectPath,
        string centralPackagesPath,
        ApplyPlan plan,
        List<string> messages,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var projectDocument = XDocument.Load(projectPath);
        var centralDocument = XDocument.Load(centralPackagesPath);
        var projectChanged = false;
        var centralChanged = false;

        foreach (var package in plan.PackagesToApply)
        {
            var version = dryRun ? package.Version : await ResolveVersionAsync(package, cancellationToken);
            var packageReference = FindPackageReference(projectDocument, package.Id);

            if (packageReference is null)
            {
                if (dryRun)
                {
                    messages.Add($"Would add PackageReference '{package.Id}' to {projectPath}.");
                }
                else
                {
                    AddPackageReference(projectDocument, package.Id);
                    projectChanged = true;
                    messages.Add($"Added PackageReference '{package.Id}' to {projectPath}.");
                }
            }

            packageReference ??= FindPackageReference(projectDocument, package.Id);
            if (packageReference is not null && TrySetLocalVersion(packageReference, version))
            {
                if (dryRun)
                {
                    messages.Add($"Would update local PackageReference version for '{package.Id}' to {version}.");
                }
                else
                {
                    projectChanged = true;
                    messages.Add($"Updated local PackageReference version for '{package.Id}' to {version}.");
                }
            }
            else if (dryRun)
            {
                messages.Add($"Would set central PackageVersion '{package.Id}' to {version} in {centralPackagesPath}.");
            }
            else
            {
                UpsertCentralPackageVersion(centralDocument, package.Id, version);
                centralChanged = true;
                messages.Add($"Set central PackageVersion '{package.Id}' to {version} in {centralPackagesPath}.");
            }
        }

        if (projectChanged)
        {
            projectDocument.Save(projectPath);
        }

        if (centralChanged)
        {
            centralDocument.Save(centralPackagesPath);
        }

        return new ApplyResult(true, plan, messages);
    }

    private async Task<string> ResolveVersionAsync(PackageEntry package, CancellationToken cancellationToken)
    {
        var resolver = versionResolver ?? new NuGetVersionResolver();
        return await resolver.ResolveLatestMatchingAsync(package, cancellationToken) ?? package.Version;
    }

    private static XElement? FindPackageReference(XDocument projectDocument, string packageId)
    {
        return projectDocument
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "PackageReference"
                && string.Equals(element.Attribute("Include")?.Value, packageId, StringComparison.OrdinalIgnoreCase));
    }

    private static void AddPackageReference(XDocument projectDocument, string packageId)
    {
        var projectNamespace = projectDocument.Root?.Name.Namespace ?? XNamespace.None;
        var itemGroup = projectDocument
            .Root?
            .Elements()
            .FirstOrDefault(element => element.Name.LocalName == "ItemGroup" && element.Elements().Any(child => child.Name.LocalName == "PackageReference"))
            ?? projectDocument.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "ItemGroup");

        if (itemGroup is null)
        {
            itemGroup = new XElement(projectNamespace + "ItemGroup");
            projectDocument.Root?.Add(itemGroup);
        }

        itemGroup.Add(new XElement(projectNamespace + "PackageReference", new XAttribute("Include", packageId)));
    }

    private static bool TrySetLocalVersion(XElement packageReference, string version)
    {
        var versionAttribute = packageReference.Attribute("Version");
        if (versionAttribute is not null)
        {
            versionAttribute.Value = version;
            return true;
        }

        var versionOverrideAttribute = packageReference.Attribute("VersionOverride");
        if (versionOverrideAttribute is not null)
        {
            versionOverrideAttribute.Value = version;
            return true;
        }

        var versionElement = packageReference.Elements().FirstOrDefault(element => element.Name.LocalName == "Version");
        if (versionElement is not null)
        {
            versionElement.Value = version;
            return true;
        }

        var versionOverrideElement = packageReference.Elements().FirstOrDefault(element => element.Name.LocalName == "VersionOverride");
        if (versionOverrideElement is not null)
        {
            versionOverrideElement.Value = version;
            return true;
        }

        return false;
    }

    private static void UpsertCentralPackageVersion(XDocument centralDocument, string packageId, string version)
    {
        var centralNamespace = centralDocument.Root?.Name.Namespace ?? XNamespace.None;
        var existing = centralDocument
            .Descendants()
            .FirstOrDefault(element =>
                element.Name.LocalName == "PackageVersion"
                && string.Equals(
                    element.Attribute("Include")?.Value ?? element.Attribute("Update")?.Value,
                    packageId,
                    StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            SetVersion(existing, version);
            return;
        }

        var itemGroup = centralDocument
            .Root?
            .Elements()
            .FirstOrDefault(element => element.Name.LocalName == "ItemGroup" && element.Elements().Any(child => child.Name.LocalName == "PackageVersion"))
            ?? centralDocument.Root?.Elements().FirstOrDefault(element => element.Name.LocalName == "ItemGroup");

        if (itemGroup is null)
        {
            itemGroup = new XElement(centralNamespace + "ItemGroup");
            centralDocument.Root?.Add(itemGroup);
        }

        itemGroup.Add(new XElement(centralNamespace + "PackageVersion", new XAttribute("Include", packageId), new XAttribute("Version", version)));
    }

    private static void SetVersion(XElement element, string version)
    {
        var versionAttribute = element.Attribute("Version");
        if (versionAttribute is not null)
        {
            versionAttribute.Value = version;
            return;
        }

        var versionElement = element.Elements().FirstOrDefault(child => child.Name.LocalName == "Version");
        if (versionElement is not null)
        {
            versionElement.Value = version;
            return;
        }

        element.SetAttributeValue("Version", version);
    }
}
