using Elehko.Dotkiln.Core.Models;

namespace Elehko.Dotkiln.Engine.NuGetResolution;

/// <summary>
/// Resolves concrete NuGet versions from stack package constraints.
/// </summary>
public interface INuGetVersionResolver
{
    /// <summary>
    /// Resolves the newest available version matching the package entry's version expression.
    /// </summary>
    Task<string?> ResolveLatestMatchingAsync(PackageEntry package, CancellationToken cancellationToken = default);
}
