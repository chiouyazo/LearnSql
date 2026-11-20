using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class CancellationTests
{
    private static readonly TimeSpan UpperBound = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task ExecuteAsync_CancelledWhileRunning_ReturnsCancelled()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMilliseconds(150));

        ExecutionResult result = await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            null,
            source.Token
        );

        Assert.NotNull(result.Error);
        Assert.Equal(ExecutionErrorKind.Cancelled, result.Error.Kind);
        Assert.True(result.Duration < UpperBound, result.Duration.ToString());
        Assert.Empty(result.Statements);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledWhileRunning_LeavesTheDatabaseUsable()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT); INSERT INTO T VALUES (1)");
        using CancellationTokenSource source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMilliseconds(100));

        await database.ExecuteAsync(SandboxFactory.EndlessRecursiveQuery, null, source.Token);

        Assert.Equal(1L, await database.CountRowsAsync("T"));
        Assert.False(database.Status.IsBusy);
    }

    [Fact]
    public async Task ExecuteAsync_TokenAlreadyCancelled_ReturnsCancelledWithoutRunningAnything()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (1)",
            null,
            source.Token
        );

        Assert.Equal(ExecutionErrorKind.Cancelled, result.Error?.Kind);
        Assert.Empty(result.Statements);
        Assert.Equal(0L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_TokenCancelledBeforeInitialization_ReturnsCancelled()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        using CancellationTokenSource source = new CancellationTokenSource();
        await source.CancelAsync();

        ExecutionResult result = await database.ExecuteAsync("SELECT 1", null, source.Token);

        Assert.Equal(ExecutionErrorKind.Cancelled, result.Error?.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledWhileWaitingInTheQueue_ReturnsCancelledAndLeavesTheRunningCallAlone()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.InitializeAsync();
        ExecutionOptions running = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(1) };
        Task<ExecutionResult> first = database.ExecuteOnWorkerAsync(
            SandboxFactory.EndlessRecursiveQuery,
            running
        );
        await database.WaitUntilBusyAsync();
        using CancellationTokenSource source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMilliseconds(100));

        ExecutionResult queued = await database.ExecuteAsync(
            "SELECT 'never runs'",
            null,
            source.Token
        );

        Assert.False(first.IsCompleted);
        Assert.Equal(ExecutionErrorKind.Cancelled, queued.Error?.Kind);
        Assert.Empty(queued.Statements);
        ExecutionResult firstResult = await first;
        Assert.Equal(ExecutionErrorKind.Timeout, firstResult.Error?.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledInTheQueue_DoesNotBlockLaterCalls()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.InitializeAsync();
        ExecutionOptions running = new ExecutionOptions
        {
            Timeout = TimeSpan.FromMilliseconds(500),
        };
        Task<ExecutionResult> first = database.ExecuteOnWorkerAsync(
            SandboxFactory.EndlessRecursiveQuery,
            running
        );
        await database.WaitUntilBusyAsync();
        using CancellationTokenSource source = new CancellationTokenSource();
        Task<ExecutionResult> cancelled = database.ExecuteAsync("SELECT 1", null, source.Token);
        Task<ExecutionResult> later = database.ExecuteAsync("SELECT 2");
        await source.CancelAsync();

        ExecutionResult cancelledResult = await cancelled;
        ExecutionResult laterResult = await later;

        Assert.Equal(ExecutionErrorKind.Cancelled, cancelledResult.Error?.Kind);
        Assert.True(laterResult.Succeeded, SandboxFactory.Describe(laterResult));
        Assert.Equal(2L, laterResult.ResultSets[0].Rows[0][0]);
        Assert.Equal(ExecutionErrorKind.Timeout, (await first).Error?.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledInRollbackMode_RestoresTheDatabase()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");
        using CancellationTokenSource source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromMilliseconds(200));

        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (1);\n" + SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Rollback(),
            source.Token
        );

        Assert.Equal(ExecutionErrorKind.Cancelled, result.Error?.Kind);
        Assert.True(result.RolledBack);
        Assert.Equal(0L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ExecuteAsync_NeverCancelledToken_DoesNotAffectTheResult()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        ExecutionResult result = await database.ExecuteAsync(
            "SELECT 5",
            null,
            CancellationToken.None
        );

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
    }
}
