using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Engine.ProjectFiles;

namespace Elehko.Dotkiln.Engine.Apply;

/// <summary>
/// Computes and applies stack changes to .NET project files.
/// </summary>
public sealed class ApplyEngine(CsprojInspector inspector)
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

            if (!VersionPatternMatches(package.Version, installedPackage.Version))
            {
                outOfRange.Add(package);
            }
        }

        return new ApplyPlan(missing, outOfRange);
    }

    private static bool VersionPatternMatches(string requested, string installed)
    {
        if (string.Equals(requested, installed, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (requested.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = requested[..^1];
            return installed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
