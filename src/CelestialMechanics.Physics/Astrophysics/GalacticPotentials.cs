using CelestialMechanics.Math;

namespace CelestialMechanics.Physics.Astrophysics;

/// <summary>
/// Composite galactic gravitational potentials for realistic galaxy
/// initial condition generation.
///
/// PHYSICS MODELS
/// ──────────────
///
/// 1. MIYAMOTO-NAGAI POTENTIAL (Stellar Disk)
///    Φ_MN(R,z) = -G·M / √(R² + (a + √(z² + b²))²)
///    where a = radial scale length, b = vertical scale length
///    Reproduces the flattened disk visible in Sombrero (M104) and
///    edge-on views. Derived from ESA/Hubble heic0206a imagery.
///
/// 2. HERNQUIST PROFILE (Bulge + Dark Matter Halo)
///    Φ_H(r) = -G·M / (r + a)
///    Models the smooth, luminous bulge seen in NGC 4622 (potw1811a)
///    and the golden core of M81/M106 (heic1302a, heic1305a).
///
/// 3. RADIAL DENSITY WAVE (Cartwheel-style ring formation)
///    δr ∝ A · sin(k·r - ω·t) · exp(-γ·t)
///    Triggered by head-on impact. Reproduces the concentric ring
///    structure and radial spoke features of the Cartwheel Galaxy.
///
/// 4. CHANDRASEKHAR DYNAMICAL FRICTION
///    f_df = -4π G² M ρ ln(Λ) · v / v³
///    Produces asymmetric tidal tails (Tadpole heic0206a) and
///    tidal bridges (Mice Galaxies NGC 4676).
///
/// REFERENCES
/// ──────────
/// - Miyamoto & Nagai (1975), PASJ 27, 533
/// - Hernquist (1990), ApJ 356, 359
/// - Lynds & Toomre (1976), ApJ 209, 382 (ring galaxies)
/// - Chandrasekhar (1943), ApJ 97, 255
/// </summary>
public static class GalacticPotentials
{
    // ── Miyamoto-Nagai Disk Potential ───────────────────────────────────────

    /// <summary>
    /// Miyamoto-Nagai potential at cylindrical coordinates (R, z).
    /// Φ = -G·M / √(R² + (a + √(z² + b²))²)
    /// </summary>
    /// <param name="R">Cylindrical radius (in XZ plane).</param>
    /// <param name="z">Height above disk midplane (Y axis).</param>
    /// <param name="mass">Total disk mass.</param>
    /// <param name="a">Radial scale length (controls disk extent).</param>
    /// <param name="b">Vertical scale height (controls disk thickness).</param>
    /// <returns>Gravitational potential (negative).</returns>
    public static double MiyamotoNagaiPotential(double R, double z,
        double mass, double a, double b)
    {
        double zTerm = System.Math.Sqrt(z * z + b * b);
        double denom = System.Math.Sqrt(R * R + (a + zTerm) * (a + zTerm));
        return -PhysicalConstants.G_Sim * mass / denom;
    }

    /// <summary>
    /// Radial force (∂Φ/∂R) from Miyamoto-Nagai potential.
    /// Used for computing circular velocity: v_c = √(R · |∂Φ/∂R|)
    /// </summary>
    public static double MiyamotoNagaiRadialForce(double R, double z,
        double mass, double a, double b)
    {
        double zTerm = System.Math.Sqrt(z * z + b * b);
        double s2 = R * R + (a + zTerm) * (a + zTerm);
        double s = System.Math.Sqrt(s2);
        // -∂Φ/∂R = G·M·R / (R² + (a + √(z²+b²))²)^(3/2)
        return PhysicalConstants.G_Sim * mass * R / (s * s2);
    }

    /// <summary>
    /// Enclosed mass approximation for the Miyamoto-Nagai disk at radius R
    /// (evaluated in the midplane z=0).
    ///
    /// For a Miyamoto-Nagai disk, the enclosed mass can be approximated by
    /// matching the circular velocity: M_enc(R) = R · v_c²(R) / G
    /// where v_c² = R · |∂Φ/∂R|.
    /// </summary>
    public static double MiyamotoNagaiEnclosedMass(double R, double mass,
        double a, double b)
    {
        if (R <= 0.0) return 0.0;

        // v_c²(R) = G·M·R² / (R² + (a+b)²)^(3/2) at z=0
        double ab = a + b;
        double s2 = R * R + ab * ab;
        double s = System.Math.Sqrt(s2);
        double vc2 = PhysicalConstants.G_Sim * mass * R * R / (s * s2);
        return vc2 * R / PhysicalConstants.G_Sim;
    }

    // ── Hernquist Profile (Bulge / Halo) ──────────────────────────────────

    /// <summary>
    /// Hernquist potential: Φ(r) = -G·M / (r + a)
    /// Models smooth, spherically symmetric components (bulge, halo).
    /// </summary>
    public static double HernquistPotential(double r, double mass, double a)
    {
        return -PhysicalConstants.G_Sim * mass / (r + a);
    }

    /// <summary>
    /// Hernquist enclosed mass: M(r) = M · r² / (r + a)²
    /// </summary>
    public static double HernquistEnclosedMass(double r, double mass, double a)
    {
        if (r <= 0.0) return 0.0;
        double rpa = r + a;
        return mass * r * r / (rpa * rpa);
    }

    /// <summary>
    /// Radial force from Hernquist profile: |∂Φ/∂r| = G·M / (r+a)²
    /// </summary>
    public static double HernquistRadialForce(double r, double mass, double a)
    {
        double rpa = r + a;
        return PhysicalConstants.G_Sim * mass / (rpa * rpa);
    }

    // ── Composite Circular Velocity ─────────────────────────────────────

    /// <summary>
    /// Circular velocity at radius R from a composite
    /// Miyamoto-Nagai disk + Hernquist bulge + Hernquist halo.
    ///
    /// v_c(R) = √(R · Σ|∂Φ_i/∂R|)
    ///
    /// This produces the observed flat rotation curve: rising in the
    /// bulge-dominated region, flat in the disk+halo region (matching
    /// NGC 1300, M81, M106 imagery).
    /// </summary>
    public static double CompositeCircularVelocity(double R,
        double diskMass, double diskA, double diskB,
        double bulgeMass, double bulgeA,
        double haloMass, double haloA)
    {
        if (R <= 1e-12) return 0.0;

        double fDisk = MiyamotoNagaiRadialForce(R, 0.0, diskMass, diskA, diskB);
        double fBulge = HernquistRadialForce(R, bulgeMass, bulgeA);
        double fHalo = HernquistRadialForce(R, haloMass, haloA);

        double totalForce = fDisk + fBulge + fHalo;
        return System.Math.Sqrt(R * totalForce);
    }

    /// <summary>
    /// Total enclosed mass at radius R from composite model (z=0 midplane).
    /// </summary>
    public static double CompositeEnclosedMass(double R,
        double diskMass, double diskA, double diskB,
        double bulgeMass, double bulgeA,
        double haloMass, double haloA)
    {
        double mDisk = MiyamotoNagaiEnclosedMass(R, diskMass, diskA, diskB);
        double mBulge = HernquistEnclosedMass(R, bulgeMass, bulgeA);
        double mHalo = HernquistEnclosedMass(R, haloMass, haloA);
        return mDisk + mBulge + mHalo;
    }

    // ── Radial Density Wave ─────────────────────────────────────────────

    /// <summary>
    /// Compute radial displacement from a density wave triggered by
    /// a head-on collision (Cartwheel Galaxy morphology).
    ///
    /// δr = A · sin(k·r - ω·t) · exp(-γ·t)
    ///
    /// When combined with the base circular orbit, this produces
    /// concentric ring features and radial spoke structures.
    /// </summary>
    /// <param name="r">Current radius from center.</param>
    /// <param name="t">Time since collision trigger.</param>
    /// <param name="amplitude">Wave amplitude A.</param>
    /// <param name="wavenumber">Radial wavenumber k.</param>
    /// <param name="angularFrequency">Wave angular frequency ω.</param>
    /// <param name="damping">Exponential damping rate γ.</param>
    /// <returns>Radial displacement δr.</returns>
    public static double RadialDensityWaveDisplacement(double r, double t,
        double amplitude, double wavenumber,
        double angularFrequency, double damping)
    {
        return amplitude * System.Math.Sin(wavenumber * r - angularFrequency * t)
               * System.Math.Exp(-damping * t);
    }

    // ── Chandrasekhar Dynamical Friction ────────────────────────────────

    /// <summary>
    /// Chandrasekhar dynamical friction acceleration magnitude.
    ///
    /// |a_df| = 4π G² M ρ ln(Λ) / v²
    ///
    /// The friction force is directed anti-parallel to the velocity vector.
    /// This deceleration drives orbital decay and produces the asymmetric
    /// tidal tails observed in the Tadpole Galaxy (heic0206a).
    /// </summary>
    /// <param name="objectMass">Mass of the body experiencing friction M.</param>
    /// <param name="localDensity">Background stellar density ρ at the body's location.</param>
    /// <param name="velocity">Speed |v| of the body relative to the background.</param>
    /// <param name="coulombLog">Coulomb logarithm ln(Λ), typically 3–10.</param>
    /// <returns>Magnitude of the friction deceleration (always positive).</returns>
    public static double DynamicalFrictionAcceleration(double objectMass,
        double localDensity, double velocity, double coulombLog)
    {
        if (velocity <= 1e-12 || localDensity <= 0.0)
            return 0.0;

        double g2 = PhysicalConstants.G_Sim * PhysicalConstants.G_Sim;
        return 4.0 * System.Math.PI * g2 * objectMass * localDensity
               * coulombLog / (velocity * velocity);
    }

    // ── Spiral Arm Perturbation ─────────────────────────────────────────

    /// <summary>
    /// Logarithmic spiral angle perturbation for generating spiral arm structure.
    /// The azimuthal offset follows: θ_spiral = (1/tan(pitch)) · ln(R/R0)
    ///
    /// Combined with a cosine modulation over m arms, this produces the
    /// grand-design spiral patterns observed in M81 and NGC 1073 (heic0706a).
    /// </summary>
    /// <param name="R">Cylindrical radius.</param>
    /// <param name="pitchAngleDeg">Spiral arm pitch angle in degrees (M81 ≈ 14°, NGC 1300 ≈ 25°).</param>
    /// <param name="armCount">Number of spiral arms (2 for grand design, 4 for flocculent).</param>
    /// <param name="R0">Reference radius for the spiral pattern.</param>
    /// <param name="azimuth">Current azimuthal angle in radians.</param>
    /// <param name="strength">Density enhancement factor (0 = no arms, 1 = strong).</param>
    /// <returns>Spiral arm density weight (1.0 = on arm, lower = inter-arm).</returns>
    public static double SpiralArmDensityWeight(double R, double pitchAngleDeg,
        int armCount, double R0, double azimuth, double strength)
    {
        if (R <= 1e-12 || armCount <= 0) return 1.0;

        double pitchRad = pitchAngleDeg * System.Math.PI / 180.0;
        double tanPitch = System.Math.Tan(pitchRad);
        if (System.Math.Abs(tanPitch) < 1e-12) return 1.0;

        double spiralPhase = armCount * (azimuth - (1.0 / tanPitch) * System.Math.Log(R / System.Math.Max(R0, 1e-6)));
        double armModulation = 0.5 * (1.0 + System.Math.Cos(spiralPhase));
        return 1.0 + strength * armModulation;
    }

    /// <summary>
    /// Bar perturbation strength for barred spiral galaxies (NGC 1300).
    /// Returns a radial-azimuthal density weight modulated by a cos(2θ) pattern
    /// with exponential radial taper.
    /// </summary>
    public static double BarPerturbationWeight(double R, double azimuth,
        double barLength, double barStrength)
    {
        if (barStrength <= 0.0 || barLength <= 0.0) return 1.0;

        double radialTaper = System.Math.Exp(-R / barLength);
        double azimuthalMode = 0.5 * (1.0 + System.Math.Cos(2.0 * azimuth));
        return 1.0 + barStrength * radialTaper * azimuthalMode;
    }
}
