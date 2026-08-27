using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Engine.Apply;
using Elehko.Dotkiln.Engine.ProjectFiles;
using Elehko.Dotkiln.Updates.Isolation;
using Elehko.Dotkiln.Updates.Planning;
using Elehko.Dotkiln.Updates.Verification;

namespace Elehko.Dotkiln.Updates.Execution;

/// <summary>
/// Runs safe grouped updates in isolated workspaces.
/// </summary>
public sealed class UpdateWorkflow(
    CsprojInspector inspector,
    IProjectIsolator isolator,
    BuildAndTestVerifier verifier,
    ApplyEngine applyEngine)
{
    /// <summary>
    /// Plans, applies, and verifies update groups.
    /// </summary>
    public async Task<IReadOnlyList<UpdateRunResult>> RunAsync(
        string projectPath,
        StackDefinition stack,
        string? requestedGroup = null,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var resolvedProject = inspector.ResolveProjectPath(projectPath);
        var groups = new UpdateGroupPlanner().Plan(stack, requestedGroup);
        var results = new List<UpdateRunResult>();

        foreach (var group in groups)
        {
            var groupStack = stack with { Packages = group.Packages };
            if (dryRun)
            {
                results.Add(new UpdateRunResult(group, true, $"Would update {group.Packages.Count} packages in isolation."));
                continue;
            }

            await using var isolation = await isolator.IsolateAsync(resolvedProject, group.Name, cancellationToken);
            var apply = await applyEngine.ApplyAsync(isolation.ProjectPath, groupStack, dryRun: false, cancellationToken);
            if (!apply.Succeeded)
            {
                var logPath = WriteLog(group.Name, string.Join(Environment.NewLine, apply.Messages));
                results.Add(new UpdateRunResult(group, false, "Package update failed in isolation.", logPath));
                continue;
            }

            var verification = await verifier.VerifyAsync(isolation.WorkingDirectory, cancellationToken);
            if (!verification.Succeeded)
            {
                var logPath = WriteLog(group.Name, verification.Output);
                results.Add(new UpdateRunResult(group, false, "Verification failed. No changes made to your branch.", logPath));
                continue;
            }

            results.Add(new UpdateRunResult(group, true, verification.TestsWereRun ? "Build and tests passed in isolation." : "Build passed in isolation; no test project detected."));
        }

        return results;
    }

    private static string WriteLog(string groupName, string output)
    {
        var fileName = $"Dotkiln-update-{Sanitize(groupName)}.log";
        File.WriteAllText(fileName, output);
        return Path.GetFullPath(fileName);
    }

    private static string Sanitize(string value)
    {
        return string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
    }
}
