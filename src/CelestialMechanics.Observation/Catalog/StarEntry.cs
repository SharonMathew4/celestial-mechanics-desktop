using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Represents a single stellar entry loaded from the catalog.
/// Contains basic identification, coordinates, proper motion, and astrophysical properties.
/// </summary>
public sealed class StarEntry
{
    /// <summary>
    /// Hipparcos catalog identifier (HIP number).
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Right Ascension in degrees [0, 360].
    /// </summary>
    public double RightAscension { get; }

    /// <summary>
    /// Declination in degrees [-90, 90].
    /// </summary>
    public double Declination { get; }

    /// <summary>
    /// Parallax in milliarcseconds (mas).
    /// </summary>
    public float Parallax { get; }

    /// <summary>
    /// Apparent magnitude (V band).
    /// </summary>
    public float Magnitude { get; }

    /// <summary>
    /// Proper motion in Right Ascension direction (mas/year).
    /// </summary>
    public float ProperMotionRa { get; }

    /// <summary>
    /// Proper motion in Declination direction (mas/year).
    /// </summary>
    public float ProperMotionDec { get; }

    /// <summary>
    /// Spectral classification string (e.g. "G2V").
    /// </summary>
    public string SpectralType { get; }

    /// <summary>
    /// Pre-computed 3D Cartesian position in parsecs relative to the solar system barycenter.
    /// </summary>
    public Vec3d Position { get; }

    public StarEntry(
        int id,
        double rightAscension,
        double declination,
        float parallax,
        float magnitude,
        float properMotionRa,
        float properMotionDec,
        string spectralType)
    {
        Id = id;
        RightAscension = rightAscension;
        Declination = declination;
        Parallax = parallax;
        Magnitude = magnitude;
        ProperMotionRa = properMotionRa;
        ProperMotionDec = properMotionDec;
        SpectralType = spectralType ?? string.Empty;

        double distance = Core.CoordinateTransforms.ParallaxToDistance(parallax);
        Position = Core.CoordinateTransforms.EquatorialToCartesian(RightAscension, Declination, distance);
    }
}
