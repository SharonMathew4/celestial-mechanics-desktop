using System.Text.Json;
using CelestialMechanics.Math;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Physics.ScenarioGenerators;

/// <summary>
/// Loads galaxy scenarios from JSON configuration and instantiates
/// PhysicsBody arrays using the MiyamotoNagaiDiskGenerator.
/// </summary>
public static class GalaxyScenarioLoader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Load a galaxy scenario from a JSON string.
    /// </summary>
    public static (PhysicsBody[] bodies, GalaxyScenarioConfig config) LoadFromJson(string json)
    {
        var config = JsonSerializer.Deserialize<GalaxyScenarioConfig>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize galaxy scenario config.");

        var bodies = GenerateFromConfig(config);
        return (bodies, config);
    }

    /// <summary>
    /// Load a galaxy scenario from a JSON file.
    /// </summary>
    public static (PhysicsBody[] bodies, GalaxyScenarioConfig config) LoadFromFile(string path)
    {
        string json = File.ReadAllText(path);
        return LoadFromJson(json);
    }

    /// <summary>
    /// Generate PhysicsBody[] from a GalaxyScenarioConfig.
    /// Supports 1 or 2 galaxies (single galaxy or collision scenario).
    /// </summary>
    public static PhysicsBody[] GenerateFromConfig(GalaxyScenarioConfig config)
    {
        if (config.Galaxies == null || config.Galaxies.Length == 0)
            throw new ArgumentException("Scenario config must have at least one galaxy definition.");

        var allBodies = new List<PhysicsBody>();
        int idOffset = 0;

        foreach (var galaxy in config.Galaxies)
        {
            Vec3d pos = galaxy.Position.Length >= 3
                ? new Vec3d(galaxy.Position[0], galaxy.Position[1], galaxy.Position[2])
                : Vec3d.Zero;

            Vec3d vel = galaxy.Velocity.Length >= 3
                ? new Vec3d(galaxy.Velocity[0], galaxy.Velocity[1], galaxy.Velocity[2])
                : Vec3d.Zero;

            var bodies = MiyamotoNagaiDiskGenerator.Generate(
                totalCount: galaxy.TotalParticles,
                smbhMass: galaxy.SmbhMass,
                bulgeMass: galaxy.BulgeMass,
                bulgeScale: galaxy.BulgeScale,
                diskMass: galaxy.DiskMass,
                diskRadialScale: galaxy.DiskRadialScale,
                diskVerticalScale: galaxy.DiskVerticalScale,
                haloMass: galaxy.HaloMass,
                haloScale: galaxy.HaloScale,
                spiralPitchDeg: galaxy.SpiralPitchDeg,
                spiralArmCount: galaxy.SpiralArmCount,
                spiralStrength: galaxy.SpiralStrength,
                barStrength: galaxy.BarStrength,
                barLength: galaxy.BarLength,
                dustFraction: galaxy.DustFraction,
                hiiFraction: galaxy.HiiFraction,
                youngStarFraction: galaxy.YoungStarFraction,
                bulgeFraction: galaxy.BulgeFraction,
                haloFraction: galaxy.HaloFraction,
                centerPosition: pos,
                centerVelocity: vel,
                inclinationDeg: galaxy.InclinationDeg,
                positionAngleDeg: galaxy.PositionAngleDeg,
                seed: galaxy.Seed);

            // Re-index IDs with offset
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Id = idOffset + i;
            }

            allBodies.AddRange(bodies);
            idOffset += bodies.Length;
        }

        return allBodies.ToArray();
    }

    /// <summary>
    /// Apply physics overrides from a scenario config to a PhysicsConfig.
    /// Only non-null values are applied, preserving defaults for unspecified params.
    /// </summary>
    public static void ApplyPhysicsOverrides(GalaxyScenarioConfig config, PhysicsConfig physicsConfig)
    {
        var overrides = config.Physics;
        if (overrides == null) return;

        if (overrides.SofteningEpsilon.HasValue)
            physicsConfig.SofteningEpsilon = overrides.SofteningEpsilon.Value;

        if (overrides.UseBarnesHut.HasValue)
            physicsConfig.UseBarnesHut = overrides.UseBarnesHut.Value;

        if (overrides.Theta.HasValue)
            physicsConfig.Theta = overrides.Theta.Value;

        if (overrides.EnableDynamicalFriction.HasValue)
            physicsConfig.EnableDynamicalFriction = overrides.EnableDynamicalFriction.Value;

        if (overrides.DynamicalFrictionLnLambda.HasValue)
            physicsConfig.DynamicalFrictionLnLambda = overrides.DynamicalFrictionLnLambda.Value;

        if (overrides.DensityWaveStrength.HasValue)
            physicsConfig.DensityWaveStrength = overrides.DensityWaveStrength.Value;

        if (overrides.StarburstDensityThreshold.HasValue)
            physicsConfig.StarburstDensityThreshold = overrides.StarburstDensityThreshold.Value;
    }
}
