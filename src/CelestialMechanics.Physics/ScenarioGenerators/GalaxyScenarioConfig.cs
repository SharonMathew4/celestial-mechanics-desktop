using System.Text.Json.Serialization;

namespace CelestialMechanics.Physics.ScenarioGenerators;

/// <summary>
/// JSON-serializable configuration for JWST-inspired galaxy scenarios.
/// Defines one or two galaxies with full parameter sets for the
/// Miyamoto-Nagai disk generator, plus physics and rendering overrides.
/// </summary>
public class GalaxyScenarioConfig
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Custom Galaxy Scenario";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("galaxies")]
    public GalaxyConfig[] Galaxies { get; set; } = Array.Empty<GalaxyConfig>();

    [JsonPropertyName("physics")]
    public GalaxyPhysicsOverrides Physics { get; set; } = new();

    [JsonPropertyName("rendering")]
    public GalaxyRenderingOverrides Rendering { get; set; } = new();
}

public class GalaxyConfig
{
    [JsonPropertyName("totalParticles")]
    public int TotalParticles { get; set; } = 25000;

    [JsonPropertyName("smbhMass")]
    public double SmbhMass { get; set; } = 400.0;

    [JsonPropertyName("bulgeMass")]
    public double BulgeMass { get; set; } = 200.0;

    [JsonPropertyName("bulgeScale")]
    public double BulgeScale { get; set; } = 0.5;

    [JsonPropertyName("diskMass")]
    public double DiskMass { get; set; } = 300.0;

    [JsonPropertyName("diskRadialScale")]
    public double DiskRadialScale { get; set; } = 3.0;

    [JsonPropertyName("diskVerticalScale")]
    public double DiskVerticalScale { get; set; } = 0.15;

    [JsonPropertyName("haloMass")]
    public double HaloMass { get; set; } = 1000.0;

    [JsonPropertyName("haloScale")]
    public double HaloScale { get; set; } = 10.0;

    [JsonPropertyName("spiralPitchDeg")]
    public double SpiralPitchDeg { get; set; } = 14.0;

    [JsonPropertyName("spiralArmCount")]
    public int SpiralArmCount { get; set; } = 2;

    [JsonPropertyName("spiralStrength")]
    public double SpiralStrength { get; set; } = 0.6;

    [JsonPropertyName("barStrength")]
    public double BarStrength { get; set; } = 0.0;

    [JsonPropertyName("barLength")]
    public double BarLength { get; set; } = 1.5;

    [JsonPropertyName("dustFraction")]
    public double DustFraction { get; set; } = 0.12;

    [JsonPropertyName("hiiFraction")]
    public double HiiFraction { get; set; } = 0.05;

    [JsonPropertyName("youngStarFraction")]
    public double YoungStarFraction { get; set; } = 0.08;

    [JsonPropertyName("bulgeFraction")]
    public double BulgeFraction { get; set; } = 0.15;

    [JsonPropertyName("haloFraction")]
    public double HaloFraction { get; set; } = 0.10;

    [JsonPropertyName("position")]
    public double[] Position { get; set; } = new double[] { 0, 0, 0 };

    [JsonPropertyName("velocity")]
    public double[] Velocity { get; set; } = new double[] { 0, 0, 0 };

    [JsonPropertyName("inclinationDeg")]
    public double InclinationDeg { get; set; } = 0.0;

    [JsonPropertyName("positionAngleDeg")]
    public double PositionAngleDeg { get; set; } = 0.0;

    [JsonPropertyName("seed")]
    public int Seed { get; set; } = 42;
}

public class GalaxyPhysicsOverrides
{
    [JsonPropertyName("softeningEpsilon")]
    public double? SofteningEpsilon { get; set; }

    [JsonPropertyName("useBarnesHut")]
    public bool? UseBarnesHut { get; set; }

    [JsonPropertyName("theta")]
    public double? Theta { get; set; }

    [JsonPropertyName("enableDynamicalFriction")]
    public bool? EnableDynamicalFriction { get; set; }

    [JsonPropertyName("dynamicalFrictionLnLambda")]
    public double? DynamicalFrictionLnLambda { get; set; }

    [JsonPropertyName("densityWaveStrength")]
    public double? DensityWaveStrength { get; set; }

    [JsonPropertyName("starburstDensityThreshold")]
    public double? StarburstDensityThreshold { get; set; }
}

public class GalaxyRenderingOverrides
{
    [JsonPropertyName("enableDiffractionSpikes")]
    public bool? EnableDiffractionSpikes { get; set; }

    [JsonPropertyName("enableVolumetricDust")]
    public bool? EnableVolumetricDust { get; set; }

    [JsonPropertyName("jwstColorIntensity")]
    public float? JwstColorIntensity { get; set; }
}
