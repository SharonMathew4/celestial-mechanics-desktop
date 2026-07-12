using CelestialMechanics.Observation.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Implementation of <see cref="IObservationCatalog"/> that queries
/// astronomical data from the standardized binary Hipparcos database.
/// </summary>
public sealed class ObservationCatalog : IObservationCatalog
{
    private readonly string _catalogPath;
    private List<StarEntry> _stars = new();

    /// <inheritdoc />
    public bool IsLoaded { get; private set; }

    /// <inheritdoc />
    public long ObjectCount => _stars.Count;

    /// <summary>
    /// Gets the list of loaded star entries.
    /// </summary>
    public IReadOnlyList<StarEntry> Stars => _stars;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservationCatalog"/> class.
    /// </summary>
    /// <param name="catalogPath">The absolute path to the binary catalog file.</param>
    public ObservationCatalog(string catalogPath)
    {
        _catalogPath = catalogPath ?? throw new ArgumentNullException(nameof(catalogPath));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservationCatalog"/> class using a default path.
    /// </summary>
    public ObservationCatalog() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "hipparcos.bin"))
    {
    }

    /// <inheritdoc />
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoaded)
            return;

        if (!File.Exists(_catalogPath))
        {
            throw new FileNotFoundException($"Hipparcos binary catalog file not found at: {_catalogPath}");
        }

        // Perform disk reading and parsing on a background thread as per threading guidelines
        _stars = await Task.Run(() => HipparcosBinaryReader.ReadCatalog(_catalogPath), cancellationToken);
        IsLoaded = true;
    }
}
