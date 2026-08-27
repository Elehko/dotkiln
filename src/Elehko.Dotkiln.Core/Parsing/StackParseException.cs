namespace Elehko.Dotkiln.Core.Parsing;

/// <summary>
/// Raised when a stack YAML document cannot be parsed into a valid object shape.
/// </summary>
public sealed class StackParseException(string message) : Exception(message);
