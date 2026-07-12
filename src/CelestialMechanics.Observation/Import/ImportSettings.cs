namespace CelestialMechanics.Observation.Import;

/// <summary>
/// General configuration preferences governing an active import pipeline.
/// </summary>
public sealed class ImportSettings
{
    /// <summary>
    /// Gets or sets whether to overwrite existing matching entries.
    /// </summary>
    public bool OverwriteExisting { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to run validation passes on records prior to insertion.
    /// </summary>
    public bool ValidateRecords { get; set; } = true;

    /// <summary>
    /// Size of batch operations for transactions and progress updates.
    /// </summary>
    public int BatchSize { get; set; } = 1000;
}
