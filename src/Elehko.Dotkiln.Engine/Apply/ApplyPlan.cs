using Elehko.Dotkiln.Core.Models;

namespace Elehko.Dotkiln.Engine.Apply;

/// <summary>
/// Describes the package operations needed to apply a stack to a project.
/// </summary>
public sealed record ApplyPlan(
    IReadOnlyList<PackageEntry> MissingPackages,
    IReadOnlyList<PackageEntry> OutOfRangePackages)
{
    /// <summary>
    /// Gets a value indicating whether the stack is already represented by the project.
    /// </summary>
    public bool HasChanges => MissingPackages.Count > 0 || OutOfRangePackages.Count > 0;
}
