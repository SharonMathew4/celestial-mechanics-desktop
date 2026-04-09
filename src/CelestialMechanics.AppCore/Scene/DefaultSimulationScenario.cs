using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.AppCore.Scene;

/// <summary>
/// Provides reusable default simulation scenes shared by multiple application entry points.
/// </summary>
public static class DefaultSimulationScenario
{
    /// <summary>
    /// Creates the same working two-body orbit used by the standalone simulation app.
    /// </summary>
    public static PhysicsBody[] CreateTwoBodyOrbit()
    {
        double mass = 1.0;
        double separation = 2.0;
        double radiusFromOrigin = separation / 2.0;
        double orbitalVelocity = System.Math.Sqrt(PhysicalConstants.G_Sim * mass / (4.0 * radiusFromOrigin));

        return
        [
            new PhysicsBody(
                0,
                mass,
                new Vec3d(radiusFromOrigin, 0, 0),
                new Vec3d(0, 0, orbitalVelocity),
                BodyType.Star)
            {
                Radius = 0.05,
                GravityStrength = 60,
                GravityRange = 8,
                IsActive = true,
            },

            new PhysicsBody(
                1,
                mass,
                new Vec3d(-radiusFromOrigin, 0, 0),
                new Vec3d(0, 0, -orbitalVelocity),
                BodyType.Star)
            {
                Radius = 0.05,
                GravityStrength = 60,
                GravityRange = 8,
                IsActive = true,
            },
        ];
    }
}
