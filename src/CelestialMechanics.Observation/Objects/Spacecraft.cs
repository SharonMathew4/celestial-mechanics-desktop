namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Represents a spacecraft — an artificial object placed in orbit
/// or on an interplanetary trajectory.
/// </summary>
public sealed class Spacecraft : CelestialBody
{
    /// <summary>
    /// Gets or sets the mission name (e.g. "Voyager 1", "James Webb Space Telescope").
    /// </summary>
    public string MissionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the launch date as a Julian Date.
    /// </summary>
    public double LaunchDateJD { get; set; }

    /// <summary>
    /// Gets or sets whether the spacecraft is currently operational.
    /// </summary>
    public bool IsOperational { get; set; }

    /// <summary>
    /// Gets or sets the NORAD catalog number, if applicable.
    /// </summary>
    public string NoradId { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Spacecraft"/> class.
    /// </summary>
    public Spacecraft(string? id, string name) : base(id, name, CelestialBodyType.Spacecraft) { }
}
