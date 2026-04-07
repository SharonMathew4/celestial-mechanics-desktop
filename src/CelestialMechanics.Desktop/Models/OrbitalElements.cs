using System;
using CelestialMechanics.Math;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Keplerian orbital elements computed from position and velocity relative to a central body.
/// All angles are in degrees.
/// </summary>
public readonly struct OrbitalElements
{
    /// <summary>Semi-major axis (AU). Negative for hyperbolic orbits.</summary>
    public double SemiMajorAxis { get; init; }

    /// <summary>Eccentricity (0 = circular, 0-1 = elliptic, 1 = parabolic, >1 = hyperbolic).</summary>
    public double Eccentricity { get; init; }

    /// <summary>Inclination in degrees (0-180).</summary>
    public double Inclination { get; init; }

    /// <summary>Longitude of ascending node in degrees (0-360).</summary>
    public double LongitudeOfAscendingNode { get; init; }

    /// <summary>Argument of periapsis in degrees (0-360).</summary>
    public double ArgumentOfPeriapsis { get; init; }

    /// <summary>True anomaly in degrees (0-360).</summary>
    public double TrueAnomaly { get; init; }

    /// <summary>Orbital period (in simulation time units). NaN for non-elliptic orbits.</summary>
    public double Period { get; init; }

    /// <summary>Periapsis distance (AU).</summary>
    public double PeriapsisDistance { get; init; }

    /// <summary>Apoapsis distance (AU). NaN for non-elliptic orbits.</summary>
    public double ApoapsisDistance { get; init; }

    /// <summary>
    /// Orbital energy (specific, per unit mass). Negative = bound, positive = unbound.
    /// </summary>
    public double SpecificOrbitalEnergy { get; init; }

    /// <summary>
    /// Indicates whether this is a valid (computed) set of elements.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Orbit type description (Circular, Elliptic, Parabolic, Hyperbolic).
    /// </summary>
    public string OrbitType => Eccentricity switch
    {
        < 0.0001 => "Circular",
        < 1.0 => "Elliptic",
        < 1.0001 => "Parabolic",
        _ => "Hyperbolic"
    };

    /// <summary>
    /// Computes orbital elements from state vectors relative to a central body.
    /// </summary>
    /// <param name="position">Position relative to central body (AU).</param>
    /// <param name="velocity">Velocity relative to central body (AU/time unit).</param>
    /// <param name="mu">Standard gravitational parameter (G * central mass).</param>
    public static OrbitalElements FromStateVectors(Vec3d position, Vec3d velocity, double mu)
    {
        if (mu <= 0 || position.LengthSquared < 1e-20)
            return new OrbitalElements { IsValid = false };

        double r = position.Length;
        double v = velocity.Length;

        // Specific orbital energy
        double energy = (v * v / 2.0) - (mu / r);

        // Specific angular momentum vector h = r × v
        var h = Cross(position, velocity);
        double hMag = h.Length;

        if (hMag < 1e-20)
        {
            // Radial orbit (degenerate case)
            return new OrbitalElements
            {
                IsValid = true,
                SemiMajorAxis = energy < 0 ? -mu / (2.0 * energy) : double.PositiveInfinity,
                Eccentricity = 1.0,
                Inclination = 0,
                SpecificOrbitalEnergy = energy
            };
        }

        // Node vector n = k × h (k = [0, 1, 0] in Y-up system, but we use Z-up convention for orbits)
        // For Y-up 3D: use [0, 1, 0] as reference
        var k = new Vec3d(0, 1, 0);
        var n = Cross(k, h);
        double nMag = n.Length;

        // Eccentricity vector e = ((v² - μ/r) * r - (r·v) * v) / μ
        double rdotv = Dot(position, velocity);
        var eVec = ((v * v - mu / r) * position - rdotv * velocity) / mu;
        double e = eVec.Length;

        // Semi-major axis
        double a = energy < 0 ? -mu / (2.0 * energy) : double.PositiveInfinity;
        if (System.Math.Abs(energy) < 1e-15)
            a = double.PositiveInfinity; // Parabolic

        // Inclination: angle between h and Y axis
        double i = System.Math.Acos(Clamp(h.Y / hMag, -1, 1)) * (180.0 / System.Math.PI);

        // Longitude of ascending node
        double omega = 0;
        if (nMag > 1e-15)
        {
            omega = System.Math.Acos(Clamp(n.X / nMag, -1, 1)) * (180.0 / System.Math.PI);
            if (n.Z < 0) omega = 360.0 - omega;
        }

        // Argument of periapsis
        double w = 0;
        if (e > 1e-10 && nMag > 1e-15)
        {
            w = System.Math.Acos(Clamp(Dot(n, eVec) / (nMag * e), -1, 1)) * (180.0 / System.Math.PI);
            if (eVec.Y < 0) w = 360.0 - w;
        }
        else if (e > 1e-10)
        {
            // Equatorial orbit
            w = System.Math.Atan2(eVec.Z, eVec.X) * (180.0 / System.Math.PI);
            if (w < 0) w += 360.0;
        }

        // True anomaly
        double nu = 0;
        if (e > 1e-10)
        {
            nu = System.Math.Acos(Clamp(Dot(eVec, position) / (e * r), -1, 1)) * (180.0 / System.Math.PI);
            if (rdotv < 0) nu = 360.0 - nu;
        }
        else if (nMag > 1e-15)
        {
            // Circular orbit
            nu = System.Math.Acos(Clamp(Dot(n, position) / (nMag * r), -1, 1)) * (180.0 / System.Math.PI);
            if (position.Y < 0) nu = 360.0 - nu;
        }

        // Periapsis and apoapsis
        double periapsis = e < 1.0 ? a * (1.0 - e) : (hMag * hMag / mu) / (1.0 + e);
        double apoapsis = e < 1.0 ? a * (1.0 + e) : double.NaN;

        // Orbital period (only for elliptic)
        double period = (e < 1.0 && a > 0) 
            ? 2.0 * System.Math.PI * System.Math.Sqrt(a * a * a / mu)
            : double.NaN;

        return new OrbitalElements
        {
            IsValid = true,
            SemiMajorAxis = a,
            Eccentricity = e,
            Inclination = i,
            LongitudeOfAscendingNode = omega,
            ArgumentOfPeriapsis = w,
            TrueAnomaly = nu,
            Period = period,
            PeriapsisDistance = periapsis,
            ApoapsisDistance = apoapsis,
            SpecificOrbitalEnergy = energy
        };
    }

    private static Vec3d Cross(Vec3d a, Vec3d b) => new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X);

    private static double Dot(Vec3d a, Vec3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    private static double Clamp(double value, double min, double max) =>
        value < min ? min : value > max ? max : value;
}
