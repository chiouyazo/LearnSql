using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class TimeoutTests
{
    private const string EndlessUpdate =
        "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r) UPDATE T SET a = (SELECT COUNT(*) FROM r)";

    private static readonly TimeSpan UpperBound = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task ExecuteAsync_EndlessRecursiveCte_ReturnsTimeoutAroundTheLimit()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Timeout, result.Error.Kind);
        Assert.Contains("Zeitlimit", result.Error.Message);
        Assert.True(result.Duration >= TimeSpan.FromMilliseconds(250), result.Duration.ToString());
        Assert.True(result.Duration < UpperBound, result.Duration.ToString());
        Assert.Empty(result.Statements);
        Assert.NotNull(result.Error.Origin);
    }

    [Fact]
    public async Task ExecuteAsync_Timeout_LeavesTheDatabaseUsableWithItsData()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT); INSERT INTO T VALUES (1), (2)");

        ExecutionResult timedOut = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        Assert.Equal(ExecutionErrorKind.Timeout, timedOut.Error?.Kind);
        Assert.Equal(2L, await database.CountRowsAsync("T"));
        Assert.False(database.Status.IsBusy);
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutTwiceInARow_StaysUsable()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult first = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );
        ExecutionResult second = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        Assert.Equal(ExecutionErrorKind.Timeout, first.Error?.Kind);
        Assert.Equal(ExecutionErrorKind.Timeout, second.Error?.Kind);
        Assert.Equal(3L, await database.ScalarAsync("SELECT 1 + 2"));
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutAfterEarlierStatements_KeepsTheirEffectsInNormalMode()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (1);\n" + SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.Equal(2, error.Origin?.Start.Line);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutInRollbackMode_UndoesEarlierStatements()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (1);\n" + SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed(rollback: true)
        );

        Assert.Equal(ExecutionErrorKind.Timeout, result.Error?.Kind);
        Assert.True(result.RolledBack);
        Assert.Equal(0L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutWithoutOpenTransaction_HasNoRollbackNote()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT); INSERT INTO T VALUES (1)");

        ExecutionResult result = await database.ExecuteAsync(
            EndlessUpdate,
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.DoesNotContain("zurückgerollt", error.Message);
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutOfAWriteInsideAUserTransaction_AppendsTheRollbackNote()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (a INT); INSERT INTO T VALUES (1); BEGIN TRANSACTION; INSERT INTO T VALUES (2);"
        );
        Assert.True(database.Status.InTransaction);

        ExecutionResult result = await database.ExecuteAsync(
            EndlessUpdate,
            SandboxFactory.Timeboxed()
        );

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Timeout, result.Error.Kind);
        Assert.Contains("Zeitlimit", result.Error.Message);
        Assert.EndsWith(
            "Die offene Transaktion wurde dabei von SQLite zurückgerollt.",
            result.Error.Message
        );
        Assert.False(database.Status.InTransaction);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutOfAReadInsideAUserTransaction_KeepsTheTransactionAndAddsNoNote()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE T (a INT); BEGIN TRANSACTION; INSERT INTO T VALUES (2);"
        );

        ExecutionResult result = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Timeout);
        Assert.DoesNotContain("zurückgerollt", error.Message);
        Assert.True(database.Status.InTransaction);
        Assert.Equal(1L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_ScriptFinishingBeforeTheLimit_IsNotAffected()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(30) };

        ExecutionResult result = await database.ExecuteAsync("SELECT 1", options);

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(result.Duration < TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task ExecuteAsync_TimeoutIsMeasuredPerCall()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(2) };

        for (int run = 0; run < 3; run++)
        {
            ExecutionResult result = await database.ExecuteAsync("SELECT " + run, options);

            Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        }
    }

    [Fact]
    public async Task ExecuteAsync_LongRunningWorkThatFinishesInTime_Succeeds()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        ExecutionOptions options = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(20) };

        ExecutionResult result = await database.ExecuteAsync(
            "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r WHERE n < 100000) SELECT COUNT(*) FROM r",
            options
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.Equal(100000L, result.ResultSets[0].Rows[0][0]);
    }
}
