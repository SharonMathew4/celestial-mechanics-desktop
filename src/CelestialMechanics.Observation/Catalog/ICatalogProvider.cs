using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Defines the contract for various catalog content providers (e.g. Stars, Planets, Galaxies).
/// </summary>
public interface ICatalogProvider
{
    /// <summary>
    /// Gets the unique identifier/name of the provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets whether the provider has loaded its dataset successfully.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Asynchronously loads dataset records from source files/caches.
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken = default);
}
