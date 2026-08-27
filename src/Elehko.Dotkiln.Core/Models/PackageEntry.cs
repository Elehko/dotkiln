namespace Elehko.Dotkiln.Core.Models;

/// <summary>
/// Describes one direct NuGet package managed by a Dotkiln stack.
/// </summary>
public sealed record PackageEntry(
    string Id,
    string Version,
    string? Group = null);
