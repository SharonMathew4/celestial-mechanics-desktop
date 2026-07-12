using System;
using System.Collections.Generic;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Summary report issued when an import job completes execution.
/// </summary>
public sealed class ImportResult
{
    /// <summary>
    /// Gets whether the import job succeeded overall.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets the catalog name.
    /// </summary>
    public string CatalogName { get; set; } = string.Empty;

    /// <summary>
    /// Gets total successfully imported records.
    /// </summary>
    public long ImportedCount { get; set; }

    /// <summary>
    /// Gets total skipped records.
    /// </summary>
    public long SkippedCount { get; set; }

    /// <summary>
    /// Validation errors encountered during the import run.
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// Validation warnings or skip explanations.
    /// </summary>
    public List<string> Warnings { get; } = new();

    /// <summary>
    /// Total execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
