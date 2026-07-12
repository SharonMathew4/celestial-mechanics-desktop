namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for querying astronomical catalog data.
/// Future implementations will provide access to star catalogs
/// (Hipparcos, Tycho-2, Gaia), planetary ephemerides, and deep sky objects.
/// </summary>
public interface IObservationCatalog
{
    /// <summary>
    /// Whether the catalog has been loaded and is ready for queries.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Total number of objects available in the catalog.
    /// </summary>
    long ObjectCount { get; }

    /// <summary>
    /// Initializes the catalog data source.
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken = default);
}
