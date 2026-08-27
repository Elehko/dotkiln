namespace Elehko.Dotkiln.Updates.Isolation;

/// <summary>
/// Represents an isolated project workspace.
/// </summary>
public sealed record ProjectIsolation(string WorkingDirectory, string ProjectPath, bool IsDisposable) : IAsyncDisposable
{
    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (IsDisposable && Directory.Exists(WorkingDirectory))
        {
            Directory.Delete(WorkingDirectory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }
}
