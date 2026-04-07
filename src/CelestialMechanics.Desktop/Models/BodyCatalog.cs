using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Static catalog of all body subtypes organized by category.
/// Provides preset physical properties for quick body placement.
/// </summary>
public static class BodyCatalog
{
    /// <summary>
    /// Returns all subtypes for the given body type category.
    /// </summary>
    public static List<BodySubtype> GetSubtypes(BodyType category) => category switch
    {
        BodyType.Star => StarSubtypes,
        BodyType.Planet => PlanetSubtypes,
        BodyType.GasGiant => GasGiantSubtypes,
        BodyType.RockyPlanet => RockyBodySubtypes,
        BodyType.Moon => MoonSubtypes,
        BodyType.Asteroid => AsteroidSubtypes,
        BodyType.NeutronStar => NeutronStarSubtypes,
        BodyType.BlackHole => BlackHoleSubtypes,
        BodyType.Comet => CometSubtypes,
        BodyType.Custom => CustomSubtypes,
        _ => new List<BodySubtype>()
    };

    // ═══════════════════════════════════════════════════════════════
    //  STAR SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> StarSubtypes = new()
    {
        new("Main Sequence (G-type)", "Sun-like star, yellow-white, ~1 solar mass", 
            BodyType.Star, 1.0, 0.1, 1.0f, 0.95f, 0.4f, "★"),
        
        new("Red Giant", "Evolved star, expanded and cooled, ~2 solar masses", 
            BodyType.Star, 2.0, 0.25, 1.0f, 0.3f, 0.1f, "★"),
        
        new("Red Dwarf", "Small cool star, most common type, ~0.2 solar masses", 
            BodyType.Star, 0.2, 0.05, 0.9f, 0.2f, 0.1f, "★"),
        
        new("Blue Supergiant", "Massive hot star, 10-100 solar masses", 
            BodyType.Star, 20.0, 0.3, 0.5f, 0.7f, 1.0f, "★"),
        
        new("White Dwarf", "Stellar remnant, Earth-sized but Sun-mass", 
            BodyType.Star, 0.6, 0.02, 0.95f, 0.95f, 1.0f, "★"),
        
        new("Wolf-Rayet", "Massive star with strong stellar winds, ~15 solar masses", 
            BodyType.Star, 15.0, 0.15, 0.6f, 0.4f, 1.0f, "★"),
        
        new("T-Tauri (Protostar)", "Young star still forming, variable, ~0.5 solar masses", 
            BodyType.Star, 0.5, 0.12, 1.0f, 0.6f, 0.2f, "★"),
        
        new("Hypergiant", "Extremely luminous, 50-100+ solar masses", 
            BodyType.Star, 70.0, 0.5, 0.9f, 0.8f, 0.3f, "★"),
        
        new("Variable Star", "Star with changing brightness, ~1.5 solar masses", 
            BodyType.Star, 1.5, 0.12, 1.0f, 0.85f, 0.5f, "★"),
        
        new("Binary Star", "Two stars in mutual orbit, combined ~2 solar masses", 
            BodyType.Star, 2.0, 0.15, 1.0f, 0.9f, 0.35f, "★"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  PLANET SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> PlanetSubtypes = new()
    {
        new("Terrestrial", "Rocky planet like Earth, ~1 Earth mass", 
            BodyType.Planet, 0.001, 0.03, 0.2f, 0.4f, 0.8f, "●"),
        
        new("Super-Earth", "Large rocky planet, 2-10 Earth masses", 
            BodyType.Planet, 0.005, 0.04, 0.3f, 0.5f, 0.7f, "●"),
        
        new("Ocean World", "Planet covered in deep oceans, ~1.5 Earth masses", 
            BodyType.Planet, 0.0015, 0.032, 0.1f, 0.3f, 0.9f, "●"),
        
        new("Ice Giant", "Cold planet with water/ammonia ice, ~15 Earth masses", 
            BodyType.Planet, 0.015, 0.05, 0.4f, 0.6f, 0.9f, "●"),
        
        new("Lava Planet", "Molten surface planet close to star, ~0.8 Earth masses", 
            BodyType.Planet, 0.0008, 0.025, 0.9f, 0.2f, 0.1f, "●"),
        
        new("Desert Planet", "Arid planet like Mars, ~0.1 Earth masses", 
            BodyType.Planet, 0.0001, 0.02, 0.8f, 0.4f, 0.2f, "●"),
        
        new("Carbon Planet", "Diamond/graphite-rich world, ~2 Earth masses", 
            BodyType.Planet, 0.002, 0.035, 0.2f, 0.2f, 0.25f, "●"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  GAS GIANT SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> GasGiantSubtypes = new()
    {
        new("Hot Jupiter", "Gas giant very close to star, ~1 Jupiter mass", 
            BodyType.GasGiant, 0.001, 0.06, 0.9f, 0.5f, 0.2f, "◉"),
        
        new("Cold Gas Giant", "Jupiter-like at outer system, ~1 Jupiter mass", 
            BodyType.GasGiant, 0.001, 0.06, 0.8f, 0.7f, 0.5f, "◉"),
        
        new("Ice Giant", "Uranus/Neptune-like, ~0.05 Jupiter mass", 
            BodyType.GasGiant, 0.00005, 0.04, 0.4f, 0.7f, 0.9f, "◉"),
        
        new("Mini-Neptune", "Small gas planet, ~10 Earth masses", 
            BodyType.GasGiant, 0.00003, 0.035, 0.5f, 0.6f, 0.85f, "◉"),
        
        new("Brown Dwarf", "Failed star, ~40 Jupiter masses", 
            BodyType.GasGiant, 0.04, 0.08, 0.5f, 0.2f, 0.15f, "◉"),
        
        new("Super-Jupiter", "Very massive gas giant, ~5 Jupiter masses", 
            BodyType.GasGiant, 0.005, 0.08, 0.9f, 0.8f, 0.6f, "◉"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  ROCKY BODY SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> RockyBodySubtypes = new()
    {
        new("Mars-like", "Cold desert world with thin atmosphere, ~0.1 Earth mass", 
            BodyType.RockyPlanet, 0.0001, 0.018, 0.8f, 0.3f, 0.15f, "▪"),
        
        new("Mercury-like", "Small dense world close to star, ~0.05 Earth mass", 
            BodyType.RockyPlanet, 0.00005, 0.015, 0.5f, 0.45f, 0.4f, "▪"),
        
        new("Io-like (Volcanic)", "Tidally heated volcanic world, ~0.01 Earth mass", 
            BodyType.RockyPlanet, 0.00001, 0.012, 1.0f, 0.6f, 0.1f, "▪"),
        
        new("Ceres-like (Dwarf Planet)", "Large asteroid/dwarf planet, ~0.0002 Earth mass", 
            BodyType.RockyPlanet, 0.0000002, 0.006, 0.55f, 0.5f, 0.45f, "▪"),
        
        new("Pluto-like", "Trans-Neptunian dwarf planet, ~0.002 Earth mass", 
            BodyType.RockyPlanet, 0.000002, 0.008, 0.7f, 0.65f, 0.6f, "▪"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  MOON SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> MoonSubtypes = new()
    {
        new("Rocky Moon", "Cratered rocky moon like Luna, ~0.01 Earth mass", 
            BodyType.Moon, 0.00001, 0.01, 0.7f, 0.7f, 0.7f, "◦"),
        
        new("Icy Moon (Europa-type)", "Ice-covered ocean moon, ~0.008 Earth mass", 
            BodyType.Moon, 0.000008, 0.009, 0.85f, 0.9f, 0.95f, "◦"),
        
        new("Titan-like (Atmospheric)", "Large moon with thick atmosphere, ~0.02 Earth mass", 
            BodyType.Moon, 0.00002, 0.012, 0.8f, 0.6f, 0.3f, "◦"),
        
        new("Volcanic Moon (Io)", "Volcanically active from tidal heating, ~0.01 Earth mass", 
            BodyType.Moon, 0.00001, 0.01, 1.0f, 0.7f, 0.2f, "◦"),
        
        new("Captured Asteroid", "Small irregular moon, ~0.0001 Earth mass", 
            BodyType.Moon, 0.0000001, 0.004, 0.5f, 0.45f, 0.4f, "◦"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  ASTEROID SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> AsteroidSubtypes = new()
    {
        new("C-type (Carbonaceous)", "Dark carbon-rich asteroid, most common", 
            BodyType.Asteroid, 1e-12, 0.003, 0.2f, 0.2f, 0.2f, "◇"),
        
        new("S-type (Siliceous)", "Rocky silicate asteroid, moderately bright", 
            BodyType.Asteroid, 1e-12, 0.003, 0.6f, 0.5f, 0.4f, "◇"),
        
        new("M-type (Metallic)", "Metal-rich asteroid, high density", 
            BodyType.Asteroid, 2e-12, 0.003, 0.75f, 0.7f, 0.65f, "◇"),
        
        new("Rubble Pile", "Loose aggregate held by gravity", 
            BodyType.Asteroid, 0.5e-12, 0.004, 0.45f, 0.4f, 0.35f, "◇"),
        
        new("Trojan", "Asteroid at L4/L5 Lagrange point", 
            BodyType.Asteroid, 1e-12, 0.003, 0.35f, 0.3f, 0.25f, "◇"),
        
        new("Binary Asteroid", "Two asteroids orbiting each other", 
            BodyType.Asteroid, 1.5e-12, 0.0035, 0.5f, 0.45f, 0.4f, "◇"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  NEUTRON STAR SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> NeutronStarSubtypes = new()
    {
        new("Radio Pulsar", "Rotating neutron star emitting radio waves, ~1.4 solar masses", 
            BodyType.NeutronStar, 1.4, 0.02, 0.5f, 0.8f, 1.0f, "✦"),
        
        new("Millisecond Pulsar", "Extremely fast spinning pulsar, ~1.5 solar masses", 
            BodyType.NeutronStar, 1.5, 0.02, 0.6f, 0.9f, 1.0f, "✦"),
        
        new("Magnetar", "Neutron star with extreme magnetic field, ~2 solar masses", 
            BodyType.NeutronStar, 2.0, 0.022, 0.8f, 0.3f, 1.0f, "✦"),
        
        new("X-ray Pulsar", "Neutron star in binary emitting X-rays, ~1.4 solar masses", 
            BodyType.NeutronStar, 1.4, 0.02, 0.3f, 0.6f, 1.0f, "✦"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  BLACK HOLE SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> BlackHoleSubtypes = new()
    {
        new("Stellar (3-20 M☉)", "Black hole from stellar collapse, ~10 solar masses", 
            BodyType.BlackHole, 10.0, 0.05, 0.1f, 0.0f, 0.1f, "◯"),
        
        new("Intermediate (100-100k M☉)", "Medium black hole, ~1000 solar masses", 
            BodyType.BlackHole, 1000.0, 0.15, 0.15f, 0.0f, 0.15f, "◯"),
        
        new("Supermassive (1M+ M☉)", "Galactic center black hole, ~4 million solar masses", 
            BodyType.BlackHole, 4e6, 0.5, 0.2f, 0.0f, 0.2f, "◯"),
        
        new("Primordial (Micro)", "Hypothetical small black hole, ~0.1 solar masses", 
            BodyType.BlackHole, 0.1, 0.01, 0.05f, 0.0f, 0.05f, "◯"),
        
        new("Kerr (Rotating)", "Spinning black hole with ergosphere, ~15 solar masses", 
            BodyType.BlackHole, 15.0, 0.06, 0.12f, 0.0f, 0.12f, "◯"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  COMET SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> CometSubtypes = new()
    {
        new("Short-Period", "Comet with orbit <200 years, Jupiter family", 
            BodyType.Comet, 1e-14, 0.003, 0.6f, 0.6f, 0.5f, "☄"),
        
        new("Long-Period", "Comet from Oort Cloud, orbit >200 years", 
            BodyType.Comet, 1e-14, 0.003, 0.7f, 0.7f, 0.6f, "☄"),
        
        new("Halley-type", "Intermediate period comet, 20-200 years", 
            BodyType.Comet, 1e-14, 0.004, 0.55f, 0.55f, 0.45f, "☄"),
        
        new("Sun-Grazer", "Comet that passes extremely close to star", 
            BodyType.Comet, 0.5e-14, 0.0025, 0.8f, 0.7f, 0.5f, "☄"),
        
        new("Interstellar Visitor", "Comet from another star system", 
            BodyType.Comet, 1e-14, 0.003, 0.4f, 0.35f, 0.3f, "☄"),
    };

    // ═══════════════════════════════════════════════════════════════
    //  CUSTOM SUBTYPES
    // ═══════════════════════════════════════════════════════════════

    private static readonly List<BodySubtype> CustomSubtypes = new()
    {
        new("User-defined", "Custom body with user-specified properties", 
            BodyType.Custom, 1.0, 0.04, 0.6f, 0.6f, 0.6f, "◈"),
    };

    /// <summary>
    /// Gets all body categories.
    /// </summary>
    public static IReadOnlyList<BodyType> AllCategories { get; } = new[]
    {
        BodyType.Star,
        BodyType.Planet,
        BodyType.GasGiant,
        BodyType.RockyPlanet,
        BodyType.Moon,
        BodyType.Asteroid,
        BodyType.NeutronStar,
        BodyType.BlackHole,
        BodyType.Comet,
        BodyType.Custom
    };

    /// <summary>
    /// Gets the display name for a body type category.
    /// </summary>
    public static string GetCategoryDisplayName(BodyType type) => type switch
    {
        BodyType.Star => "★ Stars",
        BodyType.Planet => "● Planets",
        BodyType.GasGiant => "◉ Gas Giants",
        BodyType.RockyPlanet => "▪ Rocky Bodies",
        BodyType.Moon => "◦ Moons",
        BodyType.Asteroid => "◇ Asteroids",
        BodyType.NeutronStar => "✦ Neutron Stars",
        BodyType.BlackHole => "◯ Black Holes",
        BodyType.Comet => "☄ Comets",
        BodyType.Custom => "◈ Custom",
        _ => type.ToString()
    };
}
