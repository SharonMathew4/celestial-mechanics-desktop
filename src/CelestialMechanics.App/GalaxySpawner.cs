using CelestialMechanics.Math;
using CelestialMechanics.Physics.ScenarioGenerators;
using CelestialMechanics.Physics.Types;
using CelestialMechanics.Data;

namespace CelestialMechanics.App;

/// <summary>
/// Handles galaxy placement in the IDE: detects galaxy templates, generates
/// particle clouds via MiyamotoNagaiDiskGenerator, applies correct galactic
/// scale (10,000x positions, 1000x slower velocities).
///
/// SCALE RATIONALE
/// ───────────────
/// Stars are at AU scale (radius ~0.005 AU, separation ~1 AU).
/// Galaxies are at kpc scale (~30 kpc = ~6.2e9 AU diameter).
/// A 10,000x upscale puts the galaxy disk radius at ~30,000 AU in sim coords,
/// which is large enough to visually distinguish from stars while remaining
/// numerically manageable.
///
/// Galactic orbital periods are ~250 Myr vs solar system ~1 yr, so
/// velocities must be ~1000x slower to avoid the galaxy dissolving instantly.
/// </summary>
public static class GalaxySpawner
{
    /// <summary>Scale factor for galaxy positions relative to stellar scale.</summary>
    public const double PositionScale = 10000.0;

    /// <summary>Velocity reduction factor for galactic timescales.</summary>
    public const double VelocityScale = 0.001;

    /// <summary>Camera distance for galaxy viewing.</summary>
    public const float GalaxyCameraDistance = 80000f;

    /// <summary>
    /// Number of particles per galaxy. Kept low for real-time interactivity.
    /// At 2000 particles with billboard rendering, we maintain 60+ FPS.
    /// </summary>
    public const int DefaultParticleCount = 2000;

    /// <summary>
    /// Template names that trigger galaxy particle generation instead of single-body placement.
    /// </summary>
    private static readonly HashSet<string> GalaxyTemplateNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Spiral Galaxy",
        "Elliptical Galaxy",
        "Lenticular Galaxy",
        "Irregular Galaxy",
    };

    /// <summary>
    /// Returns true if the given template name corresponds to a galaxy type
    /// that should be spawned as a particle cloud rather than a single body.
    /// </summary>
    public static bool IsGalaxyTemplate(string? templateName)
    {
        return templateName != null && GalaxyTemplateNames.Contains(templateName);
    }

    /// <summary>
    /// Generate galaxy particles at the given position with appropriate scale.
    /// Returns an array of PhysicsBodies representing the galaxy.
    /// Uses batch-friendly array return for SetBodies/concat usage.
    /// </summary>
    public static PhysicsBody[] GenerateGalaxy(
        string templateName,
        int startId,
        Vec3d centerPosition,
        Vec3d centerVelocity,
        int particleCount = DefaultParticleCount)
    {
        // Scale position and velocity for galactic scale
        Vec3d scaledPos = centerPosition * PositionScale;
        Vec3d scaledVel = centerVelocity * VelocityScale;

        // Choose parameters based on galaxy type
        var (spiralPitch, spiralStrength, armCount, barStrength, barLength,
             bulgeFraction, haloFraction, inclinationDeg) = templateName.ToLowerInvariant() switch
        {
            "elliptical galaxy" => (0.0, 0.0, 0, 0.0, 0.0, 0.45, 0.30, 0.0),
            "lenticular galaxy" => (0.0, 0.1, 0, 0.0, 0.0, 0.30, 0.20, 30.0),
            "irregular galaxy" => (25.0, 0.2, 3, 0.0, 0.0, 0.10, 0.15, 15.0),
            _ /* spiral */ => (14.0, 0.6, 2, 0.0, 0.0, 0.15, 0.10, 30.0),
        };

        // Scale lengths for galactic coordinate system
        double scaledDiskRadial = 3.0 * PositionScale;
        double scaledDiskVertical = 0.15 * PositionScale;
        double scaledBulgeScale = 0.5 * PositionScale;
        double scaledHaloScale = 10.0 * PositionScale;
        double scaledBarLength = barLength * PositionScale;

        // Generate using MiyamotoNagaiDiskGenerator with galactic scale
        var bodies = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: particleCount,
            smbhMass: 500.0,
            bulgeMass: 200.0,
            bulgeScale: scaledBulgeScale,
            diskMass: 300.0,
            diskRadialScale: scaledDiskRadial,
            diskVerticalScale: scaledDiskVertical,
            haloMass: 1000.0,
            haloScale: scaledHaloScale,
            spiralPitchDeg: spiralPitch,
            spiralArmCount: armCount,
            spiralStrength: spiralStrength,
            barStrength: barStrength,
            barLength: scaledBarLength,
            dustFraction: 0.12,
            hiiFraction: 0.05,
            youngStarFraction: 0.08,
            bulgeFraction: bulgeFraction,
            haloFraction: haloFraction,
            centerPosition: scaledPos,
            centerVelocity: scaledVel,
            inclinationDeg: inclinationDeg,
            positionAngleDeg: 0.0,
            seed: -1);  // Random seed each time

        // Re-index body IDs and scale radii for visibility
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].Id = startId + i;
            bodies[i].Radius *= PositionScale;
        }

        return bodies;
    }
}
