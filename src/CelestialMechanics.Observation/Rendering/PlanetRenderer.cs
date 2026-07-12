using System;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer targeting Planet nodes.
/// </summary>
public sealed class PlanetRenderer : IRenderer
{
    public string SupportedNodeType => "Planet";

    public void Render(SceneNode node, RenderContext context, RenderSettings settings)
    {
        // Renders planet sphere mesh
    }
}
