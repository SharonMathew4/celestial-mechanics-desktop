using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CelestialMechanics.Observation.Database;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Provider managing the stellar catalog dataset, retrieved from SQLite.
/// </summary>
public sealed class StarProvider : ICatalogProvider
{
    private readonly AstronomicalObjectRepository _repository;
    private List<StarEntry> _stars = new();

    /// <inheritdoc />
    public string Name => "Stars";

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Gets the list of loaded star entries.
    /// </summary>
    public IReadOnlyList<StarEntry> Stars => _stars;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarProvider"/> class.
    /// </summary>
    public StarProvider(AstronomicalObjectRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _stars = await _repository.GetStarsAsync("Hipparcos");
        IsLoaded = true;
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
