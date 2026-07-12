using System;
using System.Collections.Generic;
using CelestialMechanics.Math;

namespace CelestialMechanics.Observation.World;

/// <summary>
/// Partitions the universe space into localized sectors to support
/// visibility querying and level-of-detail management.
/// </summary>
public sealed class Sector
{
    private readonly List<string> _loadedNodeIds = new();

    /// <summary>
    /// Gets the unique identifier of the sector.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the center point of the sector in parsecs.
    /// </summary>
    public Vec3d Centroid { get; }

    /// <summary>
    /// Gets the radius bounding the sector in parsecs.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// Gets the collection of scene node identifiers loaded within this sector.
    /// </summary>
    public IReadOnlyList<string> LoadedNodeIds => _loadedNodeIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sector"/> class.
    /// </summary>
    public Sector(string id, Vec3d centroid, double radius)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Centroid = centroid;
        Radius = radius;
    }

    /// <summary>
    /// Adds a node ID to the sector.
    /// </summary>
    public void AddNode(string nodeId)
    {
        if (nodeId == null) throw new ArgumentNullException(nameof(nodeId));
        if (!_loadedNodeIds.Contains(nodeId))
        {
            _loadedNodeIds.Add(nodeId);
        }
    }

    /// <summary>
    /// Removes a node ID from the sector.
    /// </summary>
    public bool RemoveNode(string nodeId)
    {
        if (nodeId == null) return false;
        return _loadedNodeIds.Remove(nodeId);
    }
}
