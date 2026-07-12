using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Services;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Service implementing <see cref="ICatalogService"/> that orchestrates loading and lookup
/// across registered <see cref="ICatalogProvider"/> instances.
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private readonly List<ICatalogProvider> _providers = new();

    /// <inheritdoc />
    public IReadOnlyList<ICatalogProvider> Providers => _providers;

    /// <inheritdoc />
    public void RegisterProvider(ICatalogProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (!_providers.Contains(provider))
        {
            _providers.Add(provider);
        }
    }

    /// <inheritdoc />
    public async Task LoadAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>();
        foreach (var provider in _providers)
        {
            tasks.Add(provider.LoadAsync(cancellationToken));
        }
        await Task.WhenAll(tasks);
    }
}
