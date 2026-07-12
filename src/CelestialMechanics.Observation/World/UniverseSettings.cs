namespace CelestialMechanics.Observation.World;

/// <summary>
/// Configuration settings for the universe simulation and visualization scales.
/// </summary>
public sealed class UniverseSettings
{
    /// <summary>
    /// Gets or sets the base distance scaling factor.
    /// </summary>
    public double BaseScale { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets whether to enable Level of Detail (LOD) calculations.
    /// </summary>
    public bool EnableLOD { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum render distance in parsecs.
    /// </summary>
    public double RenderDistance { get; set; } = 100000.0;
}
