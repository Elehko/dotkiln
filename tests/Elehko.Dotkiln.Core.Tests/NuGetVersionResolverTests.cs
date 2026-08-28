using System.Net;
using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Engine.NuGetResolution;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class NuGetVersionResolverTests
{
    [Fact]
    public async Task ResolveLatestMatchingAsync_prefers_latest_stable_for_wildcard_versions()
    {
        using var httpClient = new HttpClient(new StubNuGetHandler("""
            {
              "versions": [
                "8.0.2",
                "8.0.3-dev-00346",
                "8.0.3",
                "8.0.4-preview.1"
              ]
            }
            """));

        var resolver = new NuGetVersionResolver(httpClient);

        var resolved = await resolver.ResolveLatestMatchingAsync(new PackageEntry("Serilog.AspNetCore", "8.0.*"));

        Assert.Equal("8.0.3", resolved);
    }

    private sealed class StubNuGetHandler(string response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response)
            });
        }
    }
}
