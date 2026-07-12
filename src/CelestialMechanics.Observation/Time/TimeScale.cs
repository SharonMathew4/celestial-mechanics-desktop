namespace CelestialMechanics.Observation.Time;

/// <summary>
/// Defines named time speed presets for the simulation clock.
/// </summary>
public enum TimeScalePreset
{
    /// <summary>Paused — no time progression.</summary>
    Paused,

    /// <summary>Real-time: 1 second per second.</summary>
    RealTime,

    /// <summary>1× speed (equivalent to real-time).</summary>
    Speed1x,

    /// <summary>10× speed.</summary>
    Speed10x,

    /// <summary>100× speed.</summary>
    Speed100x,

    /// <summary>1000× speed.</summary>
    Speed1000x,

    /// <summary>1 Julian Day per real second (86,400×).</summary>
    DayPerSecond,

    /// <summary>~1 month per real second (30.44 days × 86,400).</summary>
    MonthPerSecond,

    /// <summary>~1 year per real second (365.25 days × 86,400).</summary>
    YearPerSecond
}
