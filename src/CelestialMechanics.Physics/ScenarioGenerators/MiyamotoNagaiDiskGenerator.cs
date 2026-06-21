using CelestialMechanics.Math;
using CelestialMechanics.Physics.Astrophysics;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Physics.ScenarioGenerators;

/// <summary>
/// Generates galaxy initial conditions using a composite Miyamoto-Nagai disk +
/// Hernquist bulge + Hernquist dark matter halo model in virial equilibrium.
///
/// COMPONENT STRUCTURE (inverse-engineered from JWST/Hubble reference imagery)
/// ─────────────────────────────────────────────────────────────────────────────
///
/// 1. SUPERMASSIVE BLACK HOLE — single massive body at center (BodyType.BlackHole)
///
/// 2. STELLAR BULGE — Hernquist profile, golden/warm hue (BodyType.GalaxyBulgeParticle)
///    Reference: NGC 4622 golden core (potw1811a), M77 bright nucleus (heic1305a)
///
/// 3. STELLAR DISK — Miyamoto-Nagai profile (BodyType.GalaxyDiskParticle)
///    - Logarithmic spiral arm modulation (M81, NGC 1073 heic0706a)
///    - Optional bar perturbation (NGC 1300 opo0501a)
///    - Subset tagged DustCloud along spiral arms (brown lanes in M106 heic1302a)
///    - Subset tagged HIIRegion at high-density arm nodes (pink knots in M81)
///    - Subset tagged YoungStarCluster at arm leading edges (blue regions in NGC 1792 potw2049a)
///
/// 4. DARK MATTER HALO — Hernquist profile (BodyType.GalaxyHaloParticle)
///    Low-mass particles, faint/invisible, provide flat rotation curve.
///
/// VIRIAL EQUILIBRIUM
/// ──────────────────
/// Each particle's circular velocity is computed from the composite enclosed
/// mass at its radius: v_c(R) = √(R · Σ|∂Φ_i/∂R|).
/// Velocity dispersion is added using Eddington-formula-inspired anisotropic
/// thermal scatter: σ_R ∝ 0.3·v_c, σ_z ∝ 0.15·v_c.
/// </summary>
public static class MiyamotoNagaiDiskGenerator
{
    /// <summary>
    /// Generate a full galaxy with composite potential.
    /// </summary>
    /// <param name="totalCount">Total particle count (including SMBH).</param>
    /// <param name="smbhMass">Supermassive black hole mass.</param>
    /// <param name="bulgeMass">Bulge total mass.</param>
    /// <param name="bulgeScale">Hernquist scale length for bulge.</param>
    /// <param name="diskMass">Disk total mass.</param>
    /// <param name="diskRadialScale">Miyamoto-Nagai radial scale a.</param>
    /// <param name="diskVerticalScale">Miyamoto-Nagai vertical scale b.</param>
    /// <param name="haloMass">Dark matter halo mass.</param>
    /// <param name="haloScale">Hernquist scale length for halo.</param>
    /// <param name="spiralPitchDeg">Spiral arm pitch angle in degrees.</param>
    /// <param name="spiralArmCount">Number of spiral arms.</param>
    /// <param name="spiralStrength">Spiral arm density contrast (0-1).</param>
    /// <param name="barStrength">Bar perturbation strength (0 = no bar).</param>
    /// <param name="barLength">Bar half-length in sim units.</param>
    /// <param name="dustFraction">Fraction of disk particles tagged as DustCloud.</param>
    /// <param name="hiiFraction">Fraction tagged as HIIRegion at density peaks.</param>
    /// <param name="youngStarFraction">Fraction tagged as YoungStarCluster.</param>
    /// <param name="bulgeFraction">Fraction of non-SMBH particles in bulge.</param>
    /// <param name="haloFraction">Fraction of non-SMBH particles in halo.</param>
    /// <param name="centerPosition">Galaxy center position.</param>
    /// <param name="centerVelocity">Galaxy bulk velocity.</param>
    /// <param name="inclinationDeg">Disk inclination angle (0 = face-on).</param>
    /// <param name="positionAngleDeg">Position angle of the disk major axis.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    public static PhysicsBody[] Generate(
        int totalCount = 50000,
        double smbhMass = 500.0,
        double bulgeMass = 200.0, double bulgeScale = 0.5,
        double diskMass = 300.0, double diskRadialScale = 3.0, double diskVerticalScale = 0.15,
        double haloMass = 1000.0, double haloScale = 10.0,
        double spiralPitchDeg = 14.0, int spiralArmCount = 2, double spiralStrength = 0.6,
        double barStrength = 0.0, double barLength = 1.5,
        double dustFraction = 0.12, double hiiFraction = 0.05, double youngStarFraction = 0.08,
        double bulgeFraction = 0.15, double haloFraction = 0.10,
        Vec3d centerPosition = default, Vec3d centerVelocity = default,
        double inclinationDeg = 0.0, double positionAngleDeg = 0.0,
        int seed = 42)
    {
        var rng = seed >= 0 ? new Random(seed) : new Random();
        var bodies = new PhysicsBody[totalCount];

        // Compute component counts
        int nonSmbh = totalCount - 1;
        int bulgeCount = System.Math.Max(1, (int)(nonSmbh * bulgeFraction));
        int haloCount = System.Math.Max(1, (int)(nonSmbh * haloFraction));
        int diskCount = nonSmbh - bulgeCount - haloCount;

        // Mass per particle in each component
        double bulgeMassPerParticle = bulgeMass / System.Math.Max(1, bulgeCount);
        double diskMassPerParticle = diskMass / System.Math.Max(1, diskCount);
        double haloMassPerParticle = haloMass / System.Math.Max(1, haloCount);

        // Rotation matrix for inclination and position angle
        double incRad = inclinationDeg * System.Math.PI / 180.0;
        double paRad = positionAngleDeg * System.Math.PI / 180.0;

        int idx = 0;

        // ── SMBH (body 0) ─────────────────────────────────────────────────
        bodies[idx++] = new PhysicsBody(0, smbhMass,
            centerPosition, centerVelocity, BodyType.BlackHole)
        {
            IsActive = true,
            GravityStrength = 60,
            GravityRange = 0
        };

        // ── Bulge particles ───────────────────────────────────────────────
        for (int i = 0; i < bulgeCount && idx < totalCount; i++, idx++)
        {
            double r = SampleHernquist(rng, bulgeScale);

            // Isotropic direction
            double cosTheta = 2.0 * rng.NextDouble() - 1.0;
            double sinTheta = System.Math.Sqrt(1.0 - cosTheta * cosTheta);
            double phi = 2.0 * System.Math.PI * rng.NextDouble();

            Vec3d localPos = new(
                r * sinTheta * System.Math.Cos(phi),
                r * cosTheta,
                r * sinTheta * System.Math.Sin(phi));

            // Isotropic velocity dispersion from virial: σ ≈ 0.35 * v_c(r)
            double vc = GalacticPotentials.CompositeCircularVelocity(r,
                diskMass, diskRadialScale, diskVerticalScale,
                bulgeMass, bulgeScale, haloMass, haloScale);
            double sigma = 0.35 * vc;

            Vec3d localVel = new(
                sigma * GaussianRandom(rng),
                sigma * GaussianRandom(rng),
                sigma * GaussianRandom(rng));

            Vec3d pos = RotateDisk(localPos, incRad, paRad) + centerPosition;
            Vec3d vel = RotateDisk(localVel, incRad, paRad) + centerVelocity;

            bodies[idx] = new PhysicsBody(idx, bulgeMassPerParticle,
                pos, vel, BodyType.GalaxyBulgeParticle)
            {
                IsActive = true,
                Radius = 0.015,
                GravityStrength = 60,
                GravityRange = 0
            };
        }

        // ── Disk particles ────────────────────────────────────────────────
        int diskStart = idx;
        for (int i = 0; i < diskCount && idx < totalCount; i++, idx++)
        {
            // Radial position: exponential disk via inverse CDF
            double u = rng.NextDouble();
            u = System.Math.Max(u, 1e-10);
            u = System.Math.Min(u, 1.0 - 1e-10);
            double R = -diskRadialScale * System.Math.Log(1.0 - u);

            // Azimuthal angle with spiral arm modulation
            double baseAngle = 2.0 * System.Math.PI * rng.NextDouble();

            // Apply spiral arm density weighting via rejection sampling
            double spiralWeight = GalacticPotentials.SpiralArmDensityWeight(
                R, spiralPitchDeg, spiralArmCount, diskRadialScale,
                baseAngle, spiralStrength);
            double barWeight = GalacticPotentials.BarPerturbationWeight(
                R, baseAngle, barLength, barStrength);
            double combinedWeight = spiralWeight * barWeight;

            // Rejection: accept with probability proportional to weight
            double maxWeight = (1.0 + spiralStrength) * (1.0 + barStrength);
            if (rng.NextDouble() * maxWeight > combinedWeight)
            {
                // Rejected — but still create the particle (just with random angle)
                baseAngle = 2.0 * System.Math.PI * rng.NextDouble();
            }

            // Vertical position: sech² distribution for thin disk
            double zRaw = diskVerticalScale * System.Math.Log(
                rng.NextDouble() / System.Math.Max(1.0 - rng.NextDouble(), 1e-10));
            zRaw = System.Math.Clamp(zRaw, -diskVerticalScale * 4.0, diskVerticalScale * 4.0);

            double px = R * System.Math.Cos(baseAngle);
            double pz = R * System.Math.Sin(baseAngle);

            // Circular velocity from composite potential
            double vc = GalacticPotentials.CompositeCircularVelocity(R,
                diskMass, diskRadialScale, diskVerticalScale,
                bulgeMass, bulgeScale, haloMass, haloScale);

            // Tangential velocity (counter-clockwise) + velocity dispersion
            double sigmaR = 0.12 * vc;
            double sigmaZ = 0.06 * vc;

            double vx = -vc * System.Math.Sin(baseAngle) + sigmaR * GaussianRandom(rng);
            double vz = vc * System.Math.Cos(baseAngle) + sigmaR * GaussianRandom(rng);
            double vy = sigmaZ * GaussianRandom(rng);

            Vec3d localPos = new(px, zRaw, pz);
            Vec3d localVel = new(vx, vy, vz);

            Vec3d pos = RotateDisk(localPos, incRad, paRad) + centerPosition;
            Vec3d vel = RotateDisk(localVel, incRad, paRad) + centerVelocity;

            // Determine sub-type based on position and density
            BodyType diskType = BodyType.GalaxyDiskParticle;
            double relativeIdx = (double)i / diskCount;

            // Dust clouds along inner edges of spiral arms
            bool isOnArm = combinedWeight > (1.0 + 0.5 * spiralStrength);
            if (isOnArm && relativeIdx < dustFraction && R > 0.5 * diskRadialScale)
            {
                diskType = BodyType.DustCloud;
            }
            // H-II regions at arm density peaks (pink knots like in M106/M81)
            else if (isOnArm && combinedWeight > (1.0 + 0.8 * spiralStrength)
                     && relativeIdx >= dustFraction
                     && relativeIdx < dustFraction + hiiFraction)
            {
                diskType = BodyType.HIIRegion;
            }
            // Young star clusters at outer arm edges (blue regions like NGC 1792)
            else if (isOnArm && R > 1.5 * diskRadialScale
                     && relativeIdx >= dustFraction + hiiFraction
                     && relativeIdx < dustFraction + hiiFraction + youngStarFraction)
            {
                diskType = BodyType.YoungStarCluster;
            }

            bodies[idx] = new PhysicsBody(idx, diskMassPerParticle,
                pos, vel, diskType)
            {
                IsActive = true,
                Radius = diskType == BodyType.DustCloud ? 0.025 : 0.012,
                GravityStrength = 60,
                GravityRange = 0
            };
        }

        // ── Halo particles ────────────────────────────────────────────────
        for (int i = 0; i < haloCount && idx < totalCount; i++, idx++)
        {
            double r = SampleHernquist(rng, haloScale);

            double cosTheta = 2.0 * rng.NextDouble() - 1.0;
            double sinTheta = System.Math.Sqrt(1.0 - cosTheta * cosTheta);
            double phi = 2.0 * System.Math.PI * rng.NextDouble();

            Vec3d localPos = new(
                r * sinTheta * System.Math.Cos(phi),
                r * cosTheta,
                r * sinTheta * System.Math.Sin(phi));

            // Velocity dispersion from virial theorem
            double vc = GalacticPotentials.CompositeCircularVelocity(
                System.Math.Min(r, haloScale * 5.0),
                diskMass, diskRadialScale, diskVerticalScale,
                bulgeMass, bulgeScale, haloMass, haloScale);
            double sigma = 0.3 * vc;

            Vec3d localVel = new(
                sigma * GaussianRandom(rng),
                sigma * GaussianRandom(rng),
                sigma * GaussianRandom(rng));

            Vec3d pos = localPos + centerPosition;
            Vec3d vel = localVel + centerVelocity;

            bodies[idx] = new PhysicsBody(idx, haloMassPerParticle,
                pos, vel, BodyType.GalaxyHaloParticle)
            {
                IsActive = true,
                Radius = 0.008,
                GravityStrength = 60,
                GravityRange = 0
            };
        }

        return bodies;
    }

    // ── Sampling Helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Sample radius from Hernquist profile via inverse CDF.
    /// CDF: P(r) = r² / (r + a)²
    /// Inverse: r = a · √U / (1 - √U)
    /// </summary>
    private static double SampleHernquist(Random rng, double scaleLength)
    {
        double u = rng.NextDouble();
        u = System.Math.Max(u, 1e-10);
        u = System.Math.Min(u, 1.0 - 1e-10);
        double sqrtU = System.Math.Sqrt(u);
        return scaleLength * sqrtU / (1.0 - sqrtU);
    }

    /// <summary>
    /// Box-Muller Gaussian random number (mean=0, σ=1).
    /// </summary>
    private static double GaussianRandom(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = rng.NextDouble();
        return System.Math.Sqrt(-2.0 * System.Math.Log(u1))
               * System.Math.Cos(2.0 * System.Math.PI * u2);
    }

    /// <summary>
    /// Rotate a vector from disk-plane coordinates (Y = up) to
    /// inclined + position-angle coordinates.
    /// </summary>
    private static Vec3d RotateDisk(Vec3d v, double incRad, double paRad)
    {
        // First: rotate about X by inclination (tilts disk)
        double cosI = System.Math.Cos(incRad);
        double sinI = System.Math.Sin(incRad);
        double y1 = v.Y * cosI - v.Z * sinI;
        double z1 = v.Y * sinI + v.Z * cosI;

        // Second: rotate about Y by position angle
        double cosPA = System.Math.Cos(paRad);
        double sinPA = System.Math.Sin(paRad);
        double x2 = v.X * cosPA + z1 * sinPA;
        double z2 = -v.X * sinPA + z1 * cosPA;

        return new Vec3d(x2, y1, z2);
    }
}
