namespace Elehko.Dotkiln.Engine.Status;

/// <summary>
/// Represents a project's status relative to a stack.
/// </summary>
public sealed record StackStatus(string StackName, IReadOnlyList<GroupDrift> Groups)
{
    /// <summary>
    /// Gets whether any group has drift.
    /// </summary>
    public bool HasDrift => Groups.Any(group => !group.IsUpToDate);
}
