using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Catalog;

namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for managing multiple dataset providers in Observation Mode.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Gets the list of registered catalog providers.
    /// </summary>
    IReadOnlyList<ICatalogProvider> Providers { get; }

    /// <summary>
    /// Registers a new catalog provider.
    /// </summary>
    void RegisterProvider(ICatalogProvider provider);

    /// <summary>
    /// Asynchronously triggers loading on all registered providers.
    /// </summary>
    Task LoadAllAsync(CancellationToken cancellationToken = default);
}
