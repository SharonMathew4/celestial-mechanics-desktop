using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Provider managing the stellar catalog dataset.
/// </summary>
public sealed class StarProvider : ICatalogProvider
{
    private readonly ObservationCatalog _catalog;

    /// <inheritdoc />
    public string Name => "Stars";

    /// <inheritdoc />
    public bool IsLoaded => _catalog.IsLoaded;

    /// <summary>
    /// Gets the list of loaded star entries.
    /// </summary>
    public IReadOnlyList<StarEntry> Stars => _catalog.Stars;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarProvider"/> class.
    /// </summary>
    public StarProvider(ObservationCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return _catalog.LoadAsync(cancellationToken);
    }
}

/// <summary>
/// Placeholder provider for planetary datasets.
/// </summary>
public sealed class PlanetProvider : ICatalogProvider
{
    /// <inheritdoc />
    public string Name => "Planets";

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoaded = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Placeholder provider for galaxy datasets.
/// </summary>
public sealed class GalaxyProvider : ICatalogProvider
{
    /// <inheritdoc />
    public string Name => "Galaxies";

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoaded = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Placeholder provider for deep space nebulae.
/// </summary>
public sealed class NebulaProvider : ICatalogProvider
{
    /// <inheritdoc />
    public string Name => "Nebulae";

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoaded = true;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Placeholder provider for spacecraft trajectory datasets.
/// </summary>
public sealed class SpacecraftProvider : ICatalogProvider
{
    /// <inheritdoc />
    public string Name => "Spacecrafts";

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoaded = true;
        return Task.CompletedTask;
    }
}
