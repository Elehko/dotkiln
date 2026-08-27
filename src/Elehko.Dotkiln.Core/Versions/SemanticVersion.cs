namespace Elehko.Dotkiln.Core.Versions;

/// <summary>
/// Represents a comparable semantic version core.
/// </summary>
public sealed record SemanticVersion(int Major, int Minor, int Patch, string Original) : IComparable<SemanticVersion>
{
    /// <summary>
    /// Parses common NuGet semantic versions, ignoring prerelease labels for ordering.
    /// </summary>
    public static bool TryParse(string value, out SemanticVersion version)
    {
        version = new SemanticVersion(0, 0, 0, string.Empty);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var core = value.Split('-', '+')[0];
        var parts = core.Split('.');
        if (parts.Length == 0 || parts.Length > 4)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var major))
        {
            return false;
        }

        var minor = parts.Length > 1 && int.TryParse(parts[1], out var parsedMinor) ? parsedMinor : 0;
        var patch = parts.Length > 2 && int.TryParse(parts[2], out var parsedPatch) ? parsedPatch : 0;
        version = new SemanticVersion(major, minor, patch, value);
        return true;
    }

    /// <inheritdoc />
    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        var major = Major.CompareTo(other.Major);
        if (major != 0)
        {
            return major;
        }

        var minor = Minor.CompareTo(other.Minor);
        return minor != 0 ? minor : Patch.CompareTo(other.Patch);
    }
}
