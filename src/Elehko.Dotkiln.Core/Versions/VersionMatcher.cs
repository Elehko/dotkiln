using System.Text.RegularExpressions;

namespace Elehko.Dotkiln.Core.Versions;

/// <summary>
/// Matches installed versions against Dotkiln version constraints.
/// </summary>
public static partial class VersionMatcher
{
    /// <summary>
    /// Returns whether an installed package version satisfies a requested stack version.
    /// </summary>
    public static bool Matches(string requested, string installed)
    {
        if (string.Equals(requested, installed, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!AllowsPrerelease(requested) && IsPrerelease(installed))
        {
            return false;
        }

        if (requested.EndsWith(".*", StringComparison.Ordinal))
        {
            var prefix = requested[..^1];
            return installed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return MatchesRange(requested, installed);
    }

    /// <summary>
    /// Returns whether a stack version expression is syntactically supported.
    /// </summary>
    public static bool IsSupportedExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        if (expression.EndsWith(".*", StringComparison.Ordinal))
        {
            return SemanticVersion.TryParse(expression[..^2], out _);
        }

        return SemanticVersion.TryParse(expression, out _) || RangePattern().IsMatch(expression);
    }

    /// <summary>
    /// Returns whether a version expression explicitly allows prerelease versions.
    /// </summary>
    public static bool AllowsPrerelease(string expression)
    {
        return expression.Contains('-', StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns whether a concrete package version has a prerelease label.
    /// </summary>
    public static bool IsPrerelease(string version)
    {
        return SemanticVersion.TryParse(version, out var parsed) && parsed.IsPrerelease;
    }

    private static bool MatchesRange(string requested, string installed)
    {
        var match = RangePattern().Match(requested);
        if (!match.Success || !SemanticVersion.TryParse(installed, out var installedVersion))
        {
            return false;
        }

        var lowerInclusive = match.Groups["lowerBracket"].Value == "[";
        var upperInclusive = match.Groups["upperBracket"].Value == "]";
        var lowerText = match.Groups["lower"].Value.Trim();
        var upperText = match.Groups["upper"].Value.Trim();

        if (lowerText.Length > 0 && SemanticVersion.TryParse(lowerText, out var lower))
        {
            var comparison = installedVersion.CompareTo(lower);
            if (comparison < 0 || comparison == 0 && !lowerInclusive)
            {
                return false;
            }
        }

        if (upperText.Length > 0 && SemanticVersion.TryParse(upperText, out var upper))
        {
            var comparison = installedVersion.CompareTo(upper);
            if (comparison > 0 || comparison == 0 && !upperInclusive)
            {
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex("^(?<lowerBracket>[\\[\\(])(?<lower>[^,]*),(?<upper>[^\\]\\)]*)(?<upperBracket>[\\]\\)])$")]
    private static partial Regex RangePattern();
}
