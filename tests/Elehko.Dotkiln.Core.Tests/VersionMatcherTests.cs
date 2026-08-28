using Elehko.Dotkiln.Core.Versions;

namespace Elehko.Dotkiln.Core.Tests;

public sealed class VersionMatcherTests
{
    [Theory]
    [InlineData("8.0.*", "8.0.8", true)]
    [InlineData("8.0.*", "8.0.9-dev-123", false)]
    [InlineData("8.0.9-dev-123", "8.0.9-dev-123", true)]
    [InlineData("8.0.*", "8.1.0", false)]
    [InlineData("[8.0.0,9.0.0)", "8.0.8", true)]
    [InlineData("[8.0.0,9.0.0)", "8.0.9-preview.1", false)]
    [InlineData("[8.0.0,9.0.0)", "9.0.0", false)]
    [InlineData("6.6.2", "6.6.2", true)]
    public void Matches_supports_exact_wildcard_and_range_versions(string requested, string installed, bool expected)
    {
        Assert.Equal(expected, VersionMatcher.Matches(requested, installed));
    }
}
