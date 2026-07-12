namespace CelestialMechanics.Observation.Services;

/// <summary>
/// Abstraction for controlling the observation timeline.
/// Manages the current epoch, playback rate, and time stepping
/// for viewing astronomical events at different points in time.
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Current observation time as Julian Date (TDB).
    /// </summary>
    double CurrentJulianDate { get; }

    /// <summary>
    /// Current time scale multiplier. 1.0 = real-time.
    /// </summary>
    double TimeScale { get; set; }

    /// <summary>
    /// Whether time playback is currently advancing.
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Starts time playback.
    /// </summary>
    void Play();

    /// <summary>
    /// Pauses time playback.
    /// </summary>
    void Pause();

    /// <summary>
    /// Sets the observation time to a specific Julian Date.
    /// </summary>
    void SetTime(double julianDate);

    /// <summary>
    /// Resets to the J2000.0 epoch.
    /// </summary>
    void ResetToJ2000();

    /// <summary>
    /// Raised when the current time changes.
    /// </summary>
    event Action<double>? TimeChanged;
}
