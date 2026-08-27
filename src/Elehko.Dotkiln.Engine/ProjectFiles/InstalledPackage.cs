namespace Elehko.Dotkiln.Engine.ProjectFiles;

/// <summary>
/// Represents a direct package reference found in a project file.
/// </summary>
public sealed record InstalledPackage(string Id, string Version);
