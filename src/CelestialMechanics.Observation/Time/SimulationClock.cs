namespace CelestialMechanics.Observation.Time;

/// <summary>
/// Internal clock that tracks simulation time as a Julian Date, advancing
/// at a configurable rate multiplier. Supports forward and reverse time flow.
/// </summary>
public sealed class SimulationClock
{
    private JulianDate _currentTime;

    /// <summary>
    /// Gets the current simulation time as a Julian Date.
    /// </summary>
    public JulianDate CurrentTime => _currentTime;

    /// <summary>
    /// Gets or sets the speed multiplier. 1.0 = real-time.
    /// Negative values are not used; use <see cref="IsReversed"/> instead.
    /// </summary>
    public double SpeedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets whether time flows in reverse.
    /// </summary>
    public bool IsReversed { get; set; }

    /// <summary>
    /// Gets or sets whether the clock is currently ticking.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationClock"/> class
    /// starting at the J2000.0 epoch.
    /// </summary>
    public SimulationClock()
    {
        _currentTime = JulianDate.J2000;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationClock"/> class
    /// starting at the specified Julian Date.
    /// </summary>
    /// <param name="startTime">The initial simulation time.</param>
    public SimulationClock(JulianDate startTime)
    {
        _currentTime = startTime;
    }

    /// <summary>
    /// Advances (or reverses) the clock by the specified real-time delta in seconds.
    /// The actual simulation time advancement is <c>realDeltaSeconds × SpeedMultiplier</c>.
    /// </summary>
    /// <param name="realDeltaSeconds">Real wall-clock time elapsed since last tick, in seconds.</param>
    /// <returns>The new current time after the tick.</returns>
    public JulianDate Tick(double realDeltaSeconds)
    {
        if (!IsRunning || realDeltaSeconds <= 0.0)
            return _currentTime;

        double direction = IsReversed ? -1.0 : 1.0;
        double simulatedSeconds = realDeltaSeconds * SpeedMultiplier * direction;

        _currentTime = _currentTime.AddSeconds(simulatedSeconds);
        return _currentTime;
    }

    /// <summary>
    /// Sets the current time to a specific Julian Date.
    /// </summary>
    /// <param name="time">The Julian Date to set.</param>
    public void SetTime(JulianDate time)
    {
        _currentTime = time;
    }

    /// <summary>
    /// Resets the clock to J2000.0 and stops it.
    /// </summary>
    public void Reset()
    {
        _currentTime = JulianDate.J2000;
        SpeedMultiplier = 1.0;
        IsReversed = false;
        IsRunning = false;
    }

    /// <summary>
    /// Gets the multiplier value for a named time scale preset.
    /// </summary>
    /// <param name="preset">The time scale preset.</param>
    /// <returns>The speed multiplier in seconds-per-second.</returns>
    public static double GetMultiplier(TimeScalePreset preset) => preset switch
    {
        TimeScalePreset.Paused => 0.0,
        TimeScalePreset.RealTime => 1.0,
        TimeScalePreset.Speed1x => 1.0,
        TimeScalePreset.Speed10x => 10.0,
        TimeScalePreset.Speed100x => 100.0,
        TimeScalePreset.Speed1000x => 1000.0,
        TimeScalePreset.DayPerSecond => JulianDate.SecondsPerDay,
        TimeScalePreset.MonthPerSecond => JulianDate.DaysPerMonth * JulianDate.SecondsPerDay,
        TimeScalePreset.YearPerSecond => JulianDate.DaysPerYear * JulianDate.SecondsPerDay,
        _ => 1.0
    };
}
