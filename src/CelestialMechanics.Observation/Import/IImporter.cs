using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Defines the contract for an astronomical dataset importer.
/// </summary>
public interface IImporter
{
    /// <summary>
    /// Gets the unique identifier for the catalog target.
    /// </summary>
    string CatalogName { get; }

    /// <summary>
    /// Executes the import job pipeline (Read -> Validate -> Convert -> Store -> Report Progress).
    /// </summary>
    Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default);
}
