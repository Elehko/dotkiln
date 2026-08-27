using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Versions;
using Elehko.Dotkiln.Engine.NuGetResolution;
using Elehko.Dotkiln.Engine.Processes;
using Elehko.Dotkiln.Engine.ProjectFiles;

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
    /// Applies package changes with the .NET CLI.
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

    private async Task<string> ResolveVersionAsync(PackageEntry package, CancellationToken cancellationToken)
    {
        var resolver = versionResolver ?? new NuGetVersionResolver();
        return await resolver.ResolveLatestMatchingAsync(package, cancellationToken) ?? package.Version;
    }
}
