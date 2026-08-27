using Elehko.Dotkiln.Core.Models;

namespace Elehko.Dotkiln.Updates.Planning;

/// <summary>
/// Represents a set of packages that should be updated atomically.
/// </summary>
public sealed record UpdateGroup(string Name, IReadOnlyList<PackageEntry> Packages);
