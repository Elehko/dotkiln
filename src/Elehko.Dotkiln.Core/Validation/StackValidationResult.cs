namespace Elehko.Dotkiln.Core.Validation;

/// <summary>
/// Contains validation messages for a stack definition.
/// </summary>
public sealed record StackValidationResult(IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Gets a value indicating whether no validation errors were found.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}
