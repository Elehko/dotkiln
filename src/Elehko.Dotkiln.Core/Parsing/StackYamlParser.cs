using Elehko.Dotkiln.Core.Models;

namespace Elehko.Dotkiln.Core.Parsing;

/// <summary>
/// Parses Dotkiln's intentionally small stack YAML subset.
/// </summary>
public sealed class StackYamlParser
{
    /// <summary>
    /// Parses a stack definition from YAML text.
    /// </summary>
    public StackDefinition Parse(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);

        var scalars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var packages = new List<PackageEntry>();

        bool inPackages = false;
        Dictionary<string, string>? currentPackage = null;

        foreach (var rawLine in yaml.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = StripComment(rawLine);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.Trim();
            if (trimmed.Equals("packages:", StringComparison.OrdinalIgnoreCase))
            {
                inPackages = true;
                continue;
            }

            if (!inPackages)
            {
                var pair = ReadPair(trimmed);
                if (pair is not null)
                {
                    scalars[pair.Value.Key] = pair.Value.Value;
                }

                continue;
            }

            if (trimmed.StartsWith("- ", StringComparison.Ordinal))
            {
                AddPackageIfReady(packages, currentPackage);
                currentPackage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var inlinePair = ReadPair(trimmed[2..].Trim());
                if (inlinePair is not null)
                {
                    currentPackage[inlinePair.Value.Key] = inlinePair.Value.Value;
                }

                continue;
            }

            var packagePair = ReadPair(trimmed);
            if (currentPackage is not null && packagePair is not null)
            {
                currentPackage[packagePair.Value.Key] = packagePair.Value.Value;
            }
        }

        AddPackageIfReady(packages, currentPackage);

        return new StackDefinition(
            Required(scalars, "name"),
            scalars.GetValueOrDefault("description") ?? string.Empty,
            Required(scalars, "targetFramework"),
            packages,
            scalars.GetValueOrDefault("schemaVersion"),
            scalars.GetValueOrDefault("snippet"));
    }

    private static void AddPackageIfReady(ICollection<PackageEntry> packages, Dictionary<string, string>? package)
    {
        if (package is null)
        {
            return;
        }

        if (!package.TryGetValue("id", out var id) || !package.TryGetValue("version", out var version))
        {
            throw new StackParseException("Each package entry must include id and version.");
        }

        packages.Add(new PackageEntry(id, version, package.GetValueOrDefault("group")));
    }

    private static KeyValuePair<string, string>? ReadPair(string line)
    {
        var separator = line.IndexOf(':');
        if (separator <= 0)
        {
            return null;
        }

        var key = line[..separator].Trim();
        var value = line[(separator + 1)..].Trim().Trim('"', '\'');
        return new KeyValuePair<string, string>(key, value);
    }

    private static string Required(IReadOnlyDictionary<string, string> scalars, string key)
    {
        if (!scalars.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new StackParseException($"Missing required stack field '{key}'.");
        }

        return value;
    }

    private static string StripComment(string line)
    {
        var commentIndex = line.IndexOf('#');
        return commentIndex < 0 ? line : line[..commentIndex];
    }
}
