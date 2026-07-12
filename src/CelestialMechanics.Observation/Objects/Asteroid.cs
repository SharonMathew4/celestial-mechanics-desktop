namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a small rocky body, typically found in the asteroid belt or as a near-Earth object.
/// </summary>
public sealed class Asteroid : CelestialBody
{
    /// <summary>
    /// Gets or sets the semi-major axis of the orbit in AU.
    /// </summary>
    public double SemiMajorAxis { get; set; }

    /// <summary>
    /// Gets or sets the orbital eccentricity.
    /// </summary>
    public double Eccentricity { get; set; }

    /// <summary>
    /// Gets or sets the taxonomic spectral classification (e.g. "C", "S", "M").
    /// </summary>
    public string SpectralClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this asteroid is classified as potentially hazardous.
    /// </summary>
    public bool IsPotentiallyHazardous { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Asteroid"/> class.
    /// </summary>
    public Asteroid(string? id, string name) : base(id, name, CelestialBodyType.Asteroid) { }
}
