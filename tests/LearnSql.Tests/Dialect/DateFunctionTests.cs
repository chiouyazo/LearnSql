using LearnSql.Sql.Dialect.Functions;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Dialect;

public class DateFunctionTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> DateDiffCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT DATEDIFF(year, '2020-12-31', '2021-01-01')", 1L },
            { "SELECT DATEDIFF(year, '2021-01-01', '2021-12-31')", 0L },
            { "SELECT DATEDIFF(year, '2021-01-01', '2020-12-31')", -1L },
            { "SELECT DATEDIFF(yy, '2000-06-01', '2024-01-01')", 24L },
            { "SELECT DATEDIFF(quarter, '2024-03-31', '2024-04-01')", 1L },
            { "SELECT DATEDIFF(quarter, '2024-01-01', '2024-03-31')", 0L },
            { "SELECT DATEDIFF(quarter, '2023-12-31', '2024-01-01')", 1L },
            { "SELECT DATEDIFF(quarter, '2024-04-01', '2024-03-31')", -1L },
            { "SELECT DATEDIFF(month, '2024-01-31', '2024-02-01')", 1L },
            { "SELECT DATEDIFF(month, '2024-02-01', '2024-01-31')", -1L },
            { "SELECT DATEDIFF(month, '2023-12-01', '2024-12-31')", 12L },
            { "SELECT DATEDIFF(week, '2024-03-02', '2024-03-03')", 1L },
            { "SELECT DATEDIFF(week, '2024-03-03', '2024-03-09')", 0L },
            { "SELECT DATEDIFF(week, '2024-03-09', '2024-03-10')", 1L },
            { "SELECT DATEDIFF(week, '2024-03-03', '2024-03-02')", -1L },
            { "SELECT DATEDIFF(week, '2024-03-01', '2024-03-31')", 5L },
            { "SELECT DATEDIFF(iso_week, '2024-03-03', '2024-03-04')", 1L },
            { "SELECT DATEDIFF(iso_week, '2024-03-04', '2024-03-10')", 0L },
            { "SELECT DATEDIFF(day, '2024-03-05', '2024-03-01')", -4L },
            { "SELECT DATEDIFF(day, '2024-02-28', '2024-03-01')", 2L },
            { "SELECT DATEDIFF(dd, '2023-02-28', '2023-03-01')", 1L },
            { "SELECT DATEDIFF(day, '2024-03-01 23:59:59', '2024-03-02 00:00:00')", 1L },
            { "SELECT DATEDIFF(day, '2024-03-01 00:00:00', '2024-03-01 23:59:59')", 0L },
            { "SELECT DATEDIFF(hour, '2024-03-15 10:59:59', '2024-03-15 11:00:00')", 1L },
            { "SELECT DATEDIFF(minute, '2024-03-15 10:00:59', '2024-03-15 10:01:00')", 1L },
            { "SELECT DATEDIFF(second, '2024-03-15 10:00:00.900', '2024-03-15 10:00:01.100')", 1L },
            {
                "SELECT DATEDIFF(millisecond, '2024-03-15 10:00:00.000', '2024-03-15 10:00:01.500')",
                1500L
            },
            { "SELECT DATEDIFF(day, NULL, '2024-03-01')", null },
            { "SELECT DATEDIFF(day, '2024-03-01', NULL)", null },
            { "SELECT DATEDIFF_BIG(day, '2024-03-01', '2024-03-11')", 10L },
            { "SELECT DATEDIFF(day, CAST('2024-03-01' AS DATE), CAST('2024-03-11' AS DATE))", 10L },
        };

    public static TheoryData<string, object?> DateAddCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT DATEADD(month, 1, CAST('2024-01-31' AS DATE))", "2024-02-29" },
            { "SELECT DATEADD(month, 1, CAST('2023-01-31' AS DATE))", "2023-02-28" },
            { "SELECT DATEADD(month, -1, CAST('2024-03-31' AS DATE))", "2024-02-29" },
            { "SELECT DATEADD(month, 12, CAST('2024-02-29' AS DATE))", "2025-02-28" },
            { "SELECT DATEADD(year, 1, CAST('2024-02-29' AS DATE))", "2025-02-28" },
            { "SELECT DATEADD(year, -4, CAST('2024-02-29' AS DATE))", "2020-02-29" },
            { "SELECT DATEADD(quarter, 1, CAST('2024-11-30' AS DATE))", "2025-02-28" },
            { "SELECT DATEADD(day, 1, CAST('2024-02-28' AS DATE))", "2024-02-29" },
            { "SELECT DATEADD(day, -31, CAST('2024-03-01' AS DATE))", "2024-01-30" },
            { "SELECT DATEADD(week, 1, CAST('2024-03-01' AS DATE))", "2024-03-08" },
            {
                "SELECT DATEADD(hour, 1, CAST('2024-01-31 23:30:00' AS DATETIME))",
                "2024-02-01 00:30:00.000"
            },
            {
                "SELECT DATEADD(minute, -31, CAST('2024-01-01 00:30:00' AS DATETIME))",
                "2023-12-31 23:59:00.000"
            },
            {
                "SELECT DATEADD(second, 90, CAST('2024-01-01 00:00:00' AS DATETIME))",
                "2024-01-01 00:01:30.000"
            },
            {
                "SELECT DATEADD(millisecond, 5, CAST('2024-01-01 00:00:00.000' AS DATETIME))",
                "2024-01-01 00:00:00.005"
            },
            {
                "SELECT DATEADD(day, 1, CAST('2024-01-31 10:00:00' AS DATETIME))",
                "2024-02-01 10:00:00.000"
            },
            { "SELECT DATEADD(day, 1, GETDATE())", "2024-03-16 10:20:30.123" },
            { "SELECT DATEADD(month, 1, GETDATE())", "2024-04-15 10:20:30.123" },
            { "SELECT DATEADD(day, 0, CAST('2024-03-01' AS DATE))", "2024-03-01" },
            { "SELECT DATEADD(day, 1, NULL)", null },
            { "SELECT DATEADD(day, NULL, CAST('2024-03-01' AS DATE))", null },
        };

    public static TheoryData<string, object?> DatePartCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT DATEPART(year, '2024-03-15')", 2024L },
            { "SELECT DATEPART(quarter, '2024-05-01')", 2L },
            { "SELECT DATEPART(month, '2024-03-15')", 3L },
            { "SELECT DATEPART(dayofyear, '2024-03-01')", 61L },
            { "SELECT DATEPART(dy, '2023-03-01')", 60L },
            { "SELECT DATEPART(day, '2024-03-15')", 15L },
            { "SELECT DATEPART(week, '2024-01-01')", 1L },
            { "SELECT DATEPART(week, '2021-01-03')", 2L },
            { "SELECT DATEPART(week, '2020-12-31')", 53L },
            { "SELECT DATEPART(week, '2024-12-31')", 53L },
            { "SELECT DATEPART(iso_week, '2024-01-01')", 1L },
            { "SELECT DATEPART(iso_week, '2021-01-03')", 53L },
            { "SELECT DATEPART(iso_week, '2020-12-31')", 53L },
            { "SELECT DATEPART(iso_week, '2024-12-31')", 1L },
            { "SELECT DATEPART(isowk, '2024-03-15')", 11L },
            { "SELECT DATEPART(weekday, '2024-03-17')", 1L },
            { "SELECT DATEPART(weekday, '2024-03-15')", 6L },
            { "SELECT DATEPART(weekday, '2024-03-16')", 7L },
            { "SELECT DATEPART(hour, '2024-03-15 10:20:30')", 10L },
            { "SELECT DATEPART(minute, '2024-03-15 10:20:30')", 20L },
            { "SELECT DATEPART(second, '2024-03-15 10:20:30')", 30L },
            { "SELECT DATEPART(millisecond, '2024-03-15 10:20:30.123')", 123L },
            { "SELECT DATEPART(year, NULL)", null },
            { "SELECT YEAR('2024-03-15')", 2024L },
            { "SELECT MONTH('2024-03-15')", 3L },
            { "SELECT DAY('2024-03-15')", 15L },
            { "SELECT YEAR(GETDATE())", 2024L },
            { "SELECT DATENAME(month, '2024-03-15')", "March" },
            { "SELECT DATENAME(weekday, '2024-03-15')", "Friday" },
            { "SELECT DATENAME(weekday, '2024-03-17')", "Sunday" },
            { "SELECT DATENAME(year, '2024-03-15')", "2024" },
            { "SELECT DATENAME(day, '2024-03-05')", "5" },
        };

    public static TheoryData<string, object?> CalendarCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT EOMONTH('2024-02-10')", "2024-02-29" },
            { "SELECT EOMONTH('2023-02-10')", "2023-02-28" },
            { "SELECT EOMONTH('2024-02-10', 1)", "2024-03-31" },
            { "SELECT EOMONTH('2024-01-31', -1)", "2023-12-31" },
            { "SELECT EOMONTH(NULL)", null },
            { "SELECT DATEFROMPARTS(2024, 2, 29)", "2024-02-29" },
            { "SELECT DATEFROMPARTS(2024, 12, 31)", "2024-12-31" },
            { "SELECT DATEFROMPARTS(2024, NULL, 1)", null },
            { "SELECT ISDATE('2024-02-29')", 1L },
            { "SELECT ISDATE('2023-02-29')", 0L },
            { "SELECT ISDATE('2024-02-30')", 0L },
            { "SELECT ISDATE('abc')", 0L },
            { "SELECT ISDATE(NULL)", 0L },
            { "SELECT ISDATE('2024-03-15 10:20:30')", 1L },
        };

    [Theory]
    [MemberData(nameof(DateDiffCases))]
    public void DateDiff_CountsCrossedBoundariesLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(DateAddCases))]
    public void DateAdd_ClampsToMonthEndAndKeepsTheInputShape(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(DatePartCases))]
    public void DatePartAndDateName_FollowTheSundayFirstWeekModel(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(CalendarCases))]
    public void EndOfMonthDateFromPartsAndIsDate_BehaveLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData(
        "SELECT DATEADD(fortnight, 1, CAST('2024-01-01' AS DATE))",
        "'fortnight' is not a recognized dateadd option."
    )]
    [InlineData(
        "SELECT DATEDIFF(fortnight, '2024-01-01', '2024-01-02')",
        "'fortnight' is not a recognized datediff option."
    )]
    [InlineData(
        "SELECT DATEPART(fortnight, '2024-01-01')",
        "'fortnight' is not a recognized datepart option."
    )]
    [InlineData(
        "SELECT DATEFROMPARTS(2024, 2, 30)",
        "Cannot construct data type date, some of the arguments have values which are not valid."
    )]
    [InlineData(
        "SELECT DATEFROMPARTS(2024, 13, 1)",
        "Cannot construct data type date, some of the arguments have values which are not valid."
    )]
    public void DateFunctions_InvalidInput_ReportTheSqlServerMessageAsFallbackError(
        string sql,
        string expectedMessage
    )
    {
        ScriptOutcome outcome = _database.Run(sql);

        Assert.NotNull(outcome.Error);
        Assert.Equal(50000, outcome.Error.Error.Number);
        Assert.Equal(expectedMessage, outcome.Error.Error.Message);
    }

    [Fact]
    public void DateFunctions_WithUnparsableText_ReportMsSqlError241()
    {
        ScriptOutcome outcome = _database.Run("SELECT DATEDIFF(day, 'abc', '2024-01-01')");

        Assert.NotNull(outcome.Error);
        Assert.Equal(241, outcome.Error.Error.Number);
        Assert.Equal(16, outcome.Error.Error.Severity);
        Assert.Equal(1, outcome.Error.Error.State);
        Assert.Equal(
            "Conversion failed when converting date and/or time from character string.",
            outcome.Error.Error.Message
        );
    }

    [Fact]
    public void DateAdd_AcrossAllDateParts_AgreesWithDateDiffForRoundTrips()
    {
        string[] parts = ["year", "quarter", "month", "day", "week", "hour", "minute", "second"];

        foreach (string part in parts)
        {
            object? diff = _database.Scalar(
                $"SELECT DATEDIFF({part}, CAST('2024-03-15 10:20:30' AS DATETIME), DATEADD({part}, 3, CAST('2024-03-15 10:20:30' AS DATETIME)))"
            );
            Assert.Equal(3L, diff);
        }
    }

    [Fact]
    public void GetDate_UsesTheLocalTimeOfTheTimeProviderWithThreeFractionDigits()
    {
        Assert.Equal("2024-03-15 10:20:30.123", _database.Scalar("SELECT GETDATE()"));
    }

    [Fact]
    public void SysDateTime_UsesSevenFractionDigits()
    {
        Assert.Equal("2024-03-15 10:20:30.1230000", _database.Scalar("SELECT SYSDATETIME()"));
    }

    [Fact]
    public void CurrentTimestamp_IsTranslatedToGetDate()
    {
        Assert.Equal("2024-03-15 10:20:30.123", _database.Scalar("SELECT CURRENT_TIMESTAMP"));
    }

    [Fact]
    public void GetUtcDate_AndSysUtcDateTime_UseUtc()
    {
        Assert.Equal("2024-03-15 10:20:30.123", _database.Scalar("SELECT GETUTCDATE()"));
        Assert.Equal("2024-03-15 10:20:30.1230000", _database.Scalar("SELECT SYSUTCDATETIME()"));
    }

    [Fact]
    public void LocalAndUtcFunctions_WithAZoneOffset_DifferByTheOffset()
    {
        TimeZoneInfo zone = TimeZoneInfo.CreateCustomTimeZone(
            "Test+2",
            TimeSpan.FromHours(2),
            "Test+2",
            "Test+2"
        );
        using SqliteTestDatabase database = new SqliteTestDatabase(
            new SingleDeterministicTimeProvider(
                SingleDeterministicTimeProvider.DefaultInstant,
                zone
            )
        );

        Assert.Equal("2024-03-15 12:20:30.123", database.Scalar("SELECT GETDATE()"));
        Assert.Equal("2024-03-15 10:20:30.123", database.Scalar("SELECT GETUTCDATE()"));
    }

    [Fact]
    public void GetDate_CalledTwiceInOneStatement_ReturnsTheSameInstantFromTheProvider()
    {
        ResultSet result = _database.Query("SELECT GETDATE(), GETDATE()");

        Assert.Equal(result.Rows[0][0], result.Rows[0][1]);
    }

    [Fact]
    public void Registrar_WithoutTimeProvider_StillRegistersTheClockFunctionsWithTheDocumentedFormat()
    {
        using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        new SqliteFunctionRegistrar().Register(connection);
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT GETUTCDATE()";

        string text = (string)command.ExecuteScalar()!;

        Assert.Matches(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}$", text);
    }
}
