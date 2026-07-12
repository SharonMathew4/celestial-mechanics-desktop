using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.World;

/// <summary>
/// Controls high-level sector lifecycles and visibility filtering for observation objects.
/// </summary>
public sealed class WorldManager
{
    private readonly HashSet<string> _visibleObjectIds = new();

    /// <summary>
    /// Gets the currently active universe settings and coordinate frames.
    /// </summary>
    public Universe ActiveUniverse { get; } = new();

    /// <summary>
    /// Gets the set of currently visible scene object IDs.
    /// </summary>
    public IReadOnlyCollection<string> VisibleObjectIds => _visibleObjectIds;

    /// <summary>
    /// Modifies the visibility setting for an object.
    /// </summary>
    public void SetObjectVisibility(string objectId, bool visible)
    {
        if (objectId == null) throw new ArgumentNullException(nameof(objectId));
        if (visible)
        {
            _visibleObjectIds.Add(objectId);
        }
        else
        {
            _visibleObjectIds.Remove(objectId);
        }
    }

    /// <summary>
    /// Lifecycle update function for managing visible sectors and objects.
    /// </summary>
    public void UpdateSectors()
    {
        // Future extensions will compute spatial intersections between frustum and sectors
    }
}
