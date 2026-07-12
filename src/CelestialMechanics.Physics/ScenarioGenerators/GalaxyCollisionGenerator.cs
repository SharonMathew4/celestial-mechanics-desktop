using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Physics.ScenarioGenerators;

/// <summary>
/// Pre-configured galaxy collision and morphology scenarios inspired by
/// JWST and Hubble imagery.
///
/// Each scenario produces a PhysicsBody[] array ready for simulation.
/// Parameters are inverse-engineered from the reference imagery to
/// reproduce the observed morphological features.
/// </summary>
public static class GalaxyCollisionGenerator
{
    /// <summary>
    /// Cartwheel Galaxy collision (heic0706a / JWST).
    /// A small compact intruder punches through the center of a disk galaxy,
    /// triggering concentric density wave rings and radial spoke structures.
    ///
    /// Observed features to replicate:
    /// - Outer blue ring (young star clusters)
    /// - Inner golden ring (older stellar population)
    /// - Radial spoke lanes connecting inner and outer rings
    /// - Small intruder galaxy offset from center
    /// </summary>
    public static PhysicsBody[] CartwheelCollision(
        int targetCount = 35000, int intruderCount = 5000, int seed = 42)
    {
        var target = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: targetCount,
            smbhMass: 400.0,
            bulgeMass: 150.0, bulgeScale: 0.4,
            diskMass: 250.0, diskRadialScale: 4.0, diskVerticalScale: 0.12,
            haloMass: 800.0, haloScale: 12.0,
            spiralPitchDeg: 20.0, spiralArmCount: 4, spiralStrength: 0.3,
            barStrength: 0.0, barLength: 0.0,
            dustFraction: 0.10, hiiFraction: 0.06, youngStarFraction: 0.10,
            bulgeFraction: 0.12, haloFraction: 0.08,
            seed: seed);

        // Compact intruder: smaller galaxy approaching head-on from above
        var intruder = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: intruderCount,
            smbhMass: 100.0,
            bulgeMass: 60.0, bulgeScale: 0.3,
            diskMass: 80.0, diskRadialScale: 1.5, diskVerticalScale: 0.1,
            haloMass: 200.0, haloScale: 5.0,
            spiralPitchDeg: 18.0, spiralArmCount: 2, spiralStrength: 0.2,
            dustFraction: 0.08, hiiFraction: 0.03, youngStarFraction: 0.05,
            bulgeFraction: 0.20, haloFraction: 0.10,
            centerPosition: new Vec3d(0, 12, 0),
            centerVelocity: new Vec3d(0, -2.5, 0),
            seed: seed + 1);

        return MergeBodies(target, intruder);
    }

    /// <summary>
    /// Tadpole Galaxy encounter (heic0206a).
    /// A smaller galaxy flies past a spiral galaxy, stripping a long tidal tail
    /// populated with blue young star clusters.
    ///
    /// Observed features to replicate:
    /// - Long tidal tail extending ~280 kly with blue star clusters at the tip
    /// - Disturbed main disk with brown dust lanes
    /// - Small intruder galaxy visible in the disk
    /// </summary>
    public static PhysicsBody[] TadpoleEncounter(
        int mainCount = 35000, int intruderCount = 8000, int seed = 42)
    {
        var main = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: mainCount,
            smbhMass: 350.0,
            bulgeMass: 120.0, bulgeScale: 0.4,
            diskMass: 280.0, diskRadialScale: 3.5, diskVerticalScale: 0.12,
            haloMass: 900.0, haloScale: 11.0,
            spiralPitchDeg: 16.0, spiralArmCount: 2, spiralStrength: 0.5,
            dustFraction: 0.14, hiiFraction: 0.04, youngStarFraction: 0.08,
            bulgeFraction: 0.12, haloFraction: 0.10,
            inclinationDeg: 30.0,
            seed: seed);

        // Intruder on a prograde flyby trajectory
        var intruder = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: intruderCount,
            smbhMass: 80.0,
            bulgeMass: 40.0, bulgeScale: 0.25,
            diskMass: 60.0, diskRadialScale: 1.2, diskVerticalScale: 0.08,
            haloMass: 150.0, haloScale: 4.0,
            spiralPitchDeg: 20.0, spiralArmCount: 2, spiralStrength: 0.3,
            dustFraction: 0.06, hiiFraction: 0.02, youngStarFraction: 0.04,
            bulgeFraction: 0.25, haloFraction: 0.12,
            centerPosition: new Vec3d(6, 3, 0),
            centerVelocity: new Vec3d(-1.5, -0.8, 0.3),
            seed: seed + 1);

        return MergeBodies(main, intruder);
    }

    /// <summary>
    /// Mice Galaxies merger (NGC 4676, user image 3).
    /// Two nearly equal-mass spiral galaxies in a close encounter,
    /// producing prominent dual tidal tails.
    ///
    /// Observed features to replicate:
    /// - Two distinct galaxy cores with bright central starburst
    /// - Long tidal tails extending in opposite directions
    /// - Blue young star clusters along the tails
    /// - Tidal bridge connecting the two galaxies
    /// </summary>
    public static PhysicsBody[] MiceGalaxiesMerger(
        int count1 = 25000, int count2 = 25000, int seed = 42)
    {
        var galaxy1 = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: count1,
            smbhMass: 300.0,
            bulgeMass: 130.0, bulgeScale: 0.35,
            diskMass: 220.0, diskRadialScale: 3.0, diskVerticalScale: 0.12,
            haloMass: 700.0, haloScale: 10.0,
            spiralPitchDeg: 18.0, spiralArmCount: 2, spiralStrength: 0.45,
            dustFraction: 0.10, hiiFraction: 0.05, youngStarFraction: 0.08,
            bulgeFraction: 0.15, haloFraction: 0.10,
            centerPosition: new Vec3d(-4, 0, 0),
            centerVelocity: new Vec3d(0.8, 0.5, 0),
            inclinationDeg: 15.0,
            seed: seed);

        var galaxy2 = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: count2,
            smbhMass: 280.0,
            bulgeMass: 110.0, bulgeScale: 0.3,
            diskMass: 200.0, diskRadialScale: 2.8, diskVerticalScale: 0.10,
            haloMass: 650.0, haloScale: 9.0,
            spiralPitchDeg: 20.0, spiralArmCount: 2, spiralStrength: 0.4,
            dustFraction: 0.10, hiiFraction: 0.05, youngStarFraction: 0.08,
            bulgeFraction: 0.15, haloFraction: 0.10,
            centerPosition: new Vec3d(4, 0, 0),
            centerVelocity: new Vec3d(-0.8, -0.5, 0),
            inclinationDeg: -20.0, positionAngleDeg: 45.0,
            seed: seed + 1);

        return MergeBodies(galaxy1, galaxy2);
    }

    /// <summary>
    /// Sombrero Galaxy (M104, user image 4).
    /// Edge-on spiral galaxy with prominent dust lane and luminous bulge.
    ///
    /// Observed features to replicate:
    /// - Very large smooth golden bulge (dominant)
    /// - Thin disk viewed edge-on with dark dust lane bisecting the bulge
    /// - Slight asymmetric dust distribution
    /// - Diffraction spikes on foreground stars
    /// </summary>
    public static PhysicsBody[] SombreroEdgeOn(int count = 50000, int seed = 42)
    {
        return MiyamotoNagaiDiskGenerator.Generate(
            totalCount: count,
            smbhMass: 600.0,
            bulgeMass: 500.0, bulgeScale: 0.8,  // Very large bulge (M104 signature)
            diskMass: 200.0, diskRadialScale: 4.0, diskVerticalScale: 0.08,  // Very thin disk
            haloMass: 1200.0, haloScale: 15.0,
            spiralPitchDeg: 12.0, spiralArmCount: 4, spiralStrength: 0.2,
            dustFraction: 0.20,  // Heavy dust lane
            hiiFraction: 0.02, youngStarFraction: 0.03,
            bulgeFraction: 0.35,  // Large bulge fraction
            haloFraction: 0.08,
            inclinationDeg: 84.0,  // Nearly edge-on
            seed: seed);
    }

    /// <summary>
    /// M81 grand design spiral.
    /// Classic two-armed spiral galaxy with tight winding and prominent H-II regions.
    ///
    /// Observed features from heic1302a/heic1305a:
    /// - Clear two-armed grand design spiral pattern
    /// - Pink H-II regions along spiral arms (M106-like)
    /// - Golden central core
    /// - Dark dust lanes on inner edges of spiral arms
    /// </summary>
    public static PhysicsBody[] M81Spiral(int count = 50000, int seed = 42)
    {
        return MiyamotoNagaiDiskGenerator.Generate(
            totalCount: count,
            smbhMass: 400.0,
            bulgeMass: 180.0, bulgeScale: 0.5,
            diskMass: 300.0, diskRadialScale: 3.5, diskVerticalScale: 0.12,
            haloMass: 1000.0, haloScale: 12.0,
            spiralPitchDeg: 14.0,  // Tight winding (grand design)
            spiralArmCount: 2,
            spiralStrength: 0.7,   // Strong arm contrast
            dustFraction: 0.12,
            hiiFraction: 0.08,     // Many H-II regions
            youngStarFraction: 0.10,
            bulgeFraction: 0.14,
            haloFraction: 0.10,
            inclinationDeg: 30.0,  // Moderately inclined
            seed: seed);
    }

    /// <summary>
    /// NGC 1300 barred spiral (user image 2, opo0501a).
    /// Barred spiral galaxy with elongated central bar and sweeping outer arms.
    ///
    /// Observed features to replicate:
    /// - Strong central bar with brown dust lanes along the bar
    /// - Two spiral arms emerging from bar ends
    /// - Blue young stars in outer arm segments
    /// - Reddish-brown dust lanes throughout
    /// </summary>
    public static PhysicsBody[] BarredSpiral(int count = 50000, int seed = 42)
    {
        return MiyamotoNagaiDiskGenerator.Generate(
            totalCount: count,
            smbhMass: 350.0,
            bulgeMass: 160.0, bulgeScale: 0.35,
            diskMass: 280.0, diskRadialScale: 3.0, diskVerticalScale: 0.10,
            haloMass: 900.0, haloScale: 11.0,
            spiralPitchDeg: 25.0,  // Looser winding for barred spirals
            spiralArmCount: 2,
            spiralStrength: 0.6,
            barStrength: 0.5,      // Strong bar perturbation (NGC 1300 signature)
            barLength: 2.5,
            dustFraction: 0.15,    // Heavy dust in bar
            hiiFraction: 0.05,
            youngStarFraction: 0.10,
            bulgeFraction: 0.12,
            haloFraction: 0.10,
            inclinationDeg: 25.0,
            seed: seed);
    }

    /// <summary>
    /// Andromeda-Milky Way future collision scenario.
    /// Two large spiral galaxies on an oblique approach trajectory.
    /// </summary>
    public static PhysicsBody[] AndromedaMilkyWayCollision(
        int mwCount = 30000, int andCount = 30000, int seed = 42)
    {
        var milkyWay = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: mwCount,
            smbhMass: 400.0,
            bulgeMass: 150.0, bulgeScale: 0.4,
            diskMass: 280.0, diskRadialScale: 3.5, diskVerticalScale: 0.12,
            haloMass: 1000.0, haloScale: 14.0,
            spiralPitchDeg: 14.0, spiralArmCount: 4, spiralStrength: 0.5,
            barStrength: 0.15,  // Mild bar (MW has a weak bar)
            barLength: 1.5,
            dustFraction: 0.12, hiiFraction: 0.05, youngStarFraction: 0.08,
            bulgeFraction: 0.12, haloFraction: 0.10,
            centerPosition: new Vec3d(-8, 0, 0),
            centerVelocity: new Vec3d(0.5, 0.1, 0),
            seed: seed);

        var andromeda = MiyamotoNagaiDiskGenerator.Generate(
            totalCount: andCount,
            smbhMass: 500.0,
            bulgeMass: 250.0, bulgeScale: 0.6,
            diskMass: 350.0, diskRadialScale: 4.0, diskVerticalScale: 0.14,
            haloMass: 1500.0, haloScale: 16.0,
            spiralPitchDeg: 12.0, spiralArmCount: 2, spiralStrength: 0.55,
            dustFraction: 0.14, hiiFraction: 0.06, youngStarFraction: 0.10,
            bulgeFraction: 0.16, haloFraction: 0.12,
            centerPosition: new Vec3d(8, 2, 0),
            centerVelocity: new Vec3d(-0.5, -0.1, 0),
            inclinationDeg: 77.0,
            positionAngleDeg: 35.0,
            seed: seed + 1);

        return MergeBodies(milkyWay, andromeda);
    }

    // ── Helper ────────────────────────────────────────────────────────────

    /// <summary>
    /// Merge two PhysicsBody arrays, re-indexing IDs to be globally unique.
    /// </summary>
    private static PhysicsBody[] MergeBodies(PhysicsBody[] a, PhysicsBody[] b)
    {
        var result = new PhysicsBody[a.Length + b.Length];
        Array.Copy(a, 0, result, 0, a.Length);

        int offset = a.Length;
        for (int i = 0; i < b.Length; i++)
        {
            var body = b[i];
            body.Id = offset + i;
            result[offset + i] = body;
        }

        return result;
    }
}
