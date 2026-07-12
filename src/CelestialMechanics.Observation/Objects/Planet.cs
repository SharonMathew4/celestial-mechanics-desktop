namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a planetary body orbiting a star.
/// </summary>
public sealed class Planet : CelestialBody
{
    /// <summary>
    /// Gets or sets the semi-major axis of the orbit in AU.
    /// </summary>
    public double SemiMajorAxis { get; set; }

    /// <summary>
    /// Gets or sets the orbital eccentricity (0 = circular, 1 = parabolic).
    /// </summary>
    public double Eccentricity { get; set; }

    /// <summary>
    /// Gets or sets the orbital inclination in degrees.
    /// </summary>
    public double Inclination { get; set; }

    /// <summary>
    /// Gets or sets the orbital period in Earth days.
    /// </summary>
    public double OrbitalPeriod { get; set; }

    /// <summary>
    /// Gets or sets whether the planet is a gas giant.
    /// </summary>
    public bool IsGasGiant { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Planet"/> class.
    /// </summary>
    public Planet(string? id, string name) : base(id, name, CelestialBodyType.Planet) { }
}
