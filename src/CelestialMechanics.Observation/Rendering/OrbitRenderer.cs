using System;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer targeting Orbit nodes.
/// </summary>
public sealed class OrbitRenderer : IRenderer
{
    public string SupportedNodeType => "Orbit";

    public void Render(SceneNode node, RenderContext context, RenderSettings settings)
    {
        // Renders orbital lines
    }
}
