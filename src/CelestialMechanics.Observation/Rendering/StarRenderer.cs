using System;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer targeting Star nodes.
/// </summary>
public sealed class StarRenderer : IRenderer
{
    public string SupportedNodeType => "Star";

    public void Render(SceneNode node, RenderContext context, RenderSettings settings)
    {
        // Renders star particle or point sprite based on magnitude/spectral type
    }
}
