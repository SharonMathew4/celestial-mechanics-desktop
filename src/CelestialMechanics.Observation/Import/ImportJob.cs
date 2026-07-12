using System;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Parameters defining a specific execution of the catalog import pipeline.
/// </summary>
public sealed class ImportJob
{
    /// <summary>
    /// Gets the catalog identity (e.g. "Hipparcos").
    /// </summary>
    public string CatalogName { get; }

    /// <summary>
    /// Gets the source reader reference.
    /// </summary>
    public IDataSource DataSource { get; }

    /// <summary>
    /// Gets the active pipeline settings.
    /// </summary>
    public ImportSettings Settings { get; }

    /// <summary>
    /// Event triggered periodically during execution to update progress.
    /// </summary>
    public Action<ImportProgress>? ProgressCallback { get; set; }

    public ImportJob(string catalogName, IDataSource dataSource, ImportSettings settings, Action<ImportProgress>? progressCallback = null)
    {
        CatalogName = catalogName ?? throw new ArgumentNullException(nameof(catalogName));
        DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        ProgressCallback = progressCallback;
    }
}
