using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class TransactionModeTests
{
    private static async Task<SandboxDatabase> CreateWithTableAsync()
    {
        SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT); INSERT INTO T VALUES (1);");
        return database;
    }

    [Fact]
    public async Task RollbackMode_Insert_IsUndoneAndFlagged()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (2), (3)",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(result.RolledBack);
        Assert.Equal(2, Assert.Single(result.Statements).RowsAffected);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_UpdateAndDelete_AreUndone()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        await database.ExecuteOkAsync(
            "UPDATE T SET a = 100; DELETE FROM T",
            SandboxFactory.Rollback()
        );

        Assert.Equal(1L, await database.ScalarAsync("SELECT a FROM T"));
    }

    [Fact]
    public async Task RollbackMode_Ddl_IsUndoneAndTheSchemaIsRefreshed()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "CREATE TABLE Temp (x INT); DROP TABLE T;\nGO\nCREATE VIEW V AS SELECT 1 AS one",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(result.RolledBack);
        DatabaseSchema schema = await database.GetSchemaAsync();
        Assert.NotNull(schema.Find("T"));
        Assert.Null(schema.Find("Temp"));
        Assert.Null(schema.Find("V"));
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_DdlThenQueryInANormalRun_SeesTheRestoredTable()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("DROP TABLE T", SandboxFactory.Rollback());

        ExecutionResult result = await database.ExecuteOkAsync("INSERT INTO T VALUES (5)");

        Assert.Equal(1, Assert.Single(result.Statements).RowsAffected);
    }

    [Fact]
    public async Task RollbackMode_SelectOnly_StillReportsRolledBack()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "SELECT a FROM T",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded);
        Assert.True(result.RolledBack);
        Assert.Equal(1L, result.ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task RollbackMode_FailingStatement_UndoesEarlierStatementsOfTheScript()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (2); SELECT * FROM Missing",
            SandboxFactory.Rollback()
        );

        Assert.Equal(ExecutionErrorKind.Sql, result.Error?.Kind);
        Assert.True(result.RolledBack);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_DialectError_DoesNotStartAnything()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "SELECT FROM",
            SandboxFactory.Rollback()
        );

        Assert.Equal(ExecutionErrorKind.Diagnostics, result.Error?.Kind);
        Assert.False(result.RolledBack);
    }

    [Fact]
    public async Task RollbackMode_UserBeginAndCommit_LeaveNothingBehind()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "BEGIN TRANSACTION; INSERT INTO T VALUES (2); COMMIT;",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(result.RolledBack);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
        Assert.False(database.Status.InTransaction);
    }

    [Fact]
    public async Task RollbackMode_StatementsAfterTheUsersCommit_AreUndoneToo()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        await database.ExecuteOkAsync(
            "BEGIN TRANSACTION; INSERT INTO T VALUES (2); COMMIT; INSERT INTO T VALUES (3); DELETE FROM T WHERE a = 1;",
            SandboxFactory.Rollback()
        );

        Assert.Equal(1L, await database.CountRowsAsync("T"));
        Assert.Equal(1L, await database.ScalarAsync("SELECT a FROM T"));
        Assert.False(database.Status.InTransaction);
    }

    [Fact]
    public async Task RollbackMode_UserRollback_RestoresAndKeepsRunningStatementsUndoable()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "BEGIN TRANSACTION; INSERT INTO T VALUES (2); ROLLBACK; INSERT INTO T VALUES (3);",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(result.RolledBack);
        Assert.Equal(4, result.Statements.Count);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
        Assert.False(database.Status.InTransaction);
    }

    [Fact]
    public async Task RollbackMode_UserRollbackWithoutBegin_IsHandledLikeAnyOtherRollback()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (2); ROLLBACK; INSERT INTO T VALUES (3);",
            SandboxFactory.Rollback()
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_RepeatedRuns_AlwaysStartFromTheSameState()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        for (int run = 0; run < 5; run++)
        {
            ExecutionResult result = await database.ExecuteAsync(
                "BEGIN TRANSACTION; INSERT INTO T VALUES (9); COMMIT; SELECT COUNT(*) FROM T",
                SandboxFactory.Rollback()
            );

            Assert.Equal(2L, result.ResultSets[0].Rows[0][0]);
        }

        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_WithAnOpenUserTransactionBefore_RestoresTheStateInsideIt()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("BEGIN TRANSACTION; INSERT INTO T VALUES (2);");

        await database.ExecuteOkAsync("INSERT INTO T VALUES (3)", SandboxFactory.Rollback());

        Assert.True(database.Status.InTransaction);
        Assert.Equal(2L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task RollbackMode_DoesNotLeakTheSavepointIntoLaterTransactions()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("INSERT INTO T VALUES (2)", SandboxFactory.Rollback());

        await database.ExecuteOkAsync("BEGIN TRANSACTION; INSERT INTO T VALUES (4); COMMIT;");

        Assert.False(database.Status.InTransaction);
        Assert.Equal(2L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task NormalMode_Commit_PersistsAndEndsTheTransaction()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        ExecutionResult begin = await database.ExecuteOkAsync(
            "BEGIN TRANSACTION; INSERT INTO T VALUES (2);"
        );
        Assert.True(database.Status.InTransaction);
        ExecutionResult commit = await database.ExecuteOkAsync("COMMIT");

        Assert.False(begin.RolledBack);
        Assert.False(commit.RolledBack);
        Assert.Equal(StatementKind.CommitTransaction, Assert.Single(commit.Statements).Kind);
        Assert.False(database.Status.InTransaction);
        Assert.Equal(2L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task NormalMode_Rollback_DiscardsTheOpenTransaction()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync(
            "BEGIN TRANSACTION; INSERT INTO T VALUES (2); DELETE FROM T WHERE a = 1;"
        );
        Assert.True(database.Status.InTransaction);
        Assert.Equal(1L, await database.CountRowsAsync("T"));

        ExecutionResult rollback = await database.ExecuteOkAsync("ROLLBACK");

        Assert.False(database.Status.InTransaction);
        Assert.Equal(StatementKind.RollbackTransaction, Assert.Single(rollback.Statements).Kind);
        Assert.Equal(1L, await database.ScalarAsync("SELECT a FROM T"));
    }

    [Fact]
    public async Task NormalMode_RollbackOfDdl_RefreshesTheSchema()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        await database.ExecuteOkAsync("BEGIN TRANSACTION; CREATE TABLE Temp (x INT);");
        Assert.NotNull((await database.GetSchemaAsync()).Find("Temp"));

        await database.ExecuteOkAsync("ROLLBACK");

        Assert.Null((await database.GetSchemaAsync()).Find("Temp"));
    }

    [Fact]
    public async Task NormalMode_StatementsOutsideATransaction_AreCommittedImmediately()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        await database.ExecuteOkAsync("INSERT INTO T VALUES (2)");

        Assert.False(database.Status.InTransaction);
        Assert.Equal(2L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task NormalMode_TransactionSpanningSeveralCalls_StaysOpenBetweenThem()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();

        await database.ExecuteOkAsync("BEGIN TRANSACTION");
        await database.ExecuteOkAsync("INSERT INTO T VALUES (2)");
        await database.ExecuteOkAsync("INSERT INTO T VALUES (3)");

        Assert.True(database.Status.InTransaction);
        await database.ExecuteOkAsync("COMMIT");
        Assert.False(database.Status.InTransaction);
        Assert.Equal(3L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task StatusChanged_ReportsTheTransactionState()
    {
        await using SandboxDatabase database = await CreateWithTableAsync();
        List<bool> observed = [];
        database.StatusChanged += (object? _, SandboxStatus status) =>
        {
            if (!status.IsBusy)
            {
                observed.Add(status.InTransaction);
            }
        };

        await database.ExecuteOkAsync("BEGIN TRANSACTION");
        await database.ExecuteOkAsync("COMMIT");

        Assert.Equal([true, false], observed);
    }

    [Fact]
    public async Task ResetAsync_DropsAnOpenUserTransaction()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );
        await database.ExecuteOkAsync("BEGIN TRANSACTION; DELETE FROM OrderItem;");
        Assert.True(database.Status.InTransaction);

        ProvisionResult reset = await database.ResetAsync();

        Assert.True(reset.Succeeded);
        Assert.False(database.Status.InTransaction);
        Assert.True(await database.CountRowsAsync("OrderItem") > 0);
    }
}
