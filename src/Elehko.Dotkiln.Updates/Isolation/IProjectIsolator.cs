namespace Elehko.Dotkiln.Updates.Isolation;

/// <summary>
/// Creates disposable workspaces for safe update verification.
/// </summary>
public interface IProjectIsolator
{
    /// <summary>
    /// Creates an isolated workspace for a project file.
    /// </summary>
    Task<ProjectIsolation> IsolateAsync(string projectPath, string groupName, CancellationToken cancellationToken = default);
}
