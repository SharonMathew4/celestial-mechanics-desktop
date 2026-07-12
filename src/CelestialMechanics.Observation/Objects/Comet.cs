namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a comet — an icy body with an eccentric orbit that develops
/// a coma and tail when approaching perihelion.
/// </summary>
public sealed class Comet : CelestialBody
{
    /// <summary>
    /// Gets or sets the perihelion distance in AU.
    /// </summary>
    public double PerihelionDistance { get; set; }

    /// <summary>
    /// Gets or sets the aphelion distance in AU.
    /// </summary>
    public double AphelionDistance { get; set; }

    /// <summary>
    /// Gets or sets the orbital eccentricity.
    /// </summary>
    public double Eccentricity { get; set; }

    /// <summary>
    /// Gets or sets the orbital period in years. Zero or negative for non-periodic comets.
    /// </summary>
    public double OrbitalPeriodYears { get; set; }

    /// <summary>
    /// Gets or sets whether this is a periodic comet.
    /// </summary>
    public bool IsPeriodic { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Comet"/> class.
    /// </summary>
    public Comet(string? id, string name) : base(id, name, CelestialBodyType.Comet) { }
}
