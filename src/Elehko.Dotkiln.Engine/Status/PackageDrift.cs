namespace Elehko.Dotkiln.Engine.Status;

/// <summary>
/// Describes one package's status relative to a stack.
/// </summary>
public sealed record PackageDrift(string Id, string RequestedVersion, string? InstalledVersion, string Group, string State);
