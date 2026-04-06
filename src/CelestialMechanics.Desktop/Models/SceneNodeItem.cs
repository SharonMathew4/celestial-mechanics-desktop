using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Display model for a single item in the Scene Outliner list.
/// </summary>
public sealed class SceneNodeItem
{
    public Guid NodeId { get; init; }
    public string Name { get; set; } = "";
    public string TypeLabel { get; set; } = "";
    public string IconGlyph { get; set; } = "";

    public static string GetIconForBodyType(BodyType type) => type switch
    {
        BodyType.Star => "\u2605",        // ★
        BodyType.Planet => "\u25CF",      // ●
        BodyType.GasGiant => "\u25C9",    // ◉
        BodyType.RockyPlanet => "\u25AA", // ▪
        BodyType.Moon => "\u25E6",        // ◦
        BodyType.Asteroid => "\u25C7",    // ◇
        BodyType.NeutronStar => "\u2726", // ✦
        BodyType.BlackHole => "\u25EF",   // ◯
        BodyType.Comet => "\u2604",       // ☄
        BodyType.Custom => "\u25C8",      // ◈
        _ => "\u25CF"
    };
}
