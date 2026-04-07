using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Models;

/// <summary>
/// Represents a specific subtype within a body category,
/// with preset physical properties for quick placement.
/// </summary>
public record BodySubtype(
    string Name,
    string Description,
    BodyType BaseType,
    double Mass,      // Solar masses for stars/BH, Earth masses for planets/moons
    double Radius,    // AU for rendering scale
    float ColorR,
    float ColorG,
    float ColorB,
    string IconGlyph);
