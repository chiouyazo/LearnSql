using System.Globalization;

namespace LearnSql.Sql.Dialect.Functions;

public static class SqlDateParser
{
    private static readonly DateTime NumericEpoch = new DateTime(1900, 1, 1);

    private static readonly string[] DateOnlyPatterns =
    [
        "yyyy-MM-dd",
        "yyyyMMdd",
        "yyyy/MM/dd",
        "M/d/yyyy",
    ];

    private static readonly string[] DateTimePatterns =
    [
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF",
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
        "yyyyMMdd HH:mm",
        "yyyyMMdd HH:mm:ss",
        "yyyyMMdd HH:mm:ss.FFFFFFF",
        "M/d/yyyy HH:mm:ss",
    ];

    private static readonly string[] TimePatterns = ["HH:mm", "HH:mm:ss", "HH:mm:ss.FFFFFFF"];

    public static bool TryParse(object value, int? style, out SqlDateValue result)
    {
        switch (value)
        {
            case long days:
                result = new SqlDateValue(NumericEpoch.AddDays(days), false, 0);
                return true;
            case double fractionalDays:
                result = new SqlDateValue(NumericEpoch.AddDays(fractionalDays), false, 0);
                return true;
            case string text:
                return TryParseText(text.Trim(), style, out result);
            default:
                result = default;
                return false;
        }
    }

    private static bool TryParseText(string text, int? style, out SqlDateValue result)
    {
        if (style is not null)
        {
            foreach (string pattern in ConvertStyleFormats.ParsePatterns(style.Value))
            {
                if (TryExact(text, pattern, out result))
                    return true;
            }
        }

        foreach (string pattern in DateTimePatterns)
        {
            if (TryExact(text, pattern, out result))
                return true;
        }
        foreach (string pattern in DateOnlyPatterns)
        {
            if (TryExact(text, pattern, out result))
                return true;
        }
        foreach (string pattern in TimePatterns)
        {
            if (TryExact(text, pattern, out result))
                return true;
        }

        result = default;
        return false;
    }

    private static bool TryExact(string text, string pattern, out SqlDateValue result)
    {
        bool parsed = DateTime.TryParseExact(
            text,
            pattern,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime value
        );

        if (!parsed)
        {
            result = default;
            return false;
        }

        bool hasTime = pattern.Contains(':', StringComparison.Ordinal);
        bool isTimeOnly = hasTime && !pattern.Contains('y', StringComparison.Ordinal);
        DateTime normalized = isTimeOnly ? NumericEpoch.Add(value.TimeOfDay) : value;
        result = new SqlDateValue(normalized, !hasTime, CountFractionDigits(text, pattern));
        return true;
    }

    private static int CountFractionDigits(string text, string pattern)
    {
        if (!pattern.Contains('.', StringComparison.Ordinal))
            return 0;

        int dot = text.LastIndexOf('.');
        if (dot < 0)
            return 0;

        int digits = 0;
        for (int index = dot + 1; index < text.Length && char.IsAsciiDigit(text[index]); index++)
            digits++;
        return digits;
    }
}
