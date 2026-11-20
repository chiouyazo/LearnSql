namespace LearnSql.Sql.Dialect.Functions;

public static class DatePartParser
{
    private static readonly Dictionary<string, DatePart> Names = new Dictionary<string, DatePart>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["year"] = DatePart.Year,
        ["yy"] = DatePart.Year,
        ["yyyy"] = DatePart.Year,
        ["quarter"] = DatePart.Quarter,
        ["qq"] = DatePart.Quarter,
        ["q"] = DatePart.Quarter,
        ["month"] = DatePart.Month,
        ["mm"] = DatePart.Month,
        ["m"] = DatePart.Month,
        ["dayofyear"] = DatePart.DayOfYear,
        ["dy"] = DatePart.DayOfYear,
        ["y"] = DatePart.DayOfYear,
        ["day"] = DatePart.Day,
        ["dd"] = DatePart.Day,
        ["d"] = DatePart.Day,
        ["week"] = DatePart.Week,
        ["wk"] = DatePart.Week,
        ["ww"] = DatePart.Week,
        ["weekday"] = DatePart.Weekday,
        ["dw"] = DatePart.Weekday,
        ["w"] = DatePart.Weekday,
        ["hour"] = DatePart.Hour,
        ["hh"] = DatePart.Hour,
        ["minute"] = DatePart.Minute,
        ["mi"] = DatePart.Minute,
        ["n"] = DatePart.Minute,
        ["second"] = DatePart.Second,
        ["ss"] = DatePart.Second,
        ["s"] = DatePart.Second,
        ["millisecond"] = DatePart.Millisecond,
        ["ms"] = DatePart.Millisecond,
        ["microsecond"] = DatePart.Microsecond,
        ["mcs"] = DatePart.Microsecond,
        ["nanosecond"] = DatePart.Nanosecond,
        ["ns"] = DatePart.Nanosecond,
        ["iso_week"] = DatePart.IsoWeek,
        ["isowk"] = DatePart.IsoWeek,
        ["isoww"] = DatePart.IsoWeek,
    };

    public static DatePart Parse(object? name, string functionName)
    {
        string text = name is null ? string.Empty : SqlValue.ToText(name);
        if (Names.TryGetValue(text.Trim(), out DatePart part))
            return part;

        throw new ArgumentException($"'{text}' is not a recognized {functionName} option.");
    }

    public static bool IsTimePart(DatePart part)
    {
        return part
            is DatePart.Hour
                or DatePart.Minute
                or DatePart.Second
                or DatePart.Millisecond
                or DatePart.Microsecond
                or DatePart.Nanosecond;
    }
}
