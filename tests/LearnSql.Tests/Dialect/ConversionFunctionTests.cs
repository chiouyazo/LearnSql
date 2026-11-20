using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Functions;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class ConversionFunctionTests : IDisposable
{
    private const string Sample = "CAST('2024-03-05 22:20:30.123' AS DATETIME)";

    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<int, string> StyleExpectations() =>
        new TheoryData<int, string>
        {
            { 0, "Mar  5 2024 10:20PM" },
            { 100, "Mar  5 2024 10:20PM" },
            { 1, "03/05/24" },
            { 101, "03/05/2024" },
            { 3, "05/03/24" },
            { 103, "05/03/2024" },
            { 4, "05.03.24" },
            { 104, "05.03.2024" },
            { 5, "05-03-24" },
            { 105, "05-03-2024" },
            { 8, "22:20:30" },
            { 108, "22:20:30" },
            { 11, "24/03/05" },
            { 111, "2024/03/05" },
            { 12, "240305" },
            { 112, "20240305" },
            { 20, "2024-03-05 22:20:30" },
            { 120, "2024-03-05 22:20:30" },
            { 21, "2024-03-05 22:20:30.123" },
            { 121, "2024-03-05 22:20:30.123" },
            { 23, "2024-03-05" },
            { 126, "2024-03-05T22:20:30.123" },
        };

    public static TheoryData<string, object?> CastCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CAST('12' AS INT)", 12L },
            { "SELECT CAST(' 12 ' AS INT)", 12L },
            { "SELECT CAST(3.7 AS INT)", 3L },
            { "SELECT CAST(-3.7 AS INT)", -3L },
            { "SELECT CAST('1.5' AS FLOAT)", 1.5 },
            { "SELECT CAST('1e3' AS FLOAT)", 1000.0 },
            { "SELECT CAST(1.2345 AS DECIMAL(5,2))", 1.23 },
            { "SELECT CAST(5 AS VARCHAR(3))", "5" },
            { "SELECT CAST('abcdef' AS VARCHAR(3))", "abc" },
            { "SELECT CAST('abcdef' AS VARCHAR(MAX))", "abcdef" },
            { "SELECT CAST('abcdef' AS NVARCHAR(MAX))", "abcdef" },
            { "SELECT CAST(123456 AS VARCHAR(5))", "12345" },
            { "SELECT CAST('2024-03-15' AS DATE)", "2024-03-15" },
            { "SELECT CAST('2024-03-15 10:20:30' AS DATE)", "2024-03-15" },
            { "SELECT CAST('2024-03-15' AS DATETIME)", "2024-03-15 00:00:00.000" },
            { "SELECT CAST('2024-03-15 10:20:30.5' AS DATETIME)", "2024-03-15 10:20:30.500" },
            { "SELECT CAST('2024-03-15' AS DATETIME2)", "2024-03-15 00:00:00.0000000" },
            { "SELECT CAST('10:20' AS TIME)", "10:20:00.0000000" },
            { "SELECT CAST('true' AS BIT)", 1L },
            { "SELECT CAST('FALSE' AS BIT)", 0L },
            { "SELECT CAST(5 AS BIT)", 1L },
            { "SELECT CAST(0 AS BIT)", 0L },
            { "SELECT CAST(NULL AS INT)", null },
            { "SELECT CAST(NULL AS VARCHAR(5))", null },
            { "SELECT CAST(NULL AS DATE)", null },
            { "SELECT CAST(NULL AS BIT)", null },
        };

    public static TheoryData<string, object?> ConvertCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CONVERT(INT, '12')", 12L },
            { "SELECT CONVERT(VARCHAR(5), 123456)", "12345" },
            { "SELECT CONVERT(DATE, '15/03/2024', 103)", "2024-03-15" },
            { "SELECT CONVERT(DATE, '03/15/2024', 101)", "2024-03-15" },
            { "SELECT CONVERT(DATE, '20240315', 112)", "2024-03-15" },
            { "SELECT CONVERT(DATE, '2024-03-15')", "2024-03-15" },
            { "SELECT CONVERT(DATETIME, '03/15/2024', 101)", "2024-03-15 00:00:00.000" },
            { "SELECT CONVERT(DATETIME, '15.03.2024 10:20:30', 104)", "2024-03-15 10:20:30.000" },
            { "SELECT CONVERT(DATETIME2, '2024-03-15')", "2024-03-15 00:00:00.0000000" },
            { "SELECT CONVERT(VARCHAR(10), CAST('2024-03-15' AS DATE), 103)", "15/03/2024" },
            { "SELECT CONVERT(VARCHAR(8), CAST('2024-03-15' AS DATE), 112)", "20240315" },
            {
                "SELECT CONVERT(VARCHAR(10), CAST('2024-03-15 10:20:30' AS DATETIME), 120)",
                "2024-03-15"
            },
            {
                "SELECT CONVERT(VARCHAR(19), CAST('2024-03-15 10:20:30' AS DATETIME), 120)",
                "2024-03-15 10:20:30"
            },
            { "SELECT CONVERT(VARCHAR(10), CAST(NULL AS DATE), 103)", null },
        };

    public static TheoryData<string, object?> FormatCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT FORMAT(1234.5, 'N2')", "1,234.50" },
            { "SELECT FORMAT(1234, 'N0')", "1,234" },
            { "SELECT FORMAT(5, 'D3')", "005" },
            { "SELECT FORMAT(-5, 'D3')", "-005" },
            { "SELECT FORMAT(255, 'X')", "FF" },
            { "SELECT FORMAT(1234.5, 'N2', '')", "1,234.50" },
            { "SELECT FORMAT(CAST('2024-03-15' AS DATE), 'yyyy-MM-dd')", "2024-03-15" },
            { "SELECT FORMAT(CAST('2024-03-15' AS DATE), 'dd.MM.yyyy')", "15.03.2024" },
            { "SELECT FORMAT(CAST('2024-03-15' AS DATE), 'MMMM')", "March" },
            { "SELECT FORMAT(CAST('2024-03-15' AS DATE), 'dddd')", "Friday" },
            { "SELECT FORMAT(CAST('2024-03-15 10:20:30' AS DATETIME), 'HH:mm')", "10:20" },
            { "SELECT FORMAT(NULL, 'N2')", null },
            { "SELECT FORMAT(5, NULL)", null },
            { "SELECT FORMAT('abc', 'N2')", "abc" },
        };

    [Theory]
    [MemberData(nameof(StyleExpectations))]
    public void Convert_WithSupportedStyle_FormatsLikeSqlServer(int style, string expected)
    {
        object? actual = _database.Scalar($"SELECT CONVERT(VARCHAR(30), {Sample}, {style})");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Convert_StyleExpectations_CoverEveryStyleTheSandboxClaimsToSupport()
    {
        HashSet<int> covered = [.. StyleExpectations().Select((object[] row) => (int)row[0])];

        Assert.Equal(ConvertStyleFormats.SupportedCodes.Order(), covered.Order());
    }

    [Theory]
    [MemberData(nameof(CastCases))]
    public void Cast_ConvertsLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(ConvertCases))]
    public void Convert_ConvertsLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(FormatCases))]
    public void Format_UsesNetFormatStringsWithTheInvariantCulture(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData("SELECT CONVERT(DATE, 'garbage')")]
    [InlineData("SELECT CONVERT(DATE, '31/02/2024', 103)")]
    [InlineData("SELECT CAST('garbage' AS DATE)")]
    [InlineData("SELECT CAST('2024-13-01' AS DATETIME)")]
    public void DateConversion_OfInvalidText_FailsWithTheSqlServerMessage(string sql)
    {
        ScriptOutcome outcome = _database.Run(sql);

        SqlErrorAssert.Raised(
            outcome,
            241,
            16,
            1,
            "Conversion failed when converting date and/or time from character string."
        );
    }

    [Fact]
    public void Convert_WithUnsupportedStyle_IsRejectedBeforeExecution()
    {
        DialectTranslationResult result = _database.Translate(
            "SELECT CONVERT(VARCHAR(10), GETDATE(), 999)"
        );

        Assert.Equal("ConvertStyle", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void Convert_WithStyleFromVariable_IsRejectedBeforeExecution()
    {
        DialectTranslationResult result = _database.Translate(
            "DECLARE @s INT = 103; SELECT CONVERT(VARCHAR(10), GETDATE(), @s)"
        );

        Assert.Equal("ConvertStyle", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public void Cast_NonNumericTextToInt_FailsWithMsSqlError245()
    {
        ScriptOutcome outcome = _database.Run("SELECT CAST('abc' AS INT)");

        SqlErrorAssert.Raised(
            outcome,
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'abc' to data type int."
        );
    }

    [Fact]
    public void Cast_TextToCharWithLength_PadsWithSpacesLikeMsSql()
    {
        Assert.Equal("a  ", _database.Scalar("SELECT CAST('a' AS CHAR(3))"));
    }

    [Fact]
    public void Cast_DatetimeToVarchar_YieldsTheIsoTextDocumentedInTheDialectHints()
    {
        Assert.Equal(
            "2024-03-05 22:20:30.123",
            _database.Scalar($"SELECT CAST({Sample} AS VARCHAR(30))")
        );
    }

    [Fact]
    public void Cast_OverTableColumns_ConvertsEveryRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE C (Raw VARCHAR(10)); INSERT INTO C VALUES ('1'), ('22'), ('333');"
        );

        ResultSet result = _database.Query("SELECT CAST(Raw AS INT) + 1 FROM C ORDER BY rowid");

        Assert.Equal([2L, 23L, 334L], result.Column());
    }

    [Fact]
    public void Iif_PicksTheBranchByCondition()
    {
        Assert.Equal("yes", _database.Scalar("SELECT IIF(1 = 1, 'yes', 'no')"));
        Assert.Equal("no", _database.Scalar("SELECT IIF(1 = 2, 'yes', 'no')"));
    }

    [Fact]
    public void Iif_WithNullCondition_PicksTheFalseBranch()
    {
        Assert.Equal("no", _database.Scalar("SELECT IIF(NULL = 1, 'yes', 'no')"));
    }

    [Fact]
    public void Iif_Nested_EvaluatesInnerExpression()
    {
        Assert.Equal(
            "mid",
            _database.Scalar("SELECT IIF(5 > 10, 'high', IIF(5 > 3, 'mid', 'low'))")
        );
    }
}
