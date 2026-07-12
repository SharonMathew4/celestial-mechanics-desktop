using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.World;

/// <summary>
/// Root model containing all settings, reference frames, and spatial sectors of the universe.
/// </summary>
public sealed class Universe
{
    private readonly Dictionary<string, Sector> _sectors = new();

    /// <summary>
    /// Gets the settings governing this universe view.
    /// </summary>
    public UniverseSettings Settings { get; } = new();

    /// <summary>
    /// Gets the active reference frame coordinate framework.
    /// </summary>
    public CoordinateFrame CurrentFrame { get; } = new();

    /// <summary>
    /// Gets all registered sectors in the universe.
    /// </summary>
    public IReadOnlyDictionary<string, Sector> Sectors => _sectors;

    /// <summary>
    /// Registers a sector into the universe.
    /// </summary>
    public void AddSector(Sector sector)
    {
        if (sector == null) throw new ArgumentNullException(nameof(sector));
        _sectors[sector.Id] = sector;
    }

    /// <summary>
    /// Unregisters a sector by its identifier.
    /// </summary>
    public bool RemoveSector(string id)
    {
        if (id == null) return false;
        return _sectors.Remove(id);
    }
}
