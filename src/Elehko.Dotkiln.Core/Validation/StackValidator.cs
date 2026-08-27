using Elehko.Dotkiln.Core.Models;
using Elehko.Dotkiln.Core.Versions;

namespace Elehko.Dotkiln.Core.Validation;

/// <summary>
/// Validates stack definitions before they are applied or published.
/// </summary>
public sealed class StackValidator
{
    /// <summary>
    /// Validates required stack metadata and package entries.
    /// </summary>
    public StackValidationResult Validate(StackDefinition stack)
    {
        ArgumentNullException.ThrowIfNull(stack);

        var errors = new List<string>();
        Require(stack.Name, "Stack name is required.", errors);
        Require(stack.TargetFramework, "Target framework is required.", errors);
        Require(stack.SchemaVersion, "Schema version is required.", errors);

        if (!string.Equals(stack.SchemaVersion, "0.1", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Schema version '{stack.SchemaVersion}' is not supported by this Dotkiln build.");
        }

        if (stack.Packages.Count == 0)
        {
            errors.Add("At least one package is required.");
        }

        foreach (var package in stack.Packages)
        {
            Require(package.Id, "Package id is required.", errors);
            Require(package.Version, $"Package '{package.Id}' must declare a version.", errors);
            if (!VersionMatcher.IsSupportedExpression(package.Version))
            {
                errors.Add($"Package '{package.Id}' has an unsupported version expression '{package.Version}'.");
            }

            if (package.Group is { Length: > 0 } && package.Group.Any(character => !char.IsLower(character) && !char.IsDigit(character) && character != '-'))
            {
                errors.Add($"Package '{package.Id}' uses invalid group '{package.Group}'. Use lowercase letters, numbers, and hyphens.");
            }
        }

        var duplicatePackages = stack.Packages
            .GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        errors.AddRange(duplicatePackages.Select(packageId => $"Package '{packageId}' is declared more than once."));

        return new StackValidationResult(errors);
    }

    private static void Require(string? value, string message, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(message);
        }
    }
}
