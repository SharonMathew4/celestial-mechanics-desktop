namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a nebula — an interstellar cloud of gas, dust,
/// hydrogen, helium, and other ionized gases.
/// </summary>
public sealed class Nebula : CelestialBody
{
    /// <summary>
    /// Gets or sets the nebula classification (e.g. "Emission", "Reflection", "Planetary", "Dark").
    /// </summary>
    public string NebulaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the angular diameter in arcminutes.
    /// </summary>
    public double AngularDiameter { get; set; }

    /// <summary>
    /// Gets or sets the estimated distance in parsecs.
    /// </summary>
    public double DistanceParsecs { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Nebula"/> class.
    /// </summary>
    public Nebula(string? id, string name) : base(id, name, CelestialBodyType.Nebula) { }
}
