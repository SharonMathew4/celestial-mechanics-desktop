using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Coordinates;

/// <summary>
/// Provides pure-math coordinate frame transformations between astronomical
/// reference systems. All transformations are independent of rendering.
/// </summary>
public static class CoordinateTransformService
{
    // Obliquity of the ecliptic (J2000.0) in radians: 23.4392911 degrees
    private const double EclipticObliquity = 23.4392911 * System.Math.PI / 180.0;
    private static readonly double CosObliquity = System.Math.Cos(EclipticObliquity);
    private static readonly double SinObliquity = System.Math.Sin(EclipticObliquity);

    // Galactic pole in equatorial coordinates (J2000)
    // North Galactic Pole: RA = 192.85948°, Dec = 27.12825°
    // Galactic center direction: l = 0, b = 0 => RA = 266.405°, Dec = -28.936°
    private const double GalPolRA = 192.85948 * System.Math.PI / 180.0;
    private const double GalPolDec = 27.12825 * System.Math.PI / 180.0;
    private const double GalAscNode = 32.93192 * System.Math.PI / 180.0;

    // ── Equatorial ↔ Ecliptic ───────────────────────────────────────

    /// <summary>
    /// Transforms a position from equatorial (J2000) to ecliptic coordinates.
    /// </summary>
    /// <param name="equatorial">Position in equatorial Cartesian coordinates.</param>
    /// <returns>Position in ecliptic Cartesian coordinates.</returns>
    public static Vec3d EquatorialToEcliptic(Vec3d equatorial)
    {
        double x = equatorial.X;
        double y = equatorial.Y * CosObliquity + equatorial.Z * SinObliquity;
        double z = -equatorial.Y * SinObliquity + equatorial.Z * CosObliquity;
        return new Vec3d(x, y, z);
    }

    /// <summary>
    /// Transforms a position from ecliptic to equatorial (J2000) coordinates.
    /// </summary>
    /// <param name="ecliptic">Position in ecliptic Cartesian coordinates.</param>
    /// <returns>Position in equatorial Cartesian coordinates.</returns>
    public static Vec3d EclipticToEquatorial(Vec3d ecliptic)
    {
        double x = ecliptic.X;
        double y = ecliptic.Y * CosObliquity - ecliptic.Z * SinObliquity;
        double z = ecliptic.Y * SinObliquity + ecliptic.Z * CosObliquity;
        return new Vec3d(x, y, z);
    }

    // ── Equatorial ↔ Galactic ───────────────────────────────────────

    /// <summary>
    /// Transforms a position from equatorial (J2000) to galactic coordinates.
    /// Uses the IAU 1958 galactic coordinate system definition.
    /// </summary>
    /// <param name="equatorial">Position in equatorial Cartesian coordinates.</param>
    /// <returns>Position in galactic Cartesian coordinates.</returns>
    public static Vec3d EquatorialToGalactic(Vec3d equatorial)
    {
        double cosPolRA = System.Math.Cos(GalPolRA);
        double sinPolRA = System.Math.Sin(GalPolRA);
        double cosPolDec = System.Math.Cos(GalPolDec);
        double sinPolDec = System.Math.Sin(GalPolDec);
        double cosAscNode = System.Math.Cos(GalAscNode);
        double sinAscNode = System.Math.Sin(GalAscNode);

        // Step 1: Rotate about Z by -GalPolRA
        double x1 = equatorial.X * cosPolRA + equatorial.Y * sinPolRA;
        double y1 = -equatorial.X * sinPolRA + equatorial.Y * cosPolRA;
        double z1 = equatorial.Z;

        // Step 2: Rotate about X by -(90° - GalPolDec)
        double x2 = x1;
        double y2 = y1 * sinPolDec - z1 * cosPolDec;
        double z2 = y1 * cosPolDec + z1 * sinPolDec;

        // Step 3: Rotate about Z by -GalAscNode
        double x3 = x2 * cosAscNode + y2 * sinAscNode;
        double y3 = -x2 * sinAscNode + y2 * cosAscNode;
        double z3 = z2;

        return new Vec3d(x3, y3, z3);
    }

    /// <summary>
    /// Transforms a position from galactic to equatorial (J2000) coordinates.
    /// </summary>
    /// <param name="galactic">Position in galactic Cartesian coordinates.</param>
    /// <returns>Position in equatorial Cartesian coordinates.</returns>
    public static Vec3d GalacticToEquatorial(Vec3d galactic)
    {
        double cosPolRA = System.Math.Cos(GalPolRA);
        double sinPolRA = System.Math.Sin(GalPolRA);
        double cosPolDec = System.Math.Cos(GalPolDec);
        double sinPolDec = System.Math.Sin(GalPolDec);
        double cosAscNode = System.Math.Cos(GalAscNode);
        double sinAscNode = System.Math.Sin(GalAscNode);

        // Inverse of Step 3: Rotate about Z by +GalAscNode
        double x3 = galactic.X * cosAscNode - galactic.Y * sinAscNode;
        double y3 = galactic.X * sinAscNode + galactic.Y * cosAscNode;
        double z3 = galactic.Z;

        // Inverse of Step 2
        double x2 = x3;
        double y2 = y3 * sinPolDec + z3 * cosPolDec;
        double z2 = -y3 * cosPolDec + z3 * sinPolDec;

        // Inverse of Step 1: Rotate about Z by +GalPolRA
        double x1 = x2 * cosPolRA - y2 * sinPolRA;
        double y1 = x2 * sinPolRA + y2 * cosPolRA;
        double z1 = z2;

        return new Vec3d(x1, y1, z1);
    }

    // ── Heliocentric / Barycentric / Geocentric ─────────────────────

    /// <summary>
    /// Converts from heliocentric coordinates to barycentric by applying
    /// the Sun's offset from the Solar System Barycenter.
    /// </summary>
    /// <param name="heliocentric">Position relative to the Sun.</param>
    /// <param name="sunBarycentricOffset">Sun's position relative to the SSB.</param>
    /// <returns>Position relative to the SSB.</returns>
    public static Vec3d HeliocentricToBarycentric(Vec3d heliocentric, Vec3d sunBarycentricOffset)
    {
        return heliocentric + sunBarycentricOffset;
    }

    /// <summary>
    /// Converts from barycentric to heliocentric coordinates.
    /// </summary>
    /// <param name="barycentric">Position relative to the SSB.</param>
    /// <param name="sunBarycentricOffset">Sun's position relative to the SSB.</param>
    /// <returns>Position relative to the Sun.</returns>
    public static Vec3d BarycentricToHeliocentric(Vec3d barycentric, Vec3d sunBarycentricOffset)
    {
        return barycentric - sunBarycentricOffset;
    }

    /// <summary>
    /// Converts from heliocentric to geocentric coordinates.
    /// </summary>
    /// <param name="heliocentric">Position relative to the Sun.</param>
    /// <param name="earthHeliocentricPosition">Earth's position relative to the Sun.</param>
    /// <returns>Position relative to Earth.</returns>
    public static Vec3d HeliocentricToGeocentric(Vec3d heliocentric, Vec3d earthHeliocentricPosition)
    {
        return heliocentric - earthHeliocentricPosition;
    }

    /// <summary>
    /// Converts from geocentric to heliocentric coordinates.
    /// </summary>
    /// <param name="geocentric">Position relative to Earth.</param>
    /// <param name="earthHeliocentricPosition">Earth's position relative to the Sun.</param>
    /// <returns>Position relative to the Sun.</returns>
    public static Vec3d GeocentricToHeliocentric(Vec3d geocentric, Vec3d earthHeliocentricPosition)
    {
        return geocentric + earthHeliocentricPosition;
    }

    // ── Camera Relative ─────────────────────────────────────────────

    /// <summary>
    /// Converts a world-space position to camera-relative coordinates
    /// using the camera's position as origin.
    /// </summary>
    /// <param name="worldPosition">The position in world space.</param>
    /// <param name="cameraPosition">The camera's position in world space.</param>
    /// <returns>Position relative to the camera.</returns>
    public static Vec3d ToCameraRelative(Vec3d worldPosition, Vec3d cameraPosition)
    {
        return worldPosition - cameraPosition;
    }
}
