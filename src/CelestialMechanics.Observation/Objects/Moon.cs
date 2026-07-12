namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a natural satellite orbiting a planet.
/// </summary>
public sealed class Moon : CelestialBody
{
    /// <summary>
    /// Gets or sets the semi-major axis of the orbit around its parent planet in km.
    /// </summary>
    public double OrbitalRadius { get; set; }

    /// <summary>
    /// Gets or sets the orbital period around the parent planet in Earth days.
    /// </summary>
    public double OrbitalPeriod { get; set; }

    /// <summary>
    /// Gets or sets whether the moon is tidally locked to its parent.
    /// </summary>
    public bool IsTidallyLocked { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Moon"/> class.
    /// </summary>
    public Moon(string? id, string name) : base(id, name, CelestialBodyType.Moon) { }
}
