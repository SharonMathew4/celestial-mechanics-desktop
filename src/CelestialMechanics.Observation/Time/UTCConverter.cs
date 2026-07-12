namespace CelestialMechanics.Observation.Time;

/// <summary>
/// Provides bidirectional conversion between UTC <see cref="DateTime"/> and Julian Date values.
/// Uses the standard astronomical algorithm for the proleptic Gregorian calendar.
/// </summary>
public static class UTCConverter
{
    /// <summary>
    /// Converts a UTC <see cref="DateTime"/> to a Julian Date.
    /// </summary>
    /// <param name="dateTime">The UTC date and time.</param>
    /// <returns>The corresponding Julian Date value.</returns>
    public static double DateTimeToJulianDate(DateTime dateTime)
    {
        int y = dateTime.Year;
        int m = dateTime.Month;
        int d = dateTime.Day;

        // Adjust months so Jan/Feb are months 13/14 of the previous year
        if (m <= 2)
        {
            y -= 1;
            m += 12;
        }

        int a = y / 100;
        int b = 2 - a + a / 4;

        double jd = System.Math.Floor(365.25 * (y + 4716))
                   + System.Math.Floor(30.6001 * (m + 1))
                   + d + b - 1524.5;

        // Add fractional day for hours, minutes, seconds
        double fraction = (dateTime.Hour + dateTime.Minute / 60.0 + dateTime.Second / 3600.0
                          + dateTime.Millisecond / 3_600_000.0) / 24.0;

        return jd + fraction;
    }

    /// <summary>
    /// Converts a Julian Date to a UTC <see cref="DateTime"/>.
    /// </summary>
    /// <param name="julianDate">The Julian Date value.</param>
    /// <returns>The corresponding UTC <see cref="DateTime"/>.</returns>
    public static DateTime JulianDateToDateTime(double julianDate)
    {
        double jd = julianDate + 0.5;
        int z = (int)System.Math.Floor(jd);
        double f = jd - z;

        int a;
        if (z < 2299161)
        {
            a = z;
        }
        else
        {
            int alpha = (int)System.Math.Floor((z - 1867216.25) / 36524.25);
            a = z + 1 + alpha - alpha / 4;
        }

        int b = a + 1524;
        int c = (int)System.Math.Floor((b - 122.1) / 365.25);
        int d = (int)System.Math.Floor(365.25 * c);
        int e = (int)System.Math.Floor((b - d) / 30.6001);

        int day = b - d - (int)System.Math.Floor(30.6001 * e);
        int month = e < 14 ? e - 1 : e - 13;
        int year = month > 2 ? c - 4716 : c - 4715;

        // Convert fractional day to hours, minutes, seconds
        double totalHours = f * 24.0;
        int hours = (int)totalHours;
        double remainingMinutes = (totalHours - hours) * 60.0;
        int minutes = (int)remainingMinutes;
        double remainingSeconds = (remainingMinutes - minutes) * 60.0;
        int seconds = (int)remainingSeconds;
        int milliseconds = (int)((remainingSeconds - seconds) * 1000.0);

        // Clamp to valid ranges
        hours = System.Math.Clamp(hours, 0, 23);
        minutes = System.Math.Clamp(minutes, 0, 59);
        seconds = System.Math.Clamp(seconds, 0, 59);
        milliseconds = System.Math.Clamp(milliseconds, 0, 999);

        return new DateTime(year, month, day, hours, minutes, seconds, milliseconds, DateTimeKind.Utc);
    }
}
