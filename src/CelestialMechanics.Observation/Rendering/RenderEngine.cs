using System;
using System.Collections.Generic;
using CelestialMechanics.Observation.Services;

namespace CelestialMechanics.Observation.Rendering;

/// <summary>
/// Core rendering manager, executing rendering passes sequentially using the RenderQueue.
/// </summary>
public sealed class RenderEngine
{
    private readonly List<IRenderPass> _renderPasses = new();

    public IReadOnlyList<IRenderPass> RenderPasses => _renderPasses;

    public void AddPass(IRenderPass pass)
    {
        if (pass == null) throw new ArgumentNullException(nameof(pass));
        _renderPasses.Add(pass);
    }

    public void Render(RenderContext context, RenderSettings settings, RenderQueue queue)
    {
        foreach (var pass in _renderPasses)
        {
            pass.Execute(context, settings, queue);
        }
    }
}
