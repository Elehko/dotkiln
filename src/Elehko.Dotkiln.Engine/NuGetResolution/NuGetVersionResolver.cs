using System.Text.Json;
using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Versions;

namespace Elehko.Dotkiln.Engine.NuGetResolution;

/// <summary>
/// Resolves package versions from NuGet's v3 flat-container API.
/// </summary>
public sealed class NuGetVersionResolver : INuGetVersionResolver
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Creates a resolver.
    /// </summary>
    public NuGetVersionResolver(HttpClient? httpClient = null)
    {
        this.httpClient = httpClient ?? new HttpClient();
    }

    /// <inheritdoc />
    public async Task<string?> ResolveLatestMatchingAsync(PackageEntry package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        if (SemanticVersion.TryParse(package.Version, out _) && !package.Version.Contains('*', StringComparison.Ordinal))
        {
            return package.Version;
        }

        var packageId = package.Id.ToLowerInvariant();
        var url = $"https://api.nuget.org/v3-flatcontainer/{packageId}/index.json";

        await using var stream = await httpClient.GetStreamAsync(url, cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("versions", out var versionsElement))
        {
            return null;
        }

        return versionsElement
            .EnumerateArray()
            .Select(element => element.GetString())
            .Where(version => version is not null && VersionMatcher.Matches(package.Version, version))
            .Select(version => SemanticVersion.TryParse(version!, out var parsed) ? parsed : null)
            .Where(version => version is not null)
            .OrderDescending()
            .FirstOrDefault()
            ?.Original;
    }
}
