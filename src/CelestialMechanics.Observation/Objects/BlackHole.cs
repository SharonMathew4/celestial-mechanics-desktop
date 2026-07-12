namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a black hole — a region of spacetime where gravity is so
/// strong that nothing, including light, can escape.
/// </summary>
public sealed class BlackHole : CelestialBody
{
    /// <summary>
    /// Gets or sets the Schwarzschild radius (event horizon radius) in meters.
    /// Rs = 2GM/c²
    /// </summary>
    public double SchwarzschildRadius { get; set; }

    /// <summary>
    /// Gets or sets the dimensionless spin parameter (0 = non-rotating, 1 = maximum).
    /// </summary>
    public double SpinParameter { get; set; }

    /// <summary>
    /// Gets or sets the classification (e.g. "Stellar", "Intermediate", "Supermassive").
    /// </summary>
    public string BlackHoleClass { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlackHole"/> class.
    /// </summary>
    public BlackHole(string? id, string name) : base(id, name, CelestialBodyType.BlackHole) { }
}
