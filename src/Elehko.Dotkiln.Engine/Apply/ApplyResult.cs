namespace Elehko.Dotkiln.Engine.Apply;

/// <summary>
/// Represents the result of applying stack packages.
/// </summary>
public sealed record ApplyResult(bool Succeeded, ApplyPlan Plan, IReadOnlyList<string> Messages);
