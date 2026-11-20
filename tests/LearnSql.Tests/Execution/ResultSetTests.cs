using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Execution;

public class ResultSetTests
{
    private const string SeriesQuery =
        "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r WHERE n < {0}) SELECT n FROM r ORDER BY n";

    private static string Series(int count)
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, SeriesQuery, count);
    }

    [Fact]
    public async Task ExecuteAsync_SeveralSelects_ReturnsEveryResultSetInOrder()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT 1 AS first;\nSELECT 'a' AS second, 'b' AS third;\nSELECT 3 AS fourth WHERE 1 = 0"
        );

        Assert.Equal(3, result.ResultSets.Count);
        Assert.Equal(
            ["first"],
            result.ResultSets[0].Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal(
            ["second", "third"],
            result.ResultSets[1].Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal(
            ["fourth"],
            result.ResultSets[2].Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal(1L, Assert.Single(result.ResultSets[0].Rows)[0]);
        Assert.Equal(["a", "b"], result.ResultSets[1].Rows[0]);
        Assert.Empty(result.ResultSets[2].Rows);
    }

    [Fact]
    public async Task ExecuteAsync_SelectsAcrossBatches_ReturnsEveryResultSet()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT 1\nGO\nSELECT 2\nGO 2\nSELECT 3"
        );

        Assert.Equal(
            [1L, 2L, 2L, 3L],
            result.ResultSets.Select((SqlResultSet r) => (long)r.Rows[0][0]!).ToArray()
        );
    }

    [Fact]
    public async Task ExecuteAsync_EmptyResult_StillDescribesItsColumns()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (Id INT, Name NVARCHAR(20))");

        ExecutionResult result = await database.ExecuteOkAsync("SELECT Id, Name FROM T");

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.Empty(resultSet.Rows);
        Assert.False(resultSet.Truncated);
        Assert.Equal(
            ["Id", "Name"],
            resultSet.Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal(["(0 rows affected)"], result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ResultColumns_CarryNameDeclaredTypeAndKind()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (Id INT, Name NVARCHAR(20)); INSERT INTO T VALUES (7, 'x')"
        );

        ExecutionResult result = await database.ExecuteOkAsync("SELECT Id, Name FROM T");

        IReadOnlyList<ResultColumn> columns = Assert.Single(result.ResultSets).Columns;
        Assert.Equal("Id", columns[0].Name);
        Assert.Equal("INT", columns[0].TypeName);
        Assert.Equal(ValueKind.Numeric, columns[0].Kind);
        Assert.Equal("Name", columns[1].Name);
        Assert.Equal(ValueKind.Text, columns[1].Kind);
    }

    [Fact]
    public async Task ExecuteAsync_NullValues_AreReturnedAsNull()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync("SELECT NULL AS n, 1 AS one");

        object?[] row = Assert.Single(Assert.Single(result.ResultSets).Rows);
        Assert.Null(row[0]);
        Assert.Equal(1L, row[1]);
    }

    [Fact]
    public async Task ExecuteAsync_ResultExceedingTheLimit_ReturnsExactlyMaxRowsAndMarksTruncation()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 5 };

        ExecutionResult result = await database.ExecuteOkAsync(Series(12), options);

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.True(resultSet.Truncated);
        Assert.Equal(5, resultSet.Rows.Count);
        Assert.Equal(
            [1L, 2L, 3L, 4L, 5L],
            resultSet.Rows.Select((object?[] row) => (long)row[0]!).ToArray()
        );
        Assert.True(result.Truncated);
        Assert.True(Assert.Single(result.Statements).Truncated);
        Assert.Equal(["(5 rows shown, more rows exist)"], result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ResultWithExactlyMaxRows_IsNotTruncated()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 5 };

        ExecutionResult result = await database.ExecuteOkAsync(Series(5), options);

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.False(resultSet.Truncated);
        Assert.Equal(5, resultSet.Rows.Count);
        Assert.False(result.Truncated);
        Assert.Equal(["(5 rows affected)"], result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_ResultOneRowOverTheLimit_IsTruncated()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 5 };

        ExecutionResult result = await database.ExecuteOkAsync(Series(6), options);

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.True(resultSet.Truncated);
        Assert.Equal(5, resultSet.Rows.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ResultBelowTheLimit_IsNotTruncated()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 5 };

        ExecutionResult result = await database.ExecuteOkAsync(Series(3), options);

        Assert.False(Assert.Single(result.ResultSets).Truncated);
        Assert.Equal(3, Assert.Single(result.ResultSets).Rows.Count);
    }

    [Fact]
    public async Task ExecuteAsync_LimitOfOne_KeepsOnlyTheFirstRow()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 1 };

        ExecutionResult result = await database.ExecuteOkAsync(Series(3), options);

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.True(resultSet.Truncated);
        Assert.Equal(1L, Assert.Single(resultSet.Rows)[0]);
        Assert.Equal(["(1 rows shown, more rows exist)"], result.Messages);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultLimit_CapsAtOneThousandRows()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(Series(1500));

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.Equal(ExecutionOptions.DefaultMaxRowsPerResultSet, resultSet.Rows.Count);
        Assert.Equal(1000, resultSet.Rows.Count);
        Assert.True(resultSet.Truncated);
    }

    [Fact]
    public async Task ExecuteAsync_LimitApplies_ToEveryResultSetIndependently()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 3 };

        ExecutionResult result = await database.ExecuteOkAsync(
            Series(10) + ";\n" + Series(2) + ";\n" + Series(3),
            options
        );

        Assert.Equal(
            [true, false, false],
            result.ResultSets.Select((SqlResultSet r) => r.Truncated).ToArray()
        );
        Assert.Equal(
            [3, 2, 3],
            result.ResultSets.Select((SqlResultSet r) => r.Rows.Count).ToArray()
        );
        Assert.True(result.Truncated);
    }

    [Fact]
    public async Task ExecuteAsync_TruncationDoesNotAffectLaterStatements()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { MaxRowsPerResultSet = 2 };

        ExecutionResult result = await database.ExecuteOkAsync(
            Series(10) + ";\nSELECT 99 AS last",
            options
        );

        Assert.Equal(99L, result.ResultSets[1].Rows[0][0]);
        Assert.False(result.Statements[1].Truncated);
    }

    [Fact]
    public async Task ExecuteAsync_LargeTextValues_AreReturnedInFull()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT REPLICATE('ab', 5000) AS big"
        );

        string value = Assert.IsType<string>(
            Assert.Single(Assert.Single(result.ResultSets).Rows)[0]
        );
        Assert.Equal(10000, value.Length);
    }
}
