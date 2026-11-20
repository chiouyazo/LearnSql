using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class RowsAffectedTests
{
    private const string Setup = "CREATE TABLE T (a INT, b INT);";

    private static async Task<SandboxDatabase> CreateWithTableAsync()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            Setup + "INSERT INTO T VALUES (1, 1), (2, 1), (3, 2), (4, 2), (5, 2);"
        );
        return database;
    }

    [Fact]
    public async Task Insert_ReportsTheNumberOfInsertedRows()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(Setup);

        ExecutionResult result = await database.ExecuteOkAsync(
            "INSERT INTO T VALUES (1, 1), (2, 2), (3, 3)"
        );

        StatementResult statement = Assert.Single(result.Statements);
        Assert.Equal(StatementKind.Insert, statement.Kind);
        Assert.Equal(3, statement.RowsAffected);
        Assert.Empty(statement.ResultSets);
        Assert.Equal(["(3 rows affected)"], statement.Messages);
    }

    [Fact]
    public async Task Update_ReportsTheNumberOfChangedRows()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "UPDATE T SET a = a + 10 WHERE b = 2"
        );

        Assert.Equal(3, Assert.Single(result.Statements).RowsAffected);
    }

    [Fact]
    public async Task Delete_ReportsTheNumberOfDeletedRows()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync("DELETE FROM T WHERE a <= 2");

        Assert.Equal(2, Assert.Single(result.Statements).RowsAffected);
        Assert.Equal(3L, await database.CountRowsAsync("T"));
    }

    [Theory]
    [InlineData("UPDATE T SET a = 0 WHERE a > 100")]
    [InlineData("DELETE FROM T WHERE a > 100")]
    public async Task DataModificationWithoutMatches_ReportsZeroNotNull(string script)
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(script);

        StatementResult statement = Assert.Single(result.Statements);
        Assert.Equal(0, statement.RowsAffected);
        Assert.Equal(["(0 rows affected)"], statement.Messages);
    }

    [Fact]
    public async Task InsertSelectWithoutRows_ReportsZero()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "INSERT INTO T SELECT a, b FROM T WHERE a > 100"
        );

        Assert.Equal(0, Assert.Single(result.Statements).RowsAffected);
    }

    [Fact]
    public async Task Insert_ReportsOnlyDirectlyChangedRowsNotCascades()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE Parent (Id INT PRIMARY KEY);"
                + "CREATE TABLE Child (Id INT PRIMARY KEY, ParentId INT, "
                + "CONSTRAINT FK_Child_Parent FOREIGN KEY (ParentId) REFERENCES Parent (Id) ON DELETE CASCADE);"
                + "INSERT INTO Parent VALUES (1), (2);"
                + "INSERT INTO Child VALUES (1, 1), (2, 1), (3, 1), (4, 2);"
        );

        ExecutionResult result = await database.ExecuteOkAsync("DELETE FROM Parent WHERE Id = 1");

        Assert.Equal(1, Assert.Single(result.Statements).RowsAffected);
        Assert.Equal(1L, await database.CountRowsAsync("Child"));
    }

    [Theory]
    [InlineData("CREATE TABLE U (a INT)", StatementKind.CreateTable)]
    [InlineData("CREATE INDEX IX_T_a ON T (a)", StatementKind.CreateIndex)]
    [InlineData("DROP TABLE T", StatementKind.DropTable)]
    [InlineData("CREATE VIEW V AS SELECT a FROM T", StatementKind.CreateView)]
    public async Task Ddl_ReportsNullRowsAffectedAndTheCompletionMessage(
        string script,
        StatementKind kind
    )
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(script);

        StatementResult statement = Assert.Single(result.Statements);
        Assert.Equal(kind, statement.Kind);
        Assert.Null(statement.RowsAffected);
        Assert.Empty(statement.Messages);
        Assert.Equal([ExecutionMessages.CommandsCompleted], result.Messages);
    }

    [Fact]
    public async Task Ddl_AfterDataModificationInTheSameScript_DoesNotReportAStaleCount()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "DELETE FROM T WHERE a <= 3;\nCREATE TABLE U (a INT);\nDROP TABLE U;"
        );

        Assert.Equal(3, result.Statements.Count);
        Assert.Equal(3, result.Statements[0].RowsAffected);
        Assert.Null(result.Statements[1].RowsAffected);
        Assert.Null(result.Statements[2].RowsAffected);
    }

    [Fact]
    public async Task Ddl_InAFollowingCall_DoesNotReportTheCountOfThePreviousCall()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("DELETE FROM T");

        ExecutionResult result = await database.ExecuteOkAsync("CREATE TABLE U (a INT)");

        Assert.Null(Assert.Single(result.Statements).RowsAffected);
    }

    [Fact]
    public async Task Select_ReportsNullRowsAffectedAndTheRowCountMessage()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("DELETE FROM T WHERE a = 1");

        ExecutionResult result = await database.ExecuteOkAsync("SELECT a FROM T");

        StatementResult statement = Assert.Single(result.Statements);
        Assert.Null(statement.RowsAffected);
        Assert.Equal(["(4 rows affected)"], statement.Messages);
    }

    [Fact]
    public async Task SelectInto_ReportsTheNumberOfCopiedRows()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT a, b INTO Copy FROM T WHERE b = 2"
        );

        StatementResult statement = Assert.Single(result.Statements);
        Assert.Equal(StatementKind.SelectInto, statement.Kind);
        Assert.Equal(3, statement.RowsAffected);
        Assert.Empty(statement.ResultSets);
        Assert.Equal(["(3 rows affected)"], statement.Messages);
        Assert.Equal(3L, await database.CountRowsAsync("Copy"));
    }

    [Fact]
    public async Task SelectInto_WithoutRows_ReportsZero()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteOkAsync(
            "SELECT a INTO Empty FROM T WHERE a > 100"
        );

        Assert.Equal(0, Assert.Single(result.Statements).RowsAffected);
    }

    [Fact]
    public async Task Messages_UseTheSingularForExactlyOneRow()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(Setup);

        ExecutionResult insert = await database.ExecuteOkAsync("INSERT INTO T VALUES (1, 1)");
        ExecutionResult select = await database.ExecuteOkAsync("SELECT a FROM T");

        Assert.Equal(["(1 row affected)"], insert.Messages);
        Assert.Equal(["(1 row affected)"], select.Messages);
    }

    [Theory]
    [InlineData(0, "(0 rows affected)")]
    [InlineData(1, "(1 row affected)")]
    [InlineData(2, "(2 rows affected)")]
    [InlineData(1000, "(1000 rows affected)")]
    public void RowsAffectedMessage_PicksSingularOnlyForOne(int count, string expected)
    {
        Assert.Equal(expected, ExecutionMessages.RowsAffected(count));
    }

    [Fact]
    public void Constants_MatchTheSqlServerWording()
    {
        Assert.Equal("Commands completed successfully.", ExecutionMessages.CommandsCompleted);
        Assert.Equal("(5 rows shown, more rows exist)", ExecutionMessages.RowsShown(5));
    }

    [Fact]
    public async Task Messages_OfAMixedScript_AreCollectedInStatementOrder()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            Setup
                + "INSERT INTO T VALUES (1, 1), (2, 2); UPDATE T SET b = 9; SELECT a FROM T; DELETE FROM T;"
        );

        Assert.Equal(
            ["(2 rows affected)", "(2 rows affected)", "(2 rows affected)", "(2 rows affected)"],
            result.Messages
        );
        Assert.Equal(
            [null, 2, 2, null, 2],
            result.Statements.Select((StatementResult s) => s.RowsAffected).ToArray()
        );
    }

    [Fact]
    public async Task RepeatedBatch_ReportsEachIterationOnItsOwn()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(Setup);

        ExecutionResult result = await database.ExecuteOkAsync("INSERT INTO T VALUES (1, 1)\nGO 3");

        Assert.Equal(3, result.Statements.Count);
        Assert.Equal(
            [0, 1, 2],
            result.Statements.Select((StatementResult s) => s.Iteration).ToArray()
        );
        Assert.All(result.Statements, (StatementResult s) => Assert.Equal(1, s.RowsAffected));
        Assert.Equal(3L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task Statements_CarryBatchIndexKindAndOrigin()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteOkAsync(
            "CREATE TABLE T (a INT);\nGO\nINSERT INTO T VALUES (1);\nSELECT a FROM T"
        );

        Assert.Equal(3, result.Statements.Count);
        Assert.Equal(
            [0, 1, 1],
            result.Statements.Select((StatementResult s) => s.BatchIndex).ToArray()
        );
        Assert.Equal(
            [StatementKind.CreateTable, StatementKind.Insert, StatementKind.Select],
            result.Statements.Select((StatementResult s) => s.Kind).ToArray()
        );
        Assert.Equal(
            [1, 3, 4],
            result.Statements.Select((StatementResult s) => s.Origin.Start.Line).ToArray()
        );
        Assert.All(
            result.Statements,
            (StatementResult s) => Assert.True(s.Duration >= TimeSpan.Zero)
        );
    }

    [Fact]
    public async Task Messages_AfterAFailure_ContainNoCompletionMessage()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE T (a INT); SELECT * FROM Missing"
        );

        Assert.False(result.Succeeded);
        Assert.DoesNotContain(ExecutionMessages.CommandsCompleted, result.Messages);
    }
}
