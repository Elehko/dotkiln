using Dotkiln.Core.Models;

namespace Dotkiln.Updates.Planning;

/// <summary>
/// Represents a set of packages that should be updated atomically.
/// </summary>
public sealed record UpdateGroup(string Name, IReadOnlyList<PackageEntry> Packages);
