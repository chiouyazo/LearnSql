using System.Globalization;

namespace LearnSql.Sql.Dialect.Functions;

public static class SqlDateFormatter
{
    private const int MaxFractionDigits = 7;

    public static string FormatDate(DateTime value)
    {
        return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public static string FormatDateTime(DateTime value, int fractionDigits)
    {
        string text = value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        return text + FormatFraction(value, fractionDigits);
    }

    public static string FormatTime(DateTime value, int fractionDigits)
    {
        string text = value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        return text + FormatFraction(value, fractionDigits);
    }

    private static string FormatFraction(DateTime value, int fractionDigits)
    {
        int digits = Math.Clamp(fractionDigits, 0, MaxFractionDigits);
        if (digits == 0)
            return string.Empty;

        long ticks = value.Ticks % TimeSpan.TicksPerSecond;
        string fraction = ticks.ToString("D7", CultureInfo.InvariantCulture);
        return "." + fraction.Substring(0, digits);
    }
}
