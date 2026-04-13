namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Metadata for a user project stored on disk.
/// </summary>
public sealed class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastOpenedAtUtc { get; set; } = DateTime.UtcNow;
}
