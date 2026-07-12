using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Orchestrates the registration and execution of catalog importers.
/// </summary>
public sealed class ImportManager
{
    private readonly Dictionary<string, IImporter> _importers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the list of registered importers.
    /// </summary>
    public IEnumerable<IImporter> Importers => _importers.Values;

    /// <summary>
    /// Registers a new catalog importer.
    /// </summary>
    public void RegisterImporter(IImporter importer)
    {
        if (importer == null) throw new ArgumentNullException(nameof(importer));
        _importers[importer.CatalogName] = importer;
    }

    /// <summary>
    /// Runs the import pipeline for a given job.
    /// </summary>
    public async Task<ImportResult> RunImportAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));

        if (!_importers.TryGetValue(job.CatalogName, out var importer))
        {
            throw new ImportException($"No catalog importer registered for: '{job.CatalogName}'");
        }

        // Run validation passes and execute
        return await importer.ImportAsync(job, cancellationToken);
    }
}
