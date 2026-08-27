using Elehko.Dotkiln.Core.Models;

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

        if (stack.Packages.Count == 0)
        {
            errors.Add("At least one package is required.");
        }

        foreach (var package in stack.Packages)
        {
            Require(package.Id, "Package id is required.", errors);
            Require(package.Version, $"Package '{package.Id}' must declare a version.", errors);
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
