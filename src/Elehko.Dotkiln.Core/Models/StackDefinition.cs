namespace Dotkiln.Core.Models;

/// <summary>
/// Represents a versioned package stack definition loaded from a Dotkiln YAML file.
/// </summary>
public sealed record StackDefinition(
    string Name,
    string Description,
    string TargetFramework,
    IReadOnlyList<PackageEntry> Packages,
    string? SchemaVersion = null,
    string? Snippet = null);
