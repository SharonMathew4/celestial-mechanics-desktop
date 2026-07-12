namespace CelestialMechanics.Observation.Time;

/// <summary>
/// Represents an astronomical Julian Date (JD) value with conversion utilities.
/// Julian Date is the continuous count of days since the beginning of the Julian Period
/// (January 1, 4713 BC, proleptic Julian calendar).
/// </summary>
public readonly struct JulianDate : IEquatable<JulianDate>, IComparable<JulianDate>
{
    /// <summary>
    /// The raw Julian Date value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// J2000.0 epoch: January 1, 2000, 12:00 TT = JD 2451545.0
    /// </summary>
    public static readonly JulianDate J2000 = new(2451545.0);

    /// <summary>
    /// Julian Date of the Unix epoch (January 1, 1970, 00:00 UTC).
    /// </summary>
    public static readonly JulianDate UnixEpoch = new(2440587.5);

    /// <summary>
    /// Number of seconds in a Julian Day.
    /// </summary>
    public const double SecondsPerDay = 86400.0;

    /// <summary>
    /// Average days per Julian Year.
    /// </summary>
    public const double DaysPerYear = 365.25;

    /// <summary>
    /// Average days per month (1/12 of a Julian Year).
    /// </summary>
    public const double DaysPerMonth = 30.4375;

    /// <summary>
    /// Initializes a new instance of the <see cref="JulianDate"/> struct.
    /// </summary>
    /// <param name="value">The Julian Date value.</param>
    public JulianDate(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="JulianDate"/> from a UTC <see cref="DateTime"/>.
    /// </summary>
    public static JulianDate FromDateTime(DateTime dateTime)
    {
        return new JulianDate(UTCConverter.DateTimeToJulianDate(dateTime));
    }

    /// <summary>
    /// Converts this Julian Date to a UTC <see cref="DateTime"/>.
    /// </summary>
    public DateTime ToDateTime()
    {
        return UTCConverter.JulianDateToDateTime(Value);
    }

    /// <summary>
    /// Creates a <see cref="JulianDate"/> representing the current UTC time.
    /// </summary>
    public static JulianDate Now => FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Adds a number of days to this Julian Date.
    /// </summary>
    public JulianDate AddDays(double days) => new(Value + days);

    /// <summary>
    /// Adds a number of seconds to this Julian Date.
    /// </summary>
    public JulianDate AddSeconds(double seconds) => new(Value + seconds / SecondsPerDay);

    /// <summary>
    /// Returns the difference in days between two Julian Dates.
    /// </summary>
    public double DaysSince(JulianDate other) => Value - other.Value;

    // ── Operators ───────────────────────────────────────────────────

    public static JulianDate operator +(JulianDate jd, double days) => new(jd.Value + days);
    public static JulianDate operator -(JulianDate jd, double days) => new(jd.Value - days);
    public static double operator -(JulianDate a, JulianDate b) => a.Value - b.Value;
    public static bool operator ==(JulianDate left, JulianDate right) => left.Value == right.Value;
    public static bool operator !=(JulianDate left, JulianDate right) => left.Value != right.Value;
    public static bool operator <(JulianDate left, JulianDate right) => left.Value < right.Value;
    public static bool operator >(JulianDate left, JulianDate right) => left.Value > right.Value;
    public static bool operator <=(JulianDate left, JulianDate right) => left.Value <= right.Value;
    public static bool operator >=(JulianDate left, JulianDate right) => left.Value >= right.Value;

    // ── Equality / Comparison ───────────────────────────────────────

    public bool Equals(JulianDate other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is JulianDate other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public int CompareTo(JulianDate other) => Value.CompareTo(other.Value);

    public override string ToString() => $"JD {Value:F6}";
}
