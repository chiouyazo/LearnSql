using System.Globalization;

namespace LearnSql.Sql.Dialect.Functions;

public static class ConvertStyleFormats
{
    private static readonly Dictionary<int, string> OutputPatterns = new Dictionary<int, string>
    {
        [1] = "MM/dd/yy",
        [101] = "MM/dd/yyyy",
        [3] = "dd/MM/yy",
        [103] = "dd/MM/yyyy",
        [4] = "dd.MM.yy",
        [104] = "dd.MM.yyyy",
        [5] = "dd-MM-yy",
        [105] = "dd-MM-yyyy",
        [8] = "HH:mm:ss",
        [108] = "HH:mm:ss",
        [11] = "yy/MM/dd",
        [111] = "yyyy/MM/dd",
        [12] = "yyMMdd",
        [112] = "yyyyMMdd",
        [20] = "yyyy-MM-dd HH:mm:ss",
        [120] = "yyyy-MM-dd HH:mm:ss",
        [21] = "yyyy-MM-dd HH:mm:ss.fff",
        [121] = "yyyy-MM-dd HH:mm:ss.fff",
        [23] = "yyyy-MM-dd",
        [126] = "yyyy-MM-ddTHH:mm:ss.fff",
    };

    private static readonly string[] MonthAbbreviations =
    [
        "Jan",
        "Feb",
        "Mar",
        "Apr",
        "May",
        "Jun",
        "Jul",
        "Aug",
        "Sep",
        "Oct",
        "Nov",
        "Dec",
    ];

    private const int DefaultStyle = 0;
    private const int DefaultStyleCentury = 100;

    public static IReadOnlyList<int> SupportedCodes { get; } =
        new[] { DefaultStyle, DefaultStyleCentury }.Concat(OutputPatterns.Keys).Order().ToList();

    public static bool IsSupported(int style)
    {
        return style == DefaultStyle
            || style == DefaultStyleCentury
            || OutputPatterns.ContainsKey(style);
    }

    public static string Format(DateTime value, int style)
    {
        if (style == DefaultStyle || style == DefaultStyleCentury)
            return FormatDefault(value);

        if (OutputPatterns.TryGetValue(style, out string? pattern))
            return value.ToString(pattern, CultureInfo.InvariantCulture);

        throw new ArgumentException($"Style {style} is not supported in the sandbox.");
    }

    public static IReadOnlyList<string> ParsePatterns(int style)
    {
        if (!OutputPatterns.TryGetValue(style, out string? pattern))
            return [];

        bool isDatePattern = !pattern.Contains(':', StringComparison.Ordinal);
        if (!isDatePattern)
            return [pattern];

        return [pattern, pattern + " HH:mm:ss", pattern + " HH:mm", pattern + " HH:mm:ss.FFFFFFF"];
    }

    private static string FormatDefault(DateTime value)
    {
        int hour12 = value.Hour % 12 == 0 ? 12 : value.Hour % 12;
        string meridian = value.Hour < 12 ? "AM" : "PM";
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{MonthAbbreviations[value.Month - 1]} {value.Day, 2} {value.Year} {hour12, 2}:{value.Minute:00}{meridian}"
        );
    }
}
