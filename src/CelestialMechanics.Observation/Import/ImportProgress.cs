using System;

namespace CelestialMechanics.Observation.Import;

/// <summary>
/// Status report emitted periodically during a run.
/// </summary>
public sealed class ImportProgress
{
    /// <summary>
    /// Filename currently being processed.
    /// </summary>
    public string CurrentFile { get; }

    /// <summary>
    /// Total items successfully committed.
    /// </summary>
    public long ObjectsImported { get; }

    /// <summary>
    /// Total items skipped due to conflicts or errors.
    /// </summary>
    public long ObjectsSkipped { get; }

    /// <summary>
    /// Total record validation violations logged.
    /// </summary>
    public long ValidationErrorsCount { get; }

    /// <summary>
    /// Time elapsed since the start of the job.
    /// </summary>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// Estimated time remaining to finish the operation.
    /// </summary>
    public TimeSpan? EstimatedRemainingTime { get; }

    public ImportProgress(
        string currentFile, 
        long objectsImported, 
        long objectsSkipped, 
        long validationErrorsCount, 
        TimeSpan elapsedTime, 
        TimeSpan? estimatedRemainingTime)
    {
        CurrentFile = currentFile;
        ObjectsImported = objectsImported;
        ObjectsSkipped = objectsSkipped;
        ValidationErrorsCount = validationErrorsCount;
        ElapsedTime = elapsedTime;
        EstimatedRemainingTime = estimatedRemainingTime;
    }
}
