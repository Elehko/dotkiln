using Elehko.Dotkiln.Core.Models;

namespace Elehko.Dotkiln.Updates.Planning;

/// <summary>
/// Groups stack packages into safe update units.
/// </summary>
public sealed class UpdateGroupPlanner
{
    /// <summary>
    /// Groups packages by their declared group, falling back to the package id for ungrouped packages.
    /// </summary>
    public IReadOnlyList<UpdateGroup> Plan(StackDefinition stack, string? requestedGroup = null)
    {
        ArgumentNullException.ThrowIfNull(stack);

        return stack.Packages
            .GroupBy(package => string.IsNullOrWhiteSpace(package.Group) ? package.Id : package.Group)
            .Where(group => requestedGroup is null || string.Equals(group.Key, requestedGroup, StringComparison.OrdinalIgnoreCase))
            .Select(group => new UpdateGroup(group.Key, group.ToArray()))
            .OrderBy(group => group.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
