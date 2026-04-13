using CelestialMechanics.Math;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Keplerian elements derived from relative state vectors.
/// </summary>
public sealed class OrbitalElements
{
    public bool IsValid { get; init; }
    public string OrbitType { get; init; } = "Unknown";
    public double SemiMajorAxis { get; init; }
    public double Eccentricity { get; init; }
    public double Inclination { get; init; }
    public double LongitudeOfAscendingNode { get; init; }
    public double ArgumentOfPeriapsis { get; init; }
    public double TrueAnomaly { get; init; }
    public double Period { get; init; }
    public double PeriapsisDistance { get; init; }
    public double ApoapsisDistance { get; init; }
    public double SpecificOrbitalEnergy { get; init; }

    public static OrbitalElements FromStateVectors(Vec3d r, Vec3d v, double mu)
    {
        if (mu <= 0)
        {
            return new OrbitalElements { IsValid = false };
        }

        var rMag = r.Length;
        var vMag = v.Length;
        if (rMag <= 0 || vMag <= 0)
        {
            return new OrbitalElements { IsValid = false };
        }

        var h = Vec3d.Cross(r, v);
        var hMag = h.Length;
        if (hMag <= 0)
        {
            return new OrbitalElements { IsValid = false };
        }

        var n = Vec3d.Cross(Vec3d.UnitZ, h);
        var nMag = n.Length;

        var eVec = (Vec3d.Cross(v, h) / mu) - (r / rMag);
        var e = eVec.Length;

        var energy = 0.5 * vMag * vMag - mu / rMag;
        var a = System.Math.Abs(energy) < 1e-12 ? double.PositiveInfinity : -mu / (2.0 * energy);

        var i = System.Math.Acos(System.Math.Clamp(h.Z / hMag, -1.0, 1.0));

        double omegaNode = 0;
        if (nMag > 1e-12)
        {
            omegaNode = System.Math.Acos(System.Math.Clamp(n.X / nMag, -1.0, 1.0));
            if (n.Y < 0) omegaNode = 2.0 * System.Math.PI - omegaNode;
        }

        double argPeriapsis = 0;
        if (nMag > 1e-12 && e > 1e-12)
        {
            argPeriapsis = System.Math.Acos(System.Math.Clamp(Vec3d.Dot(n, eVec) / (nMag * e), -1.0, 1.0));
            if (eVec.Z < 0) argPeriapsis = 2.0 * System.Math.PI - argPeriapsis;
        }

        double trueAnomaly = 0;
        if (e > 1e-12)
        {
            trueAnomaly = System.Math.Acos(System.Math.Clamp(Vec3d.Dot(eVec, r) / (e * rMag), -1.0, 1.0));
            if (Vec3d.Dot(r, v) < 0) trueAnomaly = 2.0 * System.Math.PI - trueAnomaly;
        }

        var periapsis = a * (1.0 - e);
        var apoapsis = e < 1.0 ? a * (1.0 + e) : double.PositiveInfinity;
        var period = (e < 1.0 && a > 0)
            ? 2.0 * System.Math.PI * System.Math.Sqrt((a * a * a) / mu)
            : double.PositiveInfinity;

        var type = e switch
        {
            < 0.999999 => "Elliptic",
            <= 1.000001 => "Parabolic",
            _ => "Hyperbolic",
        };

        static double ToDegrees(double radians) => radians * (180.0 / System.Math.PI);

        return new OrbitalElements
        {
            IsValid = true,
            OrbitType = type,
            SemiMajorAxis = a,
            Eccentricity = e,
            Inclination = ToDegrees(i),
            LongitudeOfAscendingNode = ToDegrees(omegaNode),
            ArgumentOfPeriapsis = ToDegrees(argPeriapsis),
            TrueAnomaly = ToDegrees(trueAnomaly),
            Period = period,
            PeriapsisDistance = periapsis,
            ApoapsisDistance = apoapsis,
            SpecificOrbitalEnergy = energy,
        };
    }
}
