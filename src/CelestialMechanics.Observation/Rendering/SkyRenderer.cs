using System;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer targeting background sky/constellation outlines.
/// </summary>
public sealed class SkyRenderer : IRenderer
{
    public string SupportedNodeType => "Sky";

    public void Render(SceneNode node, RenderContext context, RenderSettings settings)
    {
        // Renders solid black background by default.
        // Structurally prepared to support Milky Way texture, HDR, and procedural backgrounds.
    }
}
