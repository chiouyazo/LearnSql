using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public class ConcurrencyTests
{
    private const string SlowCount =
        "WITH r AS (SELECT 1 AS n UNION ALL SELECT n + 1 FROM r WHERE n < 150000) SELECT COUNT(*) AS c FROM r";

    [Fact]
    public async Task ExecuteAsync_TwoOverlappingCalls_RunOneAfterTheOtherInCallOrder()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.InitializeAsync();

        Task<ExecutionResult> first = database.ExecuteAsync(
            "CREATE TABLE T (a INT);\n" + SlowCount + ";\nINSERT INTO T VALUES (1);"
        );
        Task<ExecutionResult> second = database.ExecuteAsync(
            "INSERT INTO T VALUES (2);\nSELECT COUNT(*) AS c FROM T;"
        );
        ExecutionResult[] results = await Task.WhenAll(first, second);

        Assert.True(results[0].Succeeded, SandboxFactory.Describe(results[0]));
        Assert.True(results[1].Succeeded, SandboxFactory.Describe(results[1]));
        Assert.Equal(150000L, results[0].ResultSets[0].Rows[0][0]);
        Assert.Equal(2L, results[1].ResultSets[0].Rows[0][0]);
    }

    [Fact]
    public async Task ExecuteAsync_ManyParallelCalls_NeverLoseAnUpdate()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync(
            "CREATE TABLE Counter (n INT); INSERT INTO Counter VALUES (0)"
        );

        Task<ExecutionResult>[] calls =
        [
            .. Enumerable
                .Range(0, 16)
                .Select((int _) => database.ExecuteOnWorkerAsync("UPDATE Counter SET n = n + 1")),
        ];
        ExecutionResult[] results = await Task.WhenAll(calls);

        Assert.All(
            results,
            (ExecutionResult r) => Assert.True(r.Succeeded, SandboxFactory.Describe(r))
        );
        Assert.Equal(16L, await database.ScalarAsync("SELECT n FROM Counter"));
    }

    [Fact]
    public async Task ExecuteAsync_ParallelReadsAndWrites_EachSeeAConsistentState()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.ExecuteOkAsync("CREATE TABLE T (a INT, b INT); INSERT INTO T VALUES (0, 0)");

        Task<ExecutionResult>[] writers =
        [
            .. Enumerable
                .Range(0, 8)
                .Select(
                    (int _) =>
                        database.ExecuteOnWorkerAsync(
                            "UPDATE T SET a = a + 1; UPDATE T SET b = b + 1;"
                        )
                ),
        ];
        Task<ExecutionResult>[] readers =
        [
            .. Enumerable
                .Range(0, 8)
                .Select((int _) => database.ExecuteOnWorkerAsync("SELECT a - b FROM T")),
        ];
        ExecutionResult[] results = await Task.WhenAll(writers.Concat(readers));

        Assert.All(
            results,
            (ExecutionResult r) => Assert.True(r.Succeeded, SandboxFactory.Describe(r))
        );
        foreach (Task<ExecutionResult> reader in readers)
        {
            ExecutionResult result = await reader;
            Assert.Equal(0L, result.ResultSets[0].Rows[0][0]);
        }
    }

    [Fact]
    public async Task StatusChanged_NeverShowsTwoBusyPhasesInARow()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        List<SandboxStatus> events = [];
        database.StatusChanged += (object? _, SandboxStatus status) =>
        {
            lock (events)
            {
                events.Add(status);
            }
        };

        Task<ExecutionResult>[] calls =
        [
            .. Enumerable
                .Range(0, 6)
                .Select((int i) => database.ExecuteOnWorkerAsync($"SELECT {i}")),
        ];
        await Task.WhenAll(calls);

        SandboxStatus[] snapshot;
        lock (events)
        {
            snapshot = [.. events];
        }

        Assert.True(snapshot.Length >= 2);
        Assert.True(snapshot[0].IsBusy);
        Assert.False(snapshot[^1].IsBusy);
        for (int index = 1; index < snapshot.Length; index++)
        {
            Assert.NotEqual(snapshot[index - 1].IsBusy, snapshot[index].IsBusy);
        }
    }

    [Fact]
    public async Task Status_WhileAStatementRuns_IsBusyWithTheExecutingActivity()
    {
        await using SandboxDatabase database = SandboxFactory.Create();
        await database.InitializeAsync();
        ExecutionOptions options = new ExecutionOptions
        {
            Timeout = TimeSpan.FromMilliseconds(800),
        };

        Task<ExecutionResult> running = database.ExecuteOnWorkerAsync(
            SandboxFactory.EndlessRecursiveQuery,
            options
        );
        await database.WaitUntilBusyAsync();
        SandboxActivity activity = database.Status.Activity;
        ExecutionResult result = await running;

        Assert.Equal(SandboxActivity.Executing, activity);
        Assert.Equal(ExecutionErrorKind.Timeout, result.Error?.Kind);
        Assert.Equal(SandboxActivity.Idle, database.Status.Activity);
    }

    [Fact]
    public async Task ExecuteAndProvision_Overlapping_AreSerialized()
    {
        await using SandboxDatabase database = await SandboxFactory.CreateProvisionedAsync(
            "starter"
        );

        Task<ExecutionResult> delete = database.ExecuteAsync("DELETE FROM OrderItem");
        Task<ProvisionResult> reset = database.ResetAsync();
        Task<ExecutionResult> count = database.ExecuteAsync("SELECT COUNT(*) FROM OrderItem");
        await Task.WhenAll(delete, reset, count);

        Assert.True((await delete).Succeeded);
        Assert.True((await reset).Succeeded);
        Assert.True(Convert.ToInt64((await count).ResultSets[0].Rows[0][0]) > 0);
    }

    [Fact]
    public async Task GetSchemaAsync_WhileExecuting_WaitsForTheRunToFinish()
    {
        await using SandboxDatabase database = SandboxFactory.Create();

        Task<ExecutionResult> create = database.ExecuteAsync("CREATE TABLE Late (a INT)");
        DatabaseSchema schema = await database.GetSchemaAsync();
        await create;

        Assert.NotNull(schema.Find("Late"));
    }
}
