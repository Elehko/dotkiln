using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Versions;
using Elehko.Dotkiln.Engine.Ignore;
using Elehko.Dotkiln.Engine.ProjectFiles;

namespace Elehko.Dotkiln.Engine.Status;

/// <summary>
/// Computes grouped project drift against a stack.
/// </summary>
public sealed class StatusEngine(CsprojInspector inspector, DotkilnIgnore? ignoreRules = null)
{
    /// <summary>
    /// Computes project status for a stack.
    /// </summary>
    public StackStatus GetStatus(string projectPath, StackDefinition stack)
    {
        var resolvedProject = inspector.ResolveProjectPath(projectPath);
        var installedPackages = inspector.GetInstalledPackages(resolvedProject);
        var installed = installedPackages.ToDictionary(package => package.Id, StringComparer.OrdinalIgnoreCase);
        var stackPackageIds = stack.Packages
            .Select(package => package.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ignoredPackageIds = (ignoreRules ?? new DotkilnIgnore()).LoadForProject(resolvedProject);

        var packages = stack.Packages.Select(package =>
        {
            var group = string.IsNullOrWhiteSpace(package.Group) ? package.Id : package.Group;
            if (!installed.TryGetValue(package.Id, out var installedPackage))
            {
                return new PackageDrift(package.Id, package.Version, null, group, "missing");
            }

            var state = VersionMatcher.Matches(package.Version, installedPackage.Version) ? "up-to-date" : "out-of-range";
            return new PackageDrift(package.Id, package.Version, installedPackage.Version, group, state);
        });

        var groups = packages
            .GroupBy(package => package.Group)
            .Select(group => new GroupDrift(group.Key, group.ToArray()))
            .OrderBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var extraPackages = installedPackages
            .Where(package => !stackPackageIds.Contains(package.Id))
            .Where(package => !ignoredPackageIds.Contains(package.Id))
            .Select(package => new ExtraPackage(package.Id, package.Version))
            .OrderBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new StackStatus(stack.Name, groups, extraPackages);
    }
}
