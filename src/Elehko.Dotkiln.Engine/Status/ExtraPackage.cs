namespace Elehko.Dotkiln.Engine.Status;

/// <summary>
/// Represents a direct package reference that is installed but not declared by the stack.
/// </summary>
public sealed record ExtraPackage(
    string Id,
    string Version);
