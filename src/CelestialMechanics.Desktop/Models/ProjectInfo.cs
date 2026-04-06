using System;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Metadata for a saved Celestial Mechanics project.
/// Serialized as project.json in the project directory.
/// </summary>
public class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
}
