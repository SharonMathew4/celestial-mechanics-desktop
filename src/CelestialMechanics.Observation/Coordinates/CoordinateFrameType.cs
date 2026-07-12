namespace CelestialMechanics.Observation.Coordinates;

/// <summary>
/// Defines the supported astronomical coordinate reference frames.
/// </summary>
public enum CoordinateFrameType
{
    /// <summary>Sun-centered reference frame.</summary>
    Heliocentric,

    /// <summary>Solar System Barycenter reference frame.</summary>
    Barycentric,

    /// <summary>Earth-centered reference frame.</summary>
    Geocentric,

    /// <summary>Milky Way galactic-center reference frame.</summary>
    Galactic,

    /// <summary>Right Ascension / Declination based reference frame (J2000).</summary>
    Equatorial,

    /// <summary>Ecliptic plane reference frame.</summary>
    Ecliptic,

    /// <summary>Camera-relative reference frame for rendering.</summary>
    CameraRelative
}
