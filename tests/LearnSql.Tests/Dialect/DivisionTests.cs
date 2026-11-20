using LearnSql.Sql.Dialect;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Dialect;

public class DivisionTests : IDisposable
{
    private const string DivideByZeroMessage = "Divide by zero error encountered.";

    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public void Dispose()
    {
        _database.Dispose();
    }

    public static TheoryData<string, object?> ArithmeticCases() =>
        new TheoryData<string, object?>
        {
            { "SELECT 7 / 2", 3L },
            { "SELECT -7 / 2", -3L },
            { "SELECT 7 / -2", -3L },
            { "SELECT 7 / 2.0", 3.5 },
            { "SELECT 7.0 / 2", 3.5 },
            { "SELECT 1 / 3.0", 1.0 / 3.0 },
            { "SELECT 100 / 4 / 5", 5L },
            { "SELECT 100 / (2 + 3)", 20L },
            { "SELECT 2 * 6 / 4", 3L },
            { "SELECT 10 / '2'", 5L },
            { "SELECT 10 / ' 4 '", 2L },
            { "SELECT 0 / 5", 0L },
            { "SELECT 10 / NULL", null },
            { "SELECT NULL / 5", null },
            { "SELECT 5 / NULLIF(0, 0)", null },
            { "SELECT 5 / NULLIF(2, 0)", 2L },
            { "SELECT 7 % 3", 1L },
            { "SELECT -7 % 3", -1L },
            { "SELECT 7 % -3", 1L },
            { "SELECT 7.5 % 2", 1.5 },
            { "SELECT -7.5 % 2", -1.5 },
            { "SELECT 8 % 2.5", 0.5 },
            { "SELECT '9' % '4'", 1L },
            { "SELECT 7 % NULL", null },
            { "SELECT NULL % 7", null },
            { "SELECT (-9223372036854775807 - 1) % -1", 0L },
            { "SELECT 10 % 3 + 10 / 3", 4L },
            { "SELECT CASE WHEN 0 = 0 THEN 0 ELSE 1 / 0 END", 0L },
            { "SELECT CASE WHEN 1 = 2 THEN 1 % 0 ELSE 5 END", 5L },
        };

    public static TheoryData<string> ZeroDivisions() =>
        new TheoryData<string>
        {
            "SELECT 1 / 0",
            "SELECT 1 / 0.0",
            "SELECT 1.5 / 0",
            "SELECT 1.5 / 0.0",
            "SELECT 0 / 0",
            "SELECT NULL / 0",
            "SELECT 1 / (2 - 2)",
            "SELECT 1 / '0'",
            "SELECT 1 / ' 0.0 '",
            "SELECT 1 % 0",
            "SELECT 1.5 % 0",
            "SELECT 1 % 0.0",
            "SELECT 1 % (3 - 3)",
            "SELECT CAST(1 AS FLOAT) / 0",
            "SELECT CAST(1 AS DECIMAL(5,2)) / CAST(0 AS DECIMAL(5,2))",
            "SELECT 10 / (5 - 5) + 1",
            "SELECT 1 / 0 WHERE 1 = 1",
        };

    [Theory]
    [MemberData(nameof(ArithmeticCases))]
    public void DivisionAndModulo_ComputeLikeSqlServer(string sql, object? expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [MemberData(nameof(ZeroDivisions))]
    public void DivisionOrModuloByZero_FailsWithError8134(string sql)
    {
        SqlErrorAssert.Raised(_database.Run(sql), 8134, 16, 1, DivideByZeroMessage);
    }

    [Fact]
    public void DivisionByZero_ErrorTextHasTheSqlServerHeader()
    {
        ScriptOutcome outcome = _database.Run("SELECT 1 / 0");

        Assert.NotNull(outcome.Error);
        Assert.Equal(
            "Msg 8134, Level 16, State 1\nDivide by zero error encountered.",
            outcome.Error.Message
        );
    }

    [Fact]
    public void DivisionByZero_OverColumn_FailsOnTheZeroRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE D (A INT, B INT); INSERT INTO D VALUES (10, 2), (10, 0);"
        );

        SqlErrorAssert.Raised(
            _database.Run("SELECT A / B FROM D"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void DivisionOverColumn_WithoutZero_ComputesEveryRow()
    {
        _database.ExecuteRaw(
            "CREATE TABLE D (A INT, B INT); INSERT INTO D VALUES (10, 2), (9, 4), (7, NULL);"
        );

        ResultSet result = _database.Query("SELECT A / B, A % B FROM D ORDER BY rowid");

        Assert.Equal([5L, 2L, null], result.Column(0));
        Assert.Equal([0L, 1L, null], result.Column(1));
    }

    [Fact]
    public void DivisionOverColumn_GuardedByNullIf_YieldsNull()
    {
        _database.ExecuteRaw(
            "CREATE TABLE D (A INT, B INT); INSERT INTO D VALUES (10, 2), (10, 0);"
        );

        ResultSet result = _database.Query("SELECT A / NULLIF(B, 0) FROM D ORDER BY rowid");

        Assert.Equal([5L, null], result.Column());
    }

    [Fact]
    public void DivisionByZero_InWhere_FailsWhenTheRowIsEvaluated()
    {
        _database.ExecuteRaw("CREATE TABLE D (A INT); INSERT INTO D VALUES (1), (0);");

        SqlErrorAssert.Raised(
            _database.Run("SELECT A FROM D WHERE 10 / A > 1"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void DivisionByZero_InAggregate_Fails()
    {
        _database.ExecuteRaw("CREATE TABLE D (A INT); INSERT INTO D VALUES (1), (0);");

        SqlErrorAssert.Raised(
            _database.Run("SELECT SUM(10 / A) FROM D"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void DivisionByZero_InUpdate_FailsAndLeavesTheTableUnchanged()
    {
        _database.ExecuteRaw(
            "CREATE TABLE D (A INT, B INT); INSERT INTO D VALUES (10, 2), (10, 0);"
        );

        ScriptOutcome outcome = _database.Run("UPDATE D SET A = A / B");

        SqlErrorAssert.Raised(outcome, 8134, 16, 1, DivideByZeroMessage);
        Assert.Equal([10L, 10L], _database.Query("SELECT A FROM D ORDER BY rowid").Column());
    }

    [Fact]
    public void DivisionByZero_FromVariable_Fails()
    {
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @z INT = 0; SELECT 1 / @z"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void DivisionByZero_InDeclareInitializer_Fails()
    {
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @x INT = 1 / 0"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void CompoundDivisionAndModulo_OnVariables_UseTheSameRules()
    {
        Assert.Equal(2L, _database.Scalar("DECLARE @i INT = 10; SET @i /= 4; SELECT @i"));
        Assert.Equal(2L, _database.Scalar("DECLARE @i INT = 10; SET @i %= 4; SELECT @i"));
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @i INT = 10; SET @i /= 0"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
        SqlErrorAssert.Raised(
            _database.Run("DECLARE @i INT = 10; SET @i %= 0"),
            8134,
            16,
            1,
            DivideByZeroMessage
        );
    }

    [Fact]
    public void ModuloWithNonNumericText_FailsWithError245()
    {
        SqlErrorAssert.Raised(
            _database.Run("SELECT 7 % 'abc'"),
            245,
            16,
            1,
            "Conversion failed when converting the varchar value 'abc' to data type int."
        );
    }

    [Fact]
    public void DivisionByNonZeroLiteral_IsTranslatedWithoutRuntimeGuard()
    {
        foreach (string divisor in new[] { "100", "2.5", "-4", "(8)", "1e3", "$5" })
        {
            DialectTranslationResult result = _database.Translate($"SELECT 10 / {divisor}");

            Assert.True(result.Success);
            Assert.DoesNotContain("TSQL_DIVISOR", result.Batches[0].Statements[0].Sql);
        }
    }

    [Theory]
    [InlineData("SELECT 10 / 0")]
    [InlineData("SELECT 10 / 0.0")]
    [InlineData("SELECT 10 / (1 - 1)")]
    [InlineData("SELECT 10 / x FROM (SELECT 1 AS x) AS t")]
    public void DivisionByZeroLiteralOrExpression_IsTranslatedWithRuntimeGuard(string sql)
    {
        DialectTranslationResult result = _database.Translate(sql);

        Assert.True(result.Success);
        Assert.Contains("TSQL_DIVISOR", result.Batches[0].Statements[0].Sql);
    }

    [Fact]
    public void Modulo_IsAlwaysTranslatedToTheTypedFunction()
    {
        DialectTranslationResult result = _database.Translate("SELECT 7 % 2");

        Assert.Contains("TSQL_MOD(7, 2)", result.Batches[0].Statements[0].Sql);
    }

    [Fact]
    public void DivisionTranslation_IsIdempotentInsideNestedExpressions()
    {
        DialectTranslationResult result = _database.Translate("SELECT (1 + 2) / (3 - 3) * 4 % 5");

        Assert.True(result.Success);
        Assert.Equal(
            "SELECT TSQL_MOD((1 + 2) / TSQL_DIVISOR((3 - 3)) * 4, 5)",
            result.Batches[0].Statements[0].Sql
        );
    }

    [Fact]
    public void DivisionByZero_AfterAnEarlierStatement_KeepsItsResult()
    {
        ScriptOutcome outcome = _database.Run("SELECT 1;\nSELECT 2 / 0");

        Assert.NotNull(outcome.Error);
        Assert.Equal(8134, outcome.Error.Error.Number);
        Assert.Equal([1L], outcome.ResultSets.Single().Column());
    }
}
