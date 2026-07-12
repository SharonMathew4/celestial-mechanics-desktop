namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a galaxy — a large-scale gravitationally bound system of
/// stars, stellar remnants, gas, dust, and dark matter.
/// </summary>
public sealed class Galaxy : CelestialBody
{
    /// <summary>
    /// Gets or sets the morphological classification (e.g. "Spiral", "Elliptical", "Irregular").
    /// </summary>
    public string GalaxyType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Hubble classification (e.g. "Sa", "SBb", "E0").
    /// </summary>
    public string HubbleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the redshift value (z).
    /// </summary>
    public double Redshift { get; set; }

    /// <summary>
    /// Gets or sets the estimated distance in megaparsecs (Mpc).
    /// </summary>
    public double DistanceMpc { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Galaxy"/> class.
    /// </summary>
    public Galaxy(string? id, string name) : base(id, name, CelestialBodyType.Galaxy) { }
}
