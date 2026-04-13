using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Lightweight scene node model used by UI converters and tree bindings.
/// </summary>
public sealed class SceneNodeItem
{
    public Guid NodeId { get; init; }
    public int BodyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public BodyType BodyType { get; init; }

    public static string GetIconForBodyType(BodyType bodyType)
    {
        return bodyType switch
        {
            BodyType.Star => "*",
            BodyType.Planet => "O",
            BodyType.Moon => "o",
            BodyType.Asteroid => ".",
            BodyType.Comet => ",",
            BodyType.BlackHole => "@",
            BodyType.NeutronStar => "#",
            _ => "o",
        };
    }
}
