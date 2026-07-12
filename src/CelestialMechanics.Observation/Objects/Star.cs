namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a stellar object producing light through nuclear fusion.
/// </summary>
public sealed class Star : CelestialBody
{
    /// <summary>
    /// Gets or sets the spectral classification (e.g. "G2V", "M0III").
    /// </summary>
    public string SpectralType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute magnitude.
    /// </summary>
    public double AbsoluteMagnitude { get; set; }

    /// <summary>
    /// Gets or sets the B-V color index.
    /// </summary>
    public double ColorIndex { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Star"/> class.
    /// </summary>
    public Star(string? id, string name) : base(id, name, CelestialBodyType.Star) { }
}
