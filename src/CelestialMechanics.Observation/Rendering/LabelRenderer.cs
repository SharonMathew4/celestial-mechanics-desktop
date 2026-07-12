using System;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Renderer targeting annotation and identity text labels.
/// </summary>
public sealed class LabelRenderer : IRenderer
{
    public string SupportedNodeType => "Label";

    public void Render(SceneNode node, RenderContext context, RenderSettings settings)
    {
        // Renders simple text annotation label for node.Name
    }
}
