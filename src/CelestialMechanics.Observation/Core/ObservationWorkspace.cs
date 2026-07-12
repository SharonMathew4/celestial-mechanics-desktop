namespace CelestialMechanics.Observation.Core;

/// <summary>
/// Maintains the current workspace state for an Observation session.
/// Tracks active selections, loaded catalogs, and session metadata.
/// </summary>
public sealed class ObservationWorkspace
{
    /// <summary>
    /// Unique identifier for this workspace session.
    /// </summary>
    public Guid SessionId { get; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the workspace was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// The currently selected object identifier, if any.
    /// </summary>
    public string? SelectedObjectId { get; set; }

    /// <summary>
    /// Whether the workspace has been modified since last save.
    /// </summary>
    public bool IsDirty { get; set; }

    /// <summary>
    /// Current simulation/observation time as Julian Date.
    /// Default: J2000.0 epoch (2000-01-01T12:00:00 TDB).
    /// </summary>
    public double CurrentJulianDate { get; set; } = 2_451_545.0;

    /// <summary>
    /// Resets the workspace to its default state.
    /// </summary>
    public void Reset()
    {
        SelectedObjectId = null;
        IsDirty = false;
        CurrentJulianDate = 2_451_545.0;
    }
}
