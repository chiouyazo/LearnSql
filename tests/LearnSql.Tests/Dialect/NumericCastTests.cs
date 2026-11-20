using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class NumericCastTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> IntegerSuccessCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CAST('12' AS INT)", 12L },
            { "SELECT CAST(' 12 ' AS INT)", 12L },
            { "SELECT CAST('+5' AS INT)", 5L },
            { "SELECT CAST('-5' AS INT)", -5L },
            { "SELECT CAST('007' AS INT)", 7L },
            { "SELECT CAST('' AS INT)", 0L },
            { "SELECT CAST('   ' AS INT)", 0L },
            { "SELECT CAST(3.7 AS INT)", 3L },
            { "SELECT CAST(-3.7 AS INT)", -3L },
            { "SELECT CAST(0.99 AS INT)", 0L },
            { "SELECT CAST(NULL AS INT)", null },
            { "SELECT CAST(NULL AS BIGINT)", null },
            { "SELECT CAST(NULL AS SMALLINT)", null },
            { "SELECT CAST(NULL AS TINYINT)", null },
            { "SELECT CAST(2147483647 AS INT)", 2147483647L },
            { "SELECT CAST(-2147483648 AS INT)", -2147483648L },
            { "SELECT CAST('2147483647' AS INT)", 2147483647L },
            { "SELECT CAST('-2147483648' AS INT)", -2147483648L },
            { "SELECT CAST(0 AS TINYINT)", 0L },
            { "SELECT CAST('255' AS TINYINT)", 255L },
            { "SELECT CAST(32767 AS SMALLINT)", 32767L },
            { "SELECT CAST('-32768' AS SMALLINT)", -32768L },
            { "SELECT CAST('9223372036854775807' AS BIGINT)", long.MaxValue },
            { "SELECT CAST(9223372036854775807 AS BIGINT)", long.MaxValue },
            { "SELECT CAST('-9223372036854775808' AS BIGINT)", long.MinValue },
            { "SELECT CONVERT(INT, ' 42')", 42L },
            { "SELECT CAST(CAST('12' AS VARCHAR(5)) AS INT) + 1", 13L },
        };

    public static TheoryData<string, string> IntegerConversionFailures() =>
        new TheoryData<string, string>
        {
            { "SELECT CAST('abc' AS INT)", "abc" },
            { "SELECT CAST('1.5' AS INT)", "1.5" },
            { "SELECT CAST('1e3' AS INT)", "1e3" },
            { "SELECT CAST('12abc' AS INT)", "12abc" },
            { "SELECT CAST('1,000' AS INT)", "1,000" },
            { "SELECT CAST('12 3' AS INT)", "12 3" },
            { "SELECT CAST('--5' AS INT)", "--5" },
            { "SELECT CAST('2024-01-01' AS INT)", "2024-01-01" },
            { "SELECT CONVERT(INT, 'abc')", "abc" },
        };

    public static TheoryData<string, object?> DecimalCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CAST('1.5' AS DECIMAL(5,2))", 1.5 },
            { "SELECT CAST(1.005 AS DECIMAL(5,2))", 1.01 },
            { "SELECT CAST(-1.005 AS DECIMAL(5,2))", -1.01 },
            { "SELECT CAST(1.2345 AS DECIMAL(5,2))", 1.23 },
            { "SELECT CAST(2.5 AS DECIMAL(5,0))", 3.0 },
            { "SELECT CAST(-2.5 AS DECIMAL(5,0))", -3.0 },
            { "SELECT CAST(2.4 AS DECIMAL(5,0))", 2.0 },
            { "SELECT CAST('  12.345 ' AS DECIMAL(6,1))", 12.3 },
            { "SELECT CAST('.5' AS DECIMAL(5,2))", 0.5 },
            { "SELECT CAST('5.' AS DECIMAL(5,2))", 5.0 },
            { "SELECT CAST(99999 AS DECIMAL(5,0))", 99999.0 },
            { "SELECT CAST(123.456 AS DECIMAL(5,2))", 123.46 },
            { "SELECT CAST(1234.5 AS DECIMAL)", 1235.0 },
            { "SELECT CAST(7 AS NUMERIC(3,1))", 7.0 },
            { "SELECT CAST(NULL AS DECIMAL(5,2))", null },
            { "SELECT CAST(NULL AS NUMERIC(5,2))", null },
        };

    public static TheoryData<string, object?> FloatAndBitCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT CAST('1.5' AS FLOAT)", 1.5 },
            { "SELECT CAST('1e3' AS FLOAT)", 1000.0 },
            { "SELECT CAST('1E-2' AS FLOAT)", 0.01 },
            { "SELECT CAST(' .5 ' AS FLOAT)", 0.5 },
            { "SELECT CAST('5.' AS FLOAT)", 5.0 },
            { "SELECT CAST('-2.5' AS REAL)", -2.5 },
            { "SELECT CAST('' AS FLOAT)", 0.0 },
            { "SELECT CAST(5 AS FLOAT)", 5.0 },
            { "SELECT CAST(NULL AS FLOAT)", null },
            { "SELECT CAST(NULL AS REAL)", null },
            { "SELECT CAST('2' AS BIT)", 1L },
            { "SELECT CAST('0' AS BIT)", 0L },
            { "SELECT CAST(' TRUE ' AS BIT)", 1L },
            { "SELECT CAST('False' AS BIT)", 0L },
            { "SELECT CAST('' AS BIT)", 0L },
            { "SELECT CAST(0.5 AS BIT)", 1L },
            { "SELECT CAST(-1 AS BIT)", 1L },
            { "SELECT CAST(NULL AS BIT)", null },
        };

    [Theory]
    [MemberData(nameof(IntegerSuccessCases))]
    public void CastToInteger_OfConvertibleValue_ConvertsLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(IntegerConversionFailures))]
    public void CastToInt_OfNonIntegerText_FailsWithError245(string sql, string value)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            245,
            16,
            1,
            $"Conversion failed when converting the varchar value '{value}' to data type int."
        );
    }

    [Theory]
    [InlineData("SELECT CAST('abc' AS BIGINT)", "bigint")]
    [InlineData("SELECT CAST('abc' AS SMALLINT)", "smallint")]
    [InlineData("SELECT CAST('abc' AS TINYINT)", "tinyint")]
    public void CastToOtherIntegerTypes_OfText_NamesTheTargetType(string sql, string target)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            245,
            16,
            1,
            $"Conversion failed when converting the varchar value 'abc' to data type {target}."
        );
    }

    [Theory]
    [InlineData("SELECT CAST(2147483648 AS INT)", "int")]
    [InlineData("SELECT CAST(-2147483649 AS INT)", "int")]
    [InlineData("SELECT CAST(3000000000.5 AS INT)", "int")]
    [InlineData("SELECT CAST(256 AS TINYINT)", "tinyint")]
    [InlineData("SELECT CAST(-1 AS TINYINT)", "tinyint")]
    [InlineData("SELECT CAST(32768 AS SMALLINT)", "smallint")]
    [InlineData("SELECT CAST(-32769 AS SMALLINT)", "smallint")]
    [InlineData("SELECT CAST(1e30 AS BIGINT)", "bigint")]
    [InlineData("SELECT CAST(1e30 AS INT)", "int")]
    public void CastToInteger_OfNumberOutOfRange_FailsWithError8115(string sql, string target)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            8115,
            16,
            2,
            $"Arithmetic overflow error converting expression to data type {target}."
        );
    }

    [Theory]
    [InlineData("SELECT CAST('2147483648' AS INT)", "2147483648", "an int")]
    [InlineData("SELECT CAST('-2147483649' AS INT)", "-2147483649", "an int")]
    [InlineData("SELECT CAST('300' AS TINYINT)", "300", "a tinyint")]
    [InlineData("SELECT CAST('-1' AS TINYINT)", "-1", "a tinyint")]
    [InlineData("SELECT CAST('40000' AS SMALLINT)", "40000", "a smallint")]
    [InlineData("SELECT CAST('99999999999999999999' AS INT)", "99999999999999999999", "an int")]
    public void CastToInteger_OfTextOutOfRange_FailsWithError248(
        string sql,
        string value,
        string column
    )
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            248,
            16,
            1,
            $"The conversion of the varchar value '{value}' overflowed {column} column."
        );
    }

    [Theory]
    [InlineData("SELECT CAST('9223372036854775808' AS BIGINT)")]
    [InlineData("SELECT CAST('-9223372036854775809' AS BIGINT)")]
    [InlineData("SELECT CAST('99999999999999999999999' AS BIGINT)")]
    public void CastToBigInt_OfTextOutOfRange_FailsWithError8114(string sql)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            8114,
            16,
            5,
            "Error converting data type varchar to bigint."
        );
    }

    [Theory]
    [MemberData(nameof(DecimalCases))]
    public void CastToDecimal_ConvertsAndRoundsLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData("SELECT CAST('abc' AS DECIMAL(5,2))")]
    [InlineData("SELECT CAST('1e3' AS DECIMAL(10,2))")]
    [InlineData("SELECT CAST('1.2.3' AS DECIMAL(10,2))")]
    [InlineData("SELECT CAST('' AS DECIMAL(5,2))")]
    [InlineData("SELECT CAST('1,5' AS NUMERIC(5,2))")]
    public void CastToDecimal_OfNonNumericText_FailsWithError8114(string sql)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            8114,
            16,
            5,
            "Error converting data type varchar to numeric."
        );
    }

    [Theory]
    [InlineData("SELECT CAST(100000 AS DECIMAL(5,0))")]
    [InlineData("SELECT CAST(-100000 AS DECIMAL(5,0))")]
    [InlineData("SELECT CAST(1234.5 AS DECIMAL(5,2))")]
    [InlineData("SELECT CAST(999.995 AS DECIMAL(5,2))")]
    [InlineData("SELECT CAST('1000' AS DECIMAL(5,2))")]
    [InlineData("SELECT CAST(12345678 AS NUMERIC(7,0))")]
    [InlineData("SELECT CAST(1e30 AS DECIMAL(10,2))")]
    public void CastToDecimal_OfValueTooLargeForThePrecision_FailsWithError8115(string sql)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            8115,
            16,
            8,
            "Arithmetic overflow error converting numeric to data type numeric."
        );
    }

    [Theory]
    [MemberData(nameof(FloatAndBitCases))]
    public void CastToFloatAndBit_ConvertsLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData("SELECT CAST('abc' AS FLOAT)", "float")]
    [InlineData("SELECT CAST('1e400' AS FLOAT)", "float")]
    [InlineData("SELECT CAST('NaN' AS FLOAT)", "float")]
    [InlineData("SELECT CAST('Infinity' AS FLOAT)", "float")]
    [InlineData("SELECT CAST('1,5' AS FLOAT)", "float")]
    [InlineData("SELECT CAST('abc' AS REAL)", "real")]
    public void CastToFloat_OfNonNumericText_FailsWithError8114(string sql, string target)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            8114,
            16,
            5,
            $"Error converting data type varchar to {target}."
        );
    }

    [Theory]
    [InlineData("SELECT CAST('abc' AS BIT)", "abc")]
    [InlineData("SELECT CAST('yes' AS BIT)", "yes")]
    public void CastToBit_OfNonNumericText_FailsWithError245(string sql, string value)
    {
        SqlErrorAssert.Raised(
            _database.Run(sql),
            245,
            16,
            1,
            $"Conversion failed when converting the varchar value '{value}' to data type bit."
        );
    }

    [Fact]
    public void Cast_ErrorMessage_KeepsQuotesAndNewlinesOfTheValue()
    {
        SqlErrorAssert.Raised(
            _database.Run("SELECT CAST('it''s' AS INT)"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'it's' to data type int."
        );
        SqlErrorAssert.Raised(
            _database.Run("SELECT CAST('a' + CHAR(10) + 'b' AS INT)"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'a\nb' to data type int."
        );
    }

    [Fact]
    public void Cast_ErrorMessage_IsFreeOfInternalMarkers()
    {
        ScriptOutcome outcome = _database.Run("SELECT CAST('abc' AS INT)");

        Assert.NotNull(outcome.Error);
        Assert.DoesNotContain("[mssql:", outcome.Error.Error.Message);
        Assert.DoesNotContain("[mssql:", outcome.Error.Message);
    }

    [Fact]
    public void Cast_InsideLargerExpression_StillFails()
    {
        SqlErrorAssert.Raised(
            _database.Run("SELECT 1 + CAST('x' AS INT)"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'x' to data type int."
        );
    }

    [Fact]
    public void Cast_OverTableColumn_FailsOnTheFirstBadRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE C (Raw VARCHAR(10)); INSERT INTO C VALUES ('1'), ('x2'), ('3');"
        );

        SqlErrorAssert.Raised(
            _database.Run("SELECT CAST(Raw AS INT) FROM C"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'x2' to data type int."
        );
    }

    [Fact]
    public void Cast_OverTableColumnWithNulls_PassesNullsThrough()
    {
        _database.ExecuteRaw(
            "CREATE TABLE C (Raw VARCHAR(10)); INSERT INTO C VALUES ('1'), (NULL), ('3');"
        );

        ResultSet result = _database.Query("SELECT CAST(Raw AS INT) FROM C ORDER BY rowid");

        Assert.Equal([1L, null, 3L], result.Column());
    }

    [Fact]
    public void Cast_IsNotEvaluatedForFilteredRows()
    {
        _database.ExecuteRaw(
            "CREATE TABLE C (Raw VARCHAR(10)); INSERT INTO C VALUES ('1'), ('x');"
        );

        ResultSet result = _database.Query(
            "SELECT CAST(Raw AS INT) FROM C WHERE Raw NOT LIKE 'x%'"
        );

        Assert.Equal([1L], result.Column());
    }

    [Fact]
    public void Declare_WithNonConvertibleInitialValue_FailsWithError245()
    {
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @i INT = 'abc'"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'abc' to data type int."
        );
    }

    [Fact]
    public void Set_OfNonConvertibleValue_FailsWithError245()
    {
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @i INT = 1; SET @i = 'x'"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'x' to data type int."
        );
    }

    [Fact]
    public void Set_ToTinyIntOutOfRange_FailsWithError8115()
    {
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @t TINYINT = 250; SET @t += 10"),
            8115,
            16,
            2,
            "Arithmetic overflow error converting expression to data type tinyint."
        );
    }

    [Fact]
    public void Failure_RollsBackNothingBeforeIt_AndKeepsTheResultsOfEarlierStatements()
    {
        ScriptOutcome outcome = _database.Run("SELECT 1; SELECT CAST('abc' AS INT)");

        Assert.NotNull(outcome.Error);
        Assert.Equal([1L], outcome.ResultSets.Single().Column());
    }
}
