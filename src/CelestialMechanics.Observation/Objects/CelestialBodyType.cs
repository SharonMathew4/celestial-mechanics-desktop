namespace CelestialMechanics.Observation.Objects;

/// <summary>
/// Categorizes the type of a celestial body within the universe model.
/// </summary>
public enum CelestialBodyType
{
    /// <summary>A stellar object producing light through nuclear fusion.</summary>
    Star,

    /// <summary>A large body orbiting a star, cleared of neighboring debris.</summary>
    Planet,

    /// <summary>A natural satellite orbiting a planet.</summary>
    Moon,

    /// <summary>A small rocky body, typically in the asteroid belt.</summary>
    Asteroid,

    /// <summary>An icy body with an eccentric orbit displaying a coma/tail near perihelion.</summary>
    Comet,

    /// <summary>A large-scale gravitationally bound system of stars and matter.</summary>
    Galaxy,

    /// <summary>An interstellar cloud of gas and dust.</summary>
    Nebula,

    /// <summary>A region of spacetime with extreme gravitational effects.</summary>
    BlackHole,

    /// <summary>An artificial object placed in orbit or interplanetary trajectory.</summary>
    Spacecraft
}
