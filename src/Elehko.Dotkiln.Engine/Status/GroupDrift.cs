namespace Elehko.Dotkiln.Engine.Status;

/// <summary>
/// Describes package drift for one stack group.
/// </summary>
public sealed record GroupDrift(string Name, IReadOnlyList<PackageDrift> Packages)
{
    /// <summary>
    /// Gets whether every package in the group matches the stack.
    /// </summary>
    public bool IsUpToDate => Packages.All(package => package.State == "up-to-date");
}
