using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Scene;

/// <summary>
/// Represents the 3D double-precision transform of a scene node, containing
/// position, rotation, and scale.
/// </summary>
public sealed class Transform
{
    /// <summary>
    /// Gets or sets the position of the transform relative to its parent.
    /// </summary>
    public Vec3d Position { get; set; } = Vec3d.Zero;

    /// <summary>
    /// Gets or sets the rotation of the transform relative to its parent.
    /// </summary>
    public Quaterniond Rotation { get; set; } = Quaterniond.Identity;

    /// <summary>
    /// Gets or sets the scale of the transform relative to its parent.
    /// </summary>
    public Vec3d Scale { get; set; } = Vec3d.One;
}
