using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Services;

/// <summary>
/// Creates starter scenes for new projects.
/// Units: mass in solar masses, distance in AU.
/// Circular orbit velocity: v = 2π / √r  (with G·M_sun = 4π²).
/// </summary>
public static class DefaultSceneFactory
{
    /// <summary>
    /// Creates a starter solar system with Sun + 3 planets.
    /// </summary>
    public static PhysicsBody[] CreateSolarSystem()
    {
        return new[]
        {
            // ── Sun at origin ──────────────────────────────────
            new PhysicsBody(1, 1.0, Vec3d.Zero, Vec3d.Zero, BodyType.Star)
            {
                Radius = 0.05,
                IsActive = true,
                IsCollidable = true,
                GravityStrength = 60,
                GravityRange = 100,
            },

            // ── Earth-like planet at 1 AU ─────────────────────
            // v = 2π / √1 = 6.2832
            new PhysicsBody(2, 3.003e-6, new Vec3d(1, 0, 0), new Vec3d(0, 0, 6.2832), BodyType.Planet)
            {
                Radius = 0.015,
                IsActive = true,
                IsCollidable = true,
                GravityStrength = 60,
                GravityRange = 8,
            },

            // ── Jupiter-like gas giant at 5.2 AU ──────────────
            // v = 2π / √5.2 ≈ 2.755
            new PhysicsBody(3, 9.545e-4, new Vec3d(5.2, 0, 0), new Vec3d(0, 0, 2.755), BodyType.GasGiant)
            {
                Radius = 0.035,
                IsActive = true,
                IsCollidable = true,
                GravityStrength = 60,
                GravityRange = 20,
            },

            // ── Mars-like rocky planet at 1.524 AU ────────────
            // v = 2π / √1.524 ≈ 5.089
            new PhysicsBody(4, 3.213e-7, new Vec3d(1.524, 0, 0), new Vec3d(0, 0, 5.089), BodyType.RockyPlanet)
            {
                Radius = 0.01,
                IsActive = true,
                IsCollidable = true,
                GravityStrength = 60,
                GravityRange = 8,
            },
        };
    }
}
