using CelestialMechanics.Math;
using CelestialMechanics.Physics.Astrophysics;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Physics.Forces;

/// <summary>
/// Chandrasekhar dynamical friction force for galaxy simulations.
///
/// PHYSICS
/// ───────
/// A massive body moving through a field of lighter particles experiences
/// a gravitational drag force proportional to:
///
///   F_df = -4π G² M² ρ ln(Λ) · v / v³
///
/// This force produces realistic tidal bridges (Tadpole Galaxy heic0206a),
/// tidal tails (Mice Galaxies NGC 4676), and merger orbital decay.
///
/// IMPLEMENTATION
/// ──────────────
/// Implements IForceCalculator as a pairwise interaction. When body A is
/// massive (mass > MinMassForFriction) and body B is lighter, body A
/// experiences a drag force proportional to B's contribution to the
/// local stellar density. The force is anti-parallel to A's velocity
/// relative to B.
/// </summary>
public class DynamicalFrictionForce : IForceCalculator
{
    /// <summary>Coulomb logarithm ln(Λ). Range: 3–10.</summary>
    public double CoulombLogarithm { get; set; } = 3.0;

    /// <summary>Minimum body mass to experience dynamical friction.</summary>
    public double MinMassForFriction { get; set; } = 10.0;

    /// <summary>Smoothing radius for density contribution.</summary>
    public double SmoothingRadius { get; set; } = 2.0;

    /// <summary>Maximum friction force magnitude per pair to prevent instability.</summary>
    public double MaxFrictionForce { get; set; } = 5.0;

    public string Name => "DynamicalFriction";
    public bool Enabled { get; set; } = true;

    public Vec3d ComputeForce(in PhysicsBody a, in PhysicsBody b)
    {
        if (!Enabled) return Vec3d.Zero;

        // Only apply friction to massive bodies (galaxy cores, SMBHs)
        if (a.Mass < MinMassForFriction)
            return Vec3d.Zero;

        // Distance check — only nearby bodies contribute to friction
        double dx = b.Position.X - a.Position.X;
        double dy = b.Position.Y - a.Position.Y;
        double dz = b.Position.Z - a.Position.Z;
        double r2 = dx * dx + dy * dy + dz * dz;

        if (r2 > SmoothingRadius * SmoothingRadius || r2 < 1e-20)
            return Vec3d.Zero;

        // Relative velocity of A with respect to the background (B)
        double vRelX = a.Velocity.X - b.Velocity.X;
        double vRelY = a.Velocity.Y - b.Velocity.Y;
        double vRelZ = a.Velocity.Z - b.Velocity.Z;
        double vRel = System.Math.Sqrt(vRelX * vRelX + vRelY * vRelY + vRelZ * vRelZ);

        if (vRel < 1e-12)
            return Vec3d.Zero;

        // Local density contribution from body B within smoothing volume
        double volume = (4.0 / 3.0) * System.Math.PI
                        * SmoothingRadius * SmoothingRadius * SmoothingRadius;
        double densityContribution = b.Mass / System.Math.Max(volume, 1e-20);

        // Chandrasekhar friction: F = -4π G² M ρ ln(Λ) v / v³
        double g2 = PhysicalConstants.G_Sim * PhysicalConstants.G_Sim;
        double frictionMag = 4.0 * System.Math.PI * g2 * a.Mass
                             * densityContribution * CoulombLogarithm / (vRel * vRel);

        // Clamp to prevent numerical instability
        frictionMag = System.Math.Min(frictionMag, MaxFrictionForce);

        // Anti-parallel to relative velocity
        double factor = -frictionMag / vRel;
        return new Vec3d(factor * vRelX, factor * vRelY, factor * vRelZ);
    }

    public double ComputePotentialEnergy(in PhysicsBody a, in PhysicsBody b)
    {
        // Dynamical friction is dissipative; no conservative potential energy
        return 0.0;
    }
}
