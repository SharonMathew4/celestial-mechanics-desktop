using CelestialMechanics.Observation.Events;
using CelestialMechanics.Observation.Services;

namespace CelestialMechanics.Observation.Time;

/// <summary>
/// Top-level time management service for Observation Mode.
/// Orchestrates the <see cref="SimulationClock"/> and exposes a clean API for
/// play, pause, reverse, time scale, and direct time setting.
/// Implements <see cref="ITimeService"/> for DI compatibility.
/// </summary>
public sealed class TimeManager : ITimeService
{
    private readonly SimulationClock _clock;
    private readonly EventBus _eventBus;
    private TimeScalePreset _activePreset = TimeScalePreset.RealTime;

    /// <summary>
    /// Gets the underlying simulation clock.
    /// </summary>
    public SimulationClock Clock => _clock;

    /// <summary>
    /// Gets the active time scale preset.
    /// </summary>
    public TimeScalePreset ActivePreset => _activePreset;

    /// <inheritdoc />
    public double CurrentJulianDate => _clock.CurrentTime.Value;

    /// <inheritdoc />
    public double TimeScale
    {
        get => _clock.SpeedMultiplier;
        set => _clock.SpeedMultiplier = value;
    }

    /// <inheritdoc />
    public bool IsPlaying => _clock.IsRunning;

    /// <summary>
    /// Gets whether the clock is running in reverse.
    /// </summary>
    public bool IsReversed => _clock.IsReversed;

    /// <inheritdoc />
    public event Action<double>? TimeChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeManager"/> class.
    /// </summary>
    /// <param name="eventBus">The event bus for publishing time change events.</param>
    public TimeManager(EventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _clock = new SimulationClock();
    }

    /// <inheritdoc />
    public void Play()
    {
        _clock.IsRunning = true;
        _clock.IsReversed = false;
    }

    /// <inheritdoc />
    public void Pause()
    {
        _clock.IsRunning = false;
    }

    /// <summary>
    /// Sets the clock to run in reverse.
    /// </summary>
    public void Reverse()
    {
        _clock.IsRunning = true;
        _clock.IsReversed = true;
    }

    /// <summary>
    /// Sets the time scale to a named preset.
    /// </summary>
    /// <param name="preset">The time scale preset to apply.</param>
    public void SetTimeScale(TimeScalePreset preset)
    {
        _activePreset = preset;

        if (preset == TimeScalePreset.Paused)
        {
            _clock.IsRunning = false;
            _clock.SpeedMultiplier = 0.0;
        }
        else
        {
            _clock.SpeedMultiplier = SimulationClock.GetMultiplier(preset);
        }
    }

    /// <inheritdoc />
    public void SetTime(double julianDate)
    {
        _clock.SetTime(new JulianDate(julianDate));
        RaiseTimeChanged();
    }

    /// <inheritdoc />
    public void ResetToJ2000()
    {
        _clock.SetTime(JulianDate.J2000);
        _clock.SpeedMultiplier = 1.0;
        _clock.IsReversed = false;
        _clock.IsRunning = false;
        _activePreset = TimeScalePreset.RealTime;
        RaiseTimeChanged();
    }

    /// <summary>
    /// Advances the simulation clock by the real-time delta and raises
    /// a <see cref="UniverseEvent.TimeChanged"/> event if time progressed.
    /// Called each frame from the update loop.
    /// </summary>
    /// <param name="realDeltaSeconds">Real wall-clock time elapsed since last frame, in seconds.</param>
    public void Tick(double realDeltaSeconds)
    {
        if (!_clock.IsRunning)
            return;

        double previousJd = _clock.CurrentTime.Value;
        _clock.Tick(realDeltaSeconds);

        if (_clock.CurrentTime.Value != previousJd)
        {
            RaiseTimeChanged();
        }
    }

    private void RaiseTimeChanged()
    {
        double jd = _clock.CurrentTime.Value;
        TimeChanged?.Invoke(jd);
        _eventBus.Publish(new UniverseEventArgs(UniverseEvent.TimeChanged, jd));
    }
}
