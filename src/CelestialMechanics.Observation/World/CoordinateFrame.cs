namespace CelestialMechanics.Observation.World;

/// <summary>
/// Defines the types of coordinate reference frames supported by the visualization.
/// </summary>
public enum CoordinateFrameType
{
    /// <summary>
    /// Earth-centered reference frame.
    /// </summary>
    Geocentric,

    /// <summary>
    /// Sun-centered reference frame.
    /// </summary>
    Heliocentric,

    /// <summary>
    /// Solar System Barycenter-centered reference frame.
    /// </summary>
    Barycentric,

    /// <summary>
    /// Milky Way galactic-center reference frame.
    /// </summary>
    Galactocentric
}

/// <summary>
/// Represents an astronomical coordinate reference frame.
/// </summary>
public sealed class CoordinateFrame
{
    /// <summary>
    /// Gets or sets the type of reference frame.
    /// </summary>
    public CoordinateFrameType Type { get; set; } = CoordinateFrameType.Barycentric;

    /// <summary>
    /// Gets or sets the name of the reference frame origin.
    /// </summary>
    public string Name { get; set; } = "Solar System Barycenter";
}
