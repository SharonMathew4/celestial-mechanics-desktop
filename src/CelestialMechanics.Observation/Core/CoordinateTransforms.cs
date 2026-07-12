using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Provides mathematical transformations for celestial coordinates.
/// Transforms Right Ascension (RA) and Declination (Dec) into 3D Cartesian coordinates.
/// </summary>
public static class CoordinateTransforms
{
    /// <summary>
    /// Converts equatorial coordinates (Right Ascension, Declination, Distance) to Cartesian XYZ coordinates.
    /// </summary>
    /// <param name="raDegrees">Right ascension in degrees [0, 360].</param>
    /// <param name="decDegrees">Declination in degrees [-90, 90].</param>
    /// <param name="distanceParsecs">Distance to the object in parsecs.</param>
    /// <returns>A <see cref="Vec3d"/> representing the Cartesian coordinates.</returns>
    public static Vec3d EquatorialToCartesian(double raDegrees, double decDegrees, double distanceParsecs)
    {
        double raRad = raDegrees * System.Math.PI / 180.0;
        double decRad = decDegrees * System.Math.PI / 180.0;

        double cosDec = System.Math.Cos(decRad);
        double x = distanceParsecs * cosDec * System.Math.Cos(raRad);
        double y = distanceParsecs * cosDec * System.Math.Sin(raRad);
        double z = distanceParsecs * System.Math.Sin(decRad);

        return new Vec3d(x, y, z);
    }

    /// <summary>
    /// Converts equatorial coordinates given in sexagesimal components to Cartesian XYZ coordinates.
    /// </summary>
    public static Vec3d EquatorialToCartesian(
        double raHours, double raMinutes, double raSeconds,
        double decDegrees, double decMinutes, double decSeconds,
        double distanceParsecs)
    {
        double raDecimalDegrees = (raHours + raMinutes / 60.0 + raSeconds / 3600.0) * 15.0;
        
        // Determine the sign of declination carefully (decDegrees could be 0 but minutes or seconds negative)
        double sign = 1.0;
        if (decDegrees < 0.0 || decMinutes < 0.0 || decSeconds < 0.0)
        {
            sign = -1.0;
        }
        
        double absDecDegrees = System.Math.Abs(decDegrees);
        double absDecMinutes = System.Math.Abs(decMinutes);
        double absDecSeconds = System.Math.Abs(decSeconds);
        
        double decDecimalDegrees = sign * (absDecDegrees + absDecMinutes / 60.0 + absDecSeconds / 3600.0);

        return EquatorialToCartesian(raDecimalDegrees, decDecimalDegrees, distanceParsecs);
    }

    /// <summary>
    /// Converts stellar parallax to distance in parsecs.
    /// </summary>
    /// <param name="parallaxMas">Parallax in milliarcseconds (mas).</param>
    /// <param name="fallbackDistance">Fallback distance in parsecs if parallax is zero or negative.</param>
    public static double ParallaxToDistance(double parallaxMas, double fallbackDistance = 10000.0)
    {
        if (parallaxMas <= 0.0)
        {
            return fallbackDistance;
        }
        return 1000.0 / parallaxMas;
    }
}
