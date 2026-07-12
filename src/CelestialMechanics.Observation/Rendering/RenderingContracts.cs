using System;
using System.Collections.Generic;
using System.Numerics;
using CelestialMechanics.Observation.Scene;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Configures rendering settings for the viewport.
/// </summary>
public sealed class RenderSettings
{
    public bool ShowConstellations { get; set; } = true;
    public bool ShowGrid { get; set; } = true;
    public bool ShowLabels { get; set; } = true;
    public string ActiveGridType { get; set; } = "Equatorial"; // Equatorial, Galactic, Heliocentric, Ecliptic
}

/// <summary>
/// Execution context for render passes, containing transformation matrices.
/// </summary>
public sealed class RenderContext
{
    public Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
    public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
    public float AspectRatio { get; set; } = 1.0f;
    public float DeltaTime { get; set; }
}

/// <summary>
/// Modular renderer contract for type-specific node drawing.
/// </summary>
public interface IRenderer
{
    string SupportedNodeType { get; }
    void Render(SceneNode node, RenderContext context, RenderSettings settings);
}

/// <summary>
/// Single rendering pass representation.
/// </summary>
public interface IRenderPass
{
    string PassName { get; }
    void Execute(RenderContext context, RenderSettings settings, RenderQueue queue);
}

/// <summary>
/// Categorized Render Queue. Prepares and separates nodes for drawings.
/// </summary>
public sealed class RenderQueue
{
    private readonly Dictionary<string, List<SceneNode>> _categorizedNodes = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, List<SceneNode>> CategorizedNodes => _categorizedNodes;

    public void Enqueue(SceneNode node)
    {
        var category = node.NodeType;
        if (!_categorizedNodes.TryGetValue(category, out var list))
        {
            list = new List<SceneNode>();
            _categorizedNodes[category] = list;
        }
        list.Add(node);
    }

    public void Clear()
    {
        _categorizedNodes.Clear();
    }
}
