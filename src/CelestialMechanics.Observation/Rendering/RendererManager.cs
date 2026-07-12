using System;
using System.Collections.Generic;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Orchestrator mapping scene node types to specific renderer instances.
/// </summary>
public sealed class RendererManager
{
    private readonly Dictionary<string, IRenderer> _renderers = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterRenderer(IRenderer renderer)
    {
        if (renderer == null) throw new ArgumentNullException(nameof(renderer));
        _renderers[renderer.SupportedNodeType] = renderer;
    }

    public bool TryGetRenderer(string nodeType, out IRenderer? renderer)
    {
        if (_renderers.TryGetValue(nodeType, out var r))
        {
            renderer = r;
            return true;
        }
        renderer = null;
        return false;
    }

    public void RenderNode(SceneNode node, RenderContext context, RenderSettings settings)
    {
        if (TryGetRenderer(node.NodeType, out var renderer))
        {
            renderer!.Render(node, context, settings);
        }
    }
}
