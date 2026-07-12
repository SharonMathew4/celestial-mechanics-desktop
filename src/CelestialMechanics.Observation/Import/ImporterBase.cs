using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Abstract base class for importers.
/// Provides helper methods for computing elapsed time, logging, and reporting.
/// </summary>
public abstract class ImporterBase : IImporter
{
    /// <inheritdoc />
    public abstract string CatalogName { get; }

    /// <inheritdoc />
    public abstract Task<ImportResult> ImportAsync(ImportJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper to compute estimated remaining time based on current progress.
    /// </summary>
    protected static TimeSpan? EstimateRemaining(TimeSpan elapsed, long processed, long total)
    {
        if (processed <= 0 || total <= 0 || processed >= total)
            return null;

        double ratio = (double)processed / total;
        double remainingMs = elapsed.TotalMilliseconds * (1.0 - ratio) / ratio;
        return TimeSpan.FromMilliseconds(remainingMs);
    }
}
