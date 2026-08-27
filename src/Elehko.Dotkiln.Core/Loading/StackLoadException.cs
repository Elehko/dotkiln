namespace Elehko.Dotkiln.Core.Loading;

/// <summary>
/// Raised when a stack source cannot be loaded.
/// </summary>
public sealed class StackLoadException(string message, Exception? innerException = null) : Exception(message, innerException);
