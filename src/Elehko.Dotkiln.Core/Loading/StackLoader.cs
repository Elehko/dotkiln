using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Parsing;

namespace Elehko.Dotkiln.Core.Loading;

/// <summary>
/// Loads stack definitions from local files or HTTP URLs.
/// </summary>
public sealed class StackLoader(StackYamlParser parser)
{
    /// <summary>
    /// Loads and parses a stack definition.
    /// </summary>
    public async Task<StackDefinition> LoadAsync(string source, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        try
        {
            var yaml = IsUrl(source)
                ? await LoadUrlAsync(source, cancellationToken)
                : await File.ReadAllTextAsync(source, cancellationToken);

            return parser.Parse(yaml);
        }
        catch (Exception exception) when (exception is not StackParseException)
        {
            throw new StackLoadException($"Unable to load stack source '{source}'.", exception);
        }
    }

    private static bool IsUrl(string source)
    {
        return Uri.TryCreate(source, UriKind.Absolute, out var uri)
            && uri.Scheme is "http" or "https";
    }

    private static async Task<string> LoadUrlAsync(string source, CancellationToken cancellationToken)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(source, cancellationToken);
    }
}
