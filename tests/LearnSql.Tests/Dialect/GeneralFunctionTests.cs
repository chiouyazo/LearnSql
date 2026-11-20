using System.Text.RegularExpressions;
using LearnSql.Sql.Dialect.Functions;
using LearnSql.Tests.Support;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Dialect;

public class GeneralFunctionTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new SqliteTestDatabase();

    public GeneralFunctionTests()
    {
        _database.ExecuteRaw(
            "CREATE TABLE W (Txt VARCHAR(5) NULL, Num INT NULL);"
                + "INSERT INTO W VALUES (NULL, NULL), ('a', 1);"
        );
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    [Fact]
    public void IsNull_OnNullLiteralWithNumber_ReturnsTheNumberEndToEnd()
    {
        ScriptOutcome outcome = _database.Run("SELECT ISNULL(NULL, 1)");

        Assert.True(outcome.Succeeded, outcome.Describe());
        Assert.Equal(1L, outcome.LastResultSet.Single());
    }

    [Theory]
    [InlineData("SELECT ISNULL('a', 'b')", "a")]
    [InlineData("SELECT ISNULL(NULL, 'b')", "b")]
    [InlineData("SELECT ISNULL('', 'b')", "")]
    public void IsNull_OnText_ReturnsTheFirstNonNull(string sql, string expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Theory]
    [InlineData("SELECT ISNULL(3, NULL)", 3L)]
    [InlineData("SELECT ISNULL(NULL, 5)", 5L)]
    [InlineData("SELECT ISNULL(0, 5)", 0L)]
    public void IsNull_OnNumbers_ReturnsTheFirstNonNull(string sql, long expected)
    {
        Assert.Equal(expected, _database.Scalar(sql));
    }

    [Fact]
    public void IsNull_OnTwoNulls_ReturnsNull()
    {
        Assert.Null(_database.Scalar("SELECT ISNULL(NULL, NULL)"));
    }

    [Fact]
    public void IsNull_TextColumnWithNumericReplacement_ReturnsTheReplacementAsText()
    {
        object? value = _database.Scalar("SELECT ISNULL(Txt, 5) FROM W WHERE Num IS NULL");

        Assert.Equal("5", value);
    }

    [Fact]
    public void IsNull_NumericColumnWithTextReplacement_ReturnsTheReplacementAsNumber()
    {
        object? value = _database.Scalar("SELECT ISNULL(Num, '7') FROM W WHERE Txt IS NULL");

        Assert.Equal(7L, value);
    }

    [Fact]
    public void IsNull_NumericColumnWithTextReplacement_KeepsAPresentValue()
    {
        object? value = _database.Scalar("SELECT ISNULL(Num, '7') FROM W WHERE Txt = 'a'");

        Assert.Equal(1L, value);
    }

    [Fact]
    public void IsNull_TextColumnWithTextReplacement_IsNotCoerced()
    {
        object? value = _database.Scalar("SELECT ISNULL(Txt, 'z') FROM W WHERE Num IS NULL");

        Assert.Equal("z", value);
    }

    [Fact]
    public void IsNull_ResultInConcatenation_IsTreatedAsTextWhenTheFirstArgumentIsText()
    {
        object? value = _database.Scalar("SELECT ISNULL(Txt, 'z') + '!' FROM W WHERE Num IS NULL");

        Assert.Equal("z!", value);
    }

    [Fact]
    public void IsNull_InWhereClause_FiltersOnTheReplacement()
    {
        Assert.Equal(1L, _database.Scalar("SELECT COUNT(*) FROM W WHERE ISNULL(Num, 0) = 0"));
    }

    [Fact]
    public void IsNull_WithOnlyOneArgument_IsReportedBySqlite()
    {
        ScriptOutcome outcome = _database.Run("SELECT ISNULL(1)");

        Assert.False(outcome.Succeeded);
    }

    [Theory]
    [InlineData(null, 1L, 1L)]
    [InlineData(5L, 1L, 5L)]
    public void IsNullFunction_Static_ReturnsTheFirstNonNullValue(
        object? value,
        object? replacement,
        object? expected
    )
    {
        Assert.Equal(expected, GeneralFunctions.IsNull(value, replacement));
    }

    [Fact]
    public void IsNullFunction_Static_TreatsDbNullAsNull()
    {
        Assert.Equal("x", GeneralFunctions.IsNull(DBNull.Value, "x"));
    }

    [Fact]
    public void NewId_ReturnsAnUppercaseGuid()
    {
        string value = (string)_database.Scalar("SELECT NEWID()")!;

        Assert.Matches(new Regex("^[0-9A-F]{8}(-[0-9A-F]{4}){3}-[0-9A-F]{12}$"), value);
    }

    [Fact]
    public void NewId_IsEvaluatedPerRow()
    {
        ResultSet result = _database.Query("SELECT NEWID() FROM W");

        Assert.Equal(2, result.Column().Distinct().Count());
    }

    [Fact]
    public void Registrar_RegistersEveryFunctionWithItsArgumentCount()
    {
        SqliteFunctionRegistrar registrar = new SqliteFunctionRegistrar(
            new SingleDeterministicTimeProvider()
        );

        Assert.All(
            registrar.Functions,
            (SqliteScalarFunction function) =>
            {
                Assert.False(string.IsNullOrWhiteSpace(function.Name));
                Assert.InRange(function.ArgumentCount, 0, 4);
            }
        );
        Assert.Equal(
            registrar.Functions.Count,
            registrar
                .Functions.Select((SqliteScalarFunction f) => (f.Name, f.ArgumentCount))
                .Distinct()
                .Count()
        );
    }

    [Fact]
    public void Registrar_MarksClockAndRandomFunctionsAsNonDeterministic()
    {
        SqliteFunctionRegistrar registrar = new SqliteFunctionRegistrar(
            new SingleDeterministicTimeProvider()
        );

        string[] nonDeterministic =
        [
            .. registrar
                .Functions.Where((SqliteScalarFunction f) => !f.IsDeterministic)
                .Select((SqliteScalarFunction f) => f.Name)
                .Distinct()
                .Order(),
        ];

        Assert.Equal(
            ["GETDATE", "GETUTCDATE", "NEWID", "SYSDATETIME", "SYSUTCDATETIME"],
            nonDeterministic
        );
    }

    [Fact]
    public void ScalarFunction_WithMoreThanFourArguments_CannotBeRegistered()
    {
        SqliteScalarFunction function = new SqliteScalarFunction(
            "TOO_MANY",
            5,
            (object?[] arguments) => arguments.Length
        );

        Assert.Throws<NotSupportedException>(() => function.RegisterOn(_database.Connection));
    }

    [Fact]
    public void ScalarFunction_RegisteredWithZeroToFourArguments_IsCallableFromSql()
    {
        for (int count = 0; count <= 4; count++)
        {
            string name = $"ARITY_{count}";
            new SqliteScalarFunction(
                name,
                count,
                (object?[] arguments) => (long)arguments.Length
            ).RegisterOn(_database.Connection);
            string arguments = string.Join(", ", Enumerable.Repeat("1", count));

            using SqliteCommand command = _database.Connection.CreateCommand();
            command.CommandText = $"SELECT {name}({arguments})";

            Assert.Equal((long)count, command.ExecuteScalar());
        }
    }
}
