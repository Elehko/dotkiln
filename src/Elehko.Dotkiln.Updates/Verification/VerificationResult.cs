namespace Elehko.Dotkiln.Updates.Verification;

/// <summary>
/// Represents the result of build and test verification.
/// </summary>
public sealed record VerificationResult(bool Succeeded, string Output, bool TestsWereRun);
