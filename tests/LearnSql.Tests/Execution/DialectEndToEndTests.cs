using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Execution;

public class DialectEndToEndTests
{
    private static async Task<object?[]> RowAsync(SandboxDatabase database, string tsql)
    {
        ExecutionResult result = await database.ExecuteOkAsync(tsql);
        return Assert.Single(Assert.Single(result.ResultSets).Rows);
    }

    private static async Task<SandboxDatabase> CreateWithNumbersAsync()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE N (i INT); INSERT INTO N VALUES (1), (2), (3), (4), (5);"
        );
        return database;
    }

    [Fact]
    public async Task GetDate_ReturnsTheCurrentTimeOfTheTimeProvider()
    {
        await using SandboxDatabase database = SandboxFactory.Create(
            timeProvider: new SingleDeterministicTimeProvider()
        );

        object?[] row = await RowAsync(database, "SELECT GETDATE()");

        Assert.Equal("2024-03-15 10:20:30.123", row[0]);
    }

    [Fact]
    public async Task GetDate_CanBeComparedAndCastToADate()
    {
        await using SandboxDatabase database = SandboxFactory.Create(
            timeProvider: new SingleDeterministicTimeProvider()
        );

        object?[] row = await RowAsync(
            database,
            "SELECT CAST(GETDATE() AS DATE) AS d, CASE WHEN GETDATE() > '2024-01-01' THEN 1 ELSE 0 END AS later"
        );

        Assert.Equal("2024-03-15", row[0]);
        Assert.Equal(1L, row[1]);
    }

    [Fact]
    public async Task IsNull_ReplacesNullValuesOnly()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (a INT NULL); INSERT INTO T VALUES (NULL), (7);"
        );

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT ISNULL(a, -1) AS v FROM T ORDER BY v"
        );

        Assert.Equal(
            [-1L, 7L],
            result.ResultSets[0].Rows.Select((object?[] row) => (long)row[0]!).ToArray()
        );
    }

    [Fact]
    public async Task Len_IgnoresTrailingSpacesLikeSqlServer()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        object?[] row = await RowAsync(
            database,
            "SELECT LEN('abc') AS a, LEN('abc   ') AS b, LEN('') AS c"
        );

        Assert.Equal([3L, 3L, 0L], row);
    }

    [Fact]
    public async Task DateAdd_AddsDaysAndMonthsToADate()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        object?[] row = await RowAsync(
            database,
            "SELECT DATEADD(day, 1, '2020-01-31') AS a, DATEADD(month, 1, '2020-01-31') AS b, DATEADD(year, -1, '2020-02-29') AS c"
        );

        Assert.Equal(["2020-02-01", "2020-02-29", "2019-02-28"], row);
    }

    [Fact]
    public async Task DateDiff_CountsBoundariesCrossed()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        object?[] row = await RowAsync(
            database,
            "SELECT DATEDIFF(day, '2020-01-01', '2020-01-11') AS d, DATEDIFF(month, '2020-01-31', '2020-03-01') AS m, "
                + "DATEDIFF(year, '2019-12-31', '2020-01-01') AS y"
        );

        Assert.Equal([10L, 2L, 1L], row);
    }

    [Fact]
    public async Task Top_LimitsTheRowsOfTheOrderedResult()
    {
        await using SandboxDatabase database = await CreateWithNumbersAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT TOP 2 i FROM N ORDER BY i DESC"
        );

        Assert.Equal(
            [5L, 4L],
            result.ResultSets[0].Rows.Select((object?[] row) => (long)row[0]!).ToArray()
        );
        Assert.Equal(["(2 rows affected)"], result.Messages);
    }

    [Fact]
    public async Task Top_WithParenthesesAndExpression_Works()
    {
        await using SandboxDatabase database = await CreateWithNumbersAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT TOP (1 + 2) i FROM N ORDER BY i"
        );

        Assert.Equal(3, result.ResultSets[0].Rows.Count);
    }

    [Fact]
    public async Task BracketedIdentifiers_AreUnquoted()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE [Order Lines] ([line no] INT, [select] INT); INSERT INTO [Order Lines] VALUES (1, 10), (2, 20);"
        );

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT [line no], [select] FROM [Order Lines] WHERE [select] > 10"
        );

        SqlResultSet resultSet = Assert.Single(result.ResultSets);
        Assert.Equal(
            ["line no", "select"],
            resultSet.Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal([2L, 20L], Assert.Single(resultSet.Rows));
    }

    [Fact]
    public async Task PlusOperator_ConcatenatesTextAndAddsNumbers()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        object?[] row = await RowAsync(
            database,
            "SELECT 'Learn' + 'Sql' AS text, 1 + 2 AS number, 'a' + ' ' + 'b' AS spaced"
        );

        Assert.Equal(["LearnSql", 3L, "a b"], row);
    }

    [Fact]
    public async Task PlusOperator_ConcatenatesTextColumns()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT TOP 1 FirstName + ' ' + LastName AS FullName FROM Customer ORDER BY Id"
        );

        string name = Assert.IsType<string>(result.ResultSets[0].Rows[0][0]);
        Assert.Contains(' ', name);
    }

    [Fact]
    public async Task OffsetFetch_PagesThroughTheOrderedResult()
    {
        await using SandboxDatabase database = await CreateWithNumbersAsync();

        ExecutionResult page = await database.ExecuteOkAsync(
            "SELECT i FROM N ORDER BY i OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY"
        );
        ExecutionResult tail = await database.ExecuteOkAsync(
            "SELECT i FROM N ORDER BY i OFFSET 4 ROWS"
        );

        Assert.Equal(
            [3L, 4L],
            page.ResultSets[0].Rows.Select((object?[] row) => (long)row[0]!).ToArray()
        );
        Assert.Equal(
            [5L],
            tail.ResultSets[0].Rows.Select((object?[] row) => (long)row[0]!).ToArray()
        );
    }

    [Fact]
    public async Task Declare_VariablesLiveAcrossStatementsOfTheScript()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            "DECLARE @a INT, @b NVARCHAR(10);\nSET @a = 4;\nSET @b = 'x';\nSELECT @a * 2 AS doubled, @b AS text;"
        );

        Assert.Equal(
            ["doubled", "text"],
            result.ResultSets[0].Columns.Select((ResultColumn c) => c.Name).ToArray()
        );
        Assert.Equal([8L, "x"], Assert.Single(result.ResultSets[0].Rows));
    }

    [Fact]
    public async Task Declare_VariablesCanFilterTables()
    {
        await using SandboxDatabase database = await CreateWithNumbersAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "DECLARE @min INT;\nSET @min = 3;\nSELECT COUNT(*) FROM N WHERE i >= @min;"
        );

        Assert.Equal(3L, result.ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task Declare_UndeclaredVariable_FailsBeforeAnythingRuns()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE T (a INT);\nSELECT @missing"
        );

        result.ExpectError(ExecutionErrorKind.Diagnostics);
        Assert.Null((await database.GetSchemaAsync()).Find("T"));
    }

    [Fact]
    public async Task GoWithCount_RepeatsTheBatch()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");

        ExecutionResult result = await database.ExecuteOkAsync(
            "INSERT INTO T VALUES (1)\nGO 4\nSELECT COUNT(*) FROM T"
        );

        Assert.Equal(5, result.Statements.Count);
        Assert.Equal(4L, result.ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task GoSeparatesBatches_SoCreateViewMayStartTheSecondOne()
    {
        await using SandboxDatabase database = await CreateWithNumbersAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "CREATE VIEW Big AS SELECT i FROM N WHERE i > 3\nGO\nSELECT COUNT(*) FROM Big"
        );

        Assert.Equal(2L, result.ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task CombinedConstructs_WorkTogetherInOneScript()
    {
        await using SandboxDatabase database = SandboxFactory.Create(
            timeProvider: new SingleDeterministicTimeProvider()
        );

        ExecutionResult result = await database.ExecuteOkAsync(
            "CREATE TABLE [Log Entry] ([Id] INT PRIMARY KEY, [When] DATETIME NOT NULL, Note NVARCHAR(20) NULL);\n"
                + "DECLARE @id INT;\nSET @id = 1;\n"
                + "INSERT INTO [Log Entry] VALUES (@id, GETDATE(), NULL);\n"
                + "SELECT TOP 1 [Id], ISNULL(Note, 'none') + '!' AS Note, LEN(Note) AS NoteLength, "
                + "DATEADD(day, 1, CAST([When] AS DATE)) AS NextDay, DATEDIFF(day, '2024-03-01', [When]) AS Age "
                + "FROM [Log Entry] ORDER BY [Id];"
        );

        object?[] row = Assert.Single(result.ResultSets[0].Rows);
        Assert.Equal(1L, row[0]);
        Assert.Equal("none!", row[1]);
        Assert.Null(row[2]);
        Assert.Equal("2024-03-16", row[3]);
        Assert.Equal(14L, row[4]);
    }
}
