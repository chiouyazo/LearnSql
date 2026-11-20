using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class MathFunctionTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> RoundCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT ROUND(2.675, 2)", 2.68 },
            { "SELECT ROUND(1.005, 2)", 1.01 },
            { "SELECT ROUND(2.5, 0)", 3.0 },
            { "SELECT ROUND(3.5, 0)", 4.0 },
            { "SELECT ROUND(-2.5, 0)", -3.0 },
            { "SELECT ROUND(0.5, 0)", 1.0 },
            { "SELECT ROUND(-0.5, 0)", -1.0 },
            { "SELECT ROUND(1234.5678, -2)", 1200.0 },
            { "SELECT ROUND(1234.5678, -1)", 1230.0 },
            { "SELECT ROUND(1250, -2)", 1300L },
            { "SELECT ROUND(-1250, -2)", -1300L },
            { "SELECT ROUND(1249, -2)", 1200L },
            { "SELECT ROUND(999.99, -1)", 1000.0 },
            { "SELECT ROUND(5, 1)", 5L },
            { "SELECT ROUND(5, 0)", 5L },
            { "SELECT ROUND(1.2345, 2, 1)", 1.23 },
            { "SELECT ROUND(-1.2399, 2, 1)", -1.23 },
            { "SELECT ROUND(12345.6789, 2, 1)", 12345.67 },
            { "SELECT ROUND(-12345.6789, -2, 1)", -12300.0 },
            { "SELECT ROUND(1.99, 1, 1)", 1.9 },
            { "SELECT ROUND(1.2345, 2, 0)", 1.23 },
            { "SELECT ROUND(1.2355, 3)", 1.236 },
            { "SELECT ROUND(NULL, 1)", null },
            { "SELECT ROUND(1.5, NULL)", null },
        };

    public static TheoryData<string, object?> OtherCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CEILING(1.2)", 2.0 },
            { "SELECT CEILING(-1.2)", -1.0 },
            { "SELECT CEILING(5)", 5L },
            { "SELECT FLOOR(1.8)", 1.0 },
            { "SELECT FLOOR(-1.2)", -2.0 },
            { "SELECT FLOOR(5)", 5L },
            { "SELECT CEILING(NULL)", null },
            { "SELECT POWER(2, 10)", 1024L },
            { "SELECT POWER(2, 0)", 1L },
            { "SELECT POWER(2, 0.5)", 1.4142135623730951 },
            { "SELECT POWER(2, -1)", 0.5 },
            { "SELECT POWER(2.0, 3)", 8.0 },
            { "SELECT POWER(NULL, 2)", null },
            { "SELECT SQRT(9)", 3.0 },
            { "SELECT SQRT(2)", 1.4142135623730951 },
            { "SELECT SQRT(0)", 0.0 },
            { "SELECT SQRT(NULL)", null },
            { "SELECT SQUARE(4)", 16L },
            { "SELECT SQUARE(1.5)", 2.25 },
            { "SELECT SQUARE(NULL)", null },
            { "SELECT SIGN(-5)", -1L },
            { "SELECT SIGN(0)", 0L },
            { "SELECT SIGN(7)", 1L },
            { "SELECT SIGN(-0.1)", -1.0 },
            { "SELECT SIGN(NULL)", null },
            { "SELECT LOG(1)", 0.0 },
            { "SELECT LOG(8, 2)", 3.0 },
            { "SELECT LOG10(1000)", 3.0 },
            { "SELECT LOG10(1)", 0.0 },
            { "SELECT EXP(0)", 1.0 },
            { "SELECT EXP(NULL)", null },
            { "SELECT PI()", 3.141592653589793 },
            { "SELECT ABS(-5)", 5L },
        };

    [Theory]
    [MemberData(nameof(RoundCases))]
    public void Round_UsesExactDecimalRoundingAwayFromZero(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(OtherCases))]
    public void MathFunction_BehavesLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData("SELECT SQRT(-1)")]
    [InlineData("SELECT LOG(0)")]
    [InlineData("SELECT LOG(-1)")]
    [InlineData("SELECT LOG10(0)")]
    [InlineData("SELECT LOG(8, 1)")]
    [InlineData("SELECT LOG(8, 0)")]
    public void MathFunction_InvalidArgument_FailsWithTheFloatingPointMessage(string sql)
    {
        ScriptOutcome outcome = _database.Run(sql);

        Assert.NotNull(outcome.Error);
        Assert.Equal("An invalid floating point operation occurred.", outcome.Error.Error.Message);
    }

    [Fact]
    public void Round_OverTableColumn_RoundsEveryRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE P (Price DECIMAL(8,3)); INSERT INTO P VALUES (1.005), (2.675), (3.994);"
        );

        ResultSet result = _database.Query("SELECT ROUND(Price, 2) FROM P ORDER BY rowid");

        Assert.Equal([1.01, 2.68, 3.99], result.Column());
    }
}
