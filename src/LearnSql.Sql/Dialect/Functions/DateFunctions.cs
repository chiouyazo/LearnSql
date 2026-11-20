using System.Globalization;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Dialect.Functions;

public sealed class DateFunctions
{
    private const int DefaultDateTimeDigits = 3;
    private const int DateTime2Digits = 7;
    private const string InvalidDateMessage =
        "Cannot construct data type date, some of the arguments have values which are not valid.";

    private static readonly string[] MonthNames =
    [
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December",
    ];

    private static readonly string[] DayNames =
    [
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
    ];

    private readonly TimeProvider _timeProvider;

    public DateFunctions(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public IEnumerable<SqliteScalarFunction> Create()
    {
        yield return new SqliteScalarFunction(
            "GETDATE",
            0,
            _ => SqlDateFormatter.FormatDateTime(LocalNow(), DefaultDateTimeDigits),
            false
        );
        yield return new SqliteScalarFunction(
            "GETUTCDATE",
            0,
            _ => SqlDateFormatter.FormatDateTime(UtcNow(), DefaultDateTimeDigits),
            false
        );
        yield return new SqliteScalarFunction(
            "SYSDATETIME",
            0,
            _ => SqlDateFormatter.FormatDateTime(LocalNow(), DateTime2Digits),
            false
        );
        yield return new SqliteScalarFunction(
            "SYSUTCDATETIME",
            0,
            _ => SqlDateFormatter.FormatDateTime(UtcNow(), DateTime2Digits),
            false
        );
        yield return new SqliteScalarFunction(
            "DATEADD",
            3,
            arguments => DateAdd(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "DATEDIFF",
            3,
            arguments => DateDiff(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "DATEDIFF_BIG",
            3,
            arguments => DateDiff(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction(
            "DATEPART",
            2,
            arguments => DatePartOf(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "DATENAME",
            2,
            arguments => DateName(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "YEAR",
            1,
            arguments => DatePartOf("year", arguments[0])
        );
        yield return new SqliteScalarFunction(
            "MONTH",
            1,
            arguments => DatePartOf("month", arguments[0])
        );
        yield return new SqliteScalarFunction(
            "DAY",
            1,
            arguments => DatePartOf("day", arguments[0])
        );
        yield return new SqliteScalarFunction(
            "EOMONTH",
            1,
            arguments => EndOfMonth(arguments[0], null)
        );
        yield return new SqliteScalarFunction(
            "EOMONTH",
            2,
            arguments => EndOfMonth(arguments[0], arguments[1])
        );
        yield return new SqliteScalarFunction(
            "DATEFROMPARTS",
            3,
            arguments => DateFromParts(arguments[0], arguments[1], arguments[2])
        );
        yield return new SqliteScalarFunction("ISDATE", 1, arguments => IsDate(arguments[0]));
        yield return new SqliteScalarFunction(
            InternalFunctionNames.AddDays,
            2,
            arguments => AddDays(arguments[0], arguments[1])
        );
    }

    public static object? DateAdd(object? partName, object? amount, object? date)
    {
        DatePart part = DatePartParser.Parse(partName, "dateadd");
        if (SqlValue.IsNull(amount) || SqlValue.IsNull(date))
            return null;

        SqlDateValue source = ParseOrThrow(date!);
        long count = SqlValue.ToLong(amount!);
        DateTime shifted = Shift(source.Value, part, count);

        bool keepDateOnly = source.IsDateOnly && !DatePartParser.IsTimePart(part);
        return keepDateOnly
            ? SqlDateFormatter.FormatDate(shifted)
            : FormatAsDateTime(shifted, source);
    }

    public static object? DateDiff(object? partName, object? start, object? end)
    {
        DatePart part = DatePartParser.Parse(partName, "datediff");
        if (SqlValue.IsNull(start) || SqlValue.IsNull(end))
            return null;

        DateTime from = ParseOrThrow(start!).Value;
        DateTime to = ParseOrThrow(end!).Value;
        return Boundary(to, part) - Boundary(from, part);
    }

    public static object? DatePartOf(object? partName, object? date)
    {
        DatePart part = DatePartParser.Parse(partName, "datepart");
        if (SqlValue.IsNull(date))
            return null;

        return Extract(ParseOrThrow(date!).Value, part);
    }

    public static object? DateName(object? partName, object? date)
    {
        DatePart part = DatePartParser.Parse(partName, "datename");
        if (SqlValue.IsNull(date))
            return null;

        DateTime value = ParseOrThrow(date!).Value;
        switch (part)
        {
            case DatePart.Month:
                return MonthNames[value.Month - 1];
            case DatePart.Weekday:
                return DayNames[(int)value.DayOfWeek];
            default:
                return Extract(value, part).ToString(CultureInfo.InvariantCulture);
        }
    }

    private static object? EndOfMonth(object? date, object? monthOffset)
    {
        if (SqlValue.IsNull(date))
            return null;

        DateTime value = ParseOrThrow(date!).Value;
        if (!SqlValue.IsNull(monthOffset))
            value = value.AddMonths(checked((int)SqlValue.ToLong(monthOffset!)));

        int lastDay = DateTime.DaysInMonth(value.Year, value.Month);
        return SqlDateFormatter.FormatDate(new DateTime(value.Year, value.Month, lastDay));
    }

    private static object? DateFromParts(object? year, object? month, object? day)
    {
        if (SqlValue.IsNull(year) || SqlValue.IsNull(month) || SqlValue.IsNull(day))
            return null;

        try
        {
            DateTime value = new DateTime(
                checked((int)SqlValue.ToLong(year!)),
                checked((int)SqlValue.ToLong(month!)),
                checked((int)SqlValue.ToLong(day!))
            );
            return SqlDateFormatter.FormatDate(value);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentException(InvalidDateMessage);
        }
    }

    private static object IsDate(object? value)
    {
        if (SqlValue.IsNull(value))
            return 0L;
        return SqlDateParser.TryParse(value!, null, out SqlDateValue _) ? 1L : 0L;
    }

    private static object? AddDays(object? date, object? days)
    {
        if (SqlValue.IsNull(date) || SqlValue.IsNull(days))
            return null;

        SqlDateValue source = ParseOrThrow(date!);
        double amount = SqlValue.ToDouble(days!);
        DateTime shifted = source.Value.AddTicks((long)Math.Round(amount * TimeSpan.TicksPerDay));

        bool wholeDays = Math.Abs(amount - Math.Truncate(amount)) < double.Epsilon;
        return source.IsDateOnly && wholeDays
            ? SqlDateFormatter.FormatDate(shifted)
            : FormatAsDateTime(shifted, source);
    }

    private DateTime LocalNow()
    {
        return _timeProvider.GetLocalNow().DateTime;
    }

    private DateTime UtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    private static string FormatAsDateTime(DateTime value, SqlDateValue source)
    {
        return SqlDateFormatter.FormatDateTime(
            value,
            Math.Max(DefaultDateTimeDigits, source.FractionDigits)
        );
    }

    private static SqlDateValue ParseOrThrow(object value)
    {
        if (SqlDateParser.TryParse(value, null, out SqlDateValue parsed))
            return parsed;

        throw MsSqlRaisedErrors.DateConversionFailed();
    }

    private static DateTime Shift(DateTime value, DatePart part, long count)
    {
        try
        {
            switch (part)
            {
                case DatePart.Year:
                    return value.AddYears(checked((int)count));
                case DatePart.Quarter:
                    return value.AddMonths(checked((int)count * 3));
                case DatePart.Month:
                    return value.AddMonths(checked((int)count));
                case DatePart.Day:
                case DatePart.DayOfYear:
                case DatePart.Weekday:
                    return value.AddDays(count);
                case DatePart.Week:
                case DatePart.IsoWeek:
                    return value.AddDays(count * 7);
                case DatePart.Hour:
                    return value.AddHours(count);
                case DatePart.Minute:
                    return value.AddMinutes(count);
                case DatePart.Second:
                    return value.AddSeconds(count);
                case DatePart.Millisecond:
                    return value.AddMilliseconds(count);
                case DatePart.Microsecond:
                    return value.AddTicks(count * 10);
                default:
                    return value.AddTicks(count / 100);
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentException(
                "The date function resulted in an overflow of the date range."
            );
        }
    }

    private static long Boundary(DateTime value, DatePart part)
    {
        switch (part)
        {
            case DatePart.Year:
                return value.Year;
            case DatePart.Quarter:
                return value.Year * 4L + (value.Month - 1) / 3;
            case DatePart.Month:
                return value.Year * 12L + (value.Month - 1);
            case DatePart.Day:
            case DatePart.DayOfYear:
            case DatePart.Weekday:
                return DateOnly.FromDateTime(value).DayNumber;
            case DatePart.Week:
                return (DateOnly.FromDateTime(value).DayNumber + 1L) / 7;
            case DatePart.IsoWeek:
                return DateOnly.FromDateTime(value).DayNumber / 7;
            case DatePart.Hour:
                return value.Ticks / TimeSpan.TicksPerHour;
            case DatePart.Minute:
                return value.Ticks / TimeSpan.TicksPerMinute;
            case DatePart.Second:
                return value.Ticks / TimeSpan.TicksPerSecond;
            case DatePart.Millisecond:
                return value.Ticks / TimeSpan.TicksPerMillisecond;
            case DatePart.Microsecond:
                return value.Ticks / 10;
            default:
                return value.Ticks * 100;
        }
    }

    private static long Extract(DateTime value, DatePart part)
    {
        switch (part)
        {
            case DatePart.Year:
                return value.Year;
            case DatePart.Quarter:
                return (value.Month - 1) / 3 + 1;
            case DatePart.Month:
                return value.Month;
            case DatePart.DayOfYear:
                return value.DayOfYear;
            case DatePart.Day:
                return value.Day;
            case DatePart.Week:
                return WeekOfYear(value);
            case DatePart.Weekday:
                return (int)value.DayOfWeek + 1;
            case DatePart.IsoWeek:
                return ISOWeek.GetWeekOfYear(value);
            case DatePart.Hour:
                return value.Hour;
            case DatePart.Minute:
                return value.Minute;
            case DatePart.Second:
                return value.Second;
            case DatePart.Millisecond:
                return value.Millisecond;
            case DatePart.Microsecond:
                return value.Ticks % TimeSpan.TicksPerSecond / 10;
            default:
                return value.Ticks % TimeSpan.TicksPerSecond * 100;
        }
    }

    private static long WeekOfYear(DateTime value)
    {
        int firstDayOffset = (int)new DateTime(value.Year, 1, 1).DayOfWeek;
        return (value.DayOfYear - 1 + firstDayOffset) / 7 + 1;
    }
}
