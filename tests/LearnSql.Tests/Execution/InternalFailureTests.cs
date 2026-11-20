using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using Microsoft.Extensions.Logging;

namespace LearnSql.Tests.Execution;

public class InternalFailureTests
{
    [Fact]
    public async Task ThrowingPolicy_YieldsAnInternalErrorInsteadOfAnException()
    {
        ThrowingStatementPolicy policy = new ThrowingStatementPolicy();
        await using SandboxDatabase database = SandboxFactory.Create(policies: [policy]);

        ExecutionResult result = await database.ExecuteAsync("SELECT 1");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Internal);
        Assert.Contains("interner Fehler", error.Message);
        Assert.Contains("protokolliert", error.Message);
        Assert.Equal("InvalidOperationException: The policy failed on purpose.", error.Detail);
        Assert.Empty(result.Statements);
        Assert.Equal(1, policy.Evaluations);
    }

    [Fact]
    public async Task ThrowingPolicy_IsLoggedAsAnErrorWithTheException()
    {
        RecordingLogger<SandboxDatabase> logger = new RecordingLogger<SandboxDatabase>();
        await using SandboxDatabase database = SandboxFactory.Create(
            policies: [new ThrowingStatementPolicy()],
            logger: logger
        );

        await database.ExecuteAsync("SELECT 1");

        LogEntry entry = Assert.Single(logger.Entries, (LogEntry e) => e.Level == LogLevel.Error);
        Assert.IsType<InvalidOperationException>(entry.Exception);
        Assert.Equal("The policy failed on purpose.", entry.Exception.Message);
        Assert.Contains("execution pipeline", entry.Message);
    }

    [Fact]
    public async Task ThrowingPolicy_DoesNotEndTheSandbox()
    {
        ThrowingStatementPolicy policy = new ThrowingStatementPolicy();
        await using SandboxDatabase database = SandboxFactory.Create(policies: [policy]);
        await database.ExecuteAsync("SELECT 1");
        await database.ExecuteAsync("SELECT 2");

        policy.Throws = false;
        ExecutionResult recovered = await database.ExecuteAsync("SELECT 3");

        Assert.True(recovered.Succeeded, SandboxFactory.Describe(recovered));
        Assert.Equal(3L, recovered.ResultSets[0].Rows[0][0]);
        Assert.False(database.Status.IsBusy);
        Assert.True(database.Status.IsInitialized);
    }

    [Fact]
    public async Task ThrowingPolicy_NeverRunsAnyStatement()
    {
        ThrowingStatementPolicy policy = new ThrowingStatementPolicy { Throws = false };
        await using SandboxDatabase database = SandboxFactory.Create(policies: [policy]);
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");
        policy.Throws = true;

        await database.ExecuteAsync("INSERT INTO T VALUES (1)");

        policy.Throws = false;
        Assert.Equal(0L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task ThrowingPolicy_DoesNotAffectDialectErrors()
    {
        ThrowingStatementPolicy policy = new ThrowingStatementPolicy();
        await using SandboxDatabase database = SandboxFactory.Create(policies: [policy]);

        ExecutionResult result = await database.ExecuteAsync("SELECT FROM");

        result.ExpectError(ExecutionErrorKind.Diagnostics);
        Assert.Equal(0, policy.Evaluations);
    }

    [Fact]
    public async Task FailureInTheStatementLoop_YieldsAnInternalErrorWithThePosition()
    {
        RecordingLogger<SandboxDatabase> logger = new RecordingLogger<SandboxDatabase>();
        FaultyCooperativeYield yield = new FaultyCooperativeYield { ThrowOnYieldIfDue = true };
        await using SandboxDatabase database = SandboxFactory.Create(
            logger: logger,
            cooperativeYield: yield
        );

        ExecutionResult result = await database.ExecuteAsync("SELECT 1;\nSELECT 2");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Internal);
        Assert.Equal(1, error.Origin?.Start.Line);
        Assert.Contains("InvalidOperationException", error.Detail);
        Assert.Contains(
            logger.Entries,
            (LogEntry e) => e.Level == LogLevel.Error && e.Exception is InvalidOperationException
        );
    }

    [Fact]
    public async Task FailureInTheStatementLoop_LeavesTheSandboxUsable()
    {
        FaultyCooperativeYield yield = new FaultyCooperativeYield { ThrowOnYieldIfDue = true };
        await using SandboxDatabase database = SandboxFactory.Create(cooperativeYield: yield);
        await database.ExecuteAsync("SELECT 1");

        yield.ThrowOnYieldIfDue = false;
        ExecutionResult result = await database.ExecuteAsync("SELECT 4");

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
    }

    [Fact]
    public async Task FailureInTheStatementLoop_InRollbackMode_StillRestoresTheDatabase()
    {
        FaultyCooperativeYield yield = new FaultyCooperativeYield();
        await using SandboxDatabase database = SandboxFactory.Create(cooperativeYield: yield);
        await database.ExecuteOkAsync("CREATE TABLE T (a INT)");

        yield.ThrowOnYieldIfDue = true;
        ExecutionResult result = await database.ExecuteAsync(
            "INSERT INTO T VALUES (1)",
            SandboxFactory.Rollback()
        );

        result.ExpectError(ExecutionErrorKind.Internal);
        Assert.True(result.RolledBack);
        yield.ThrowOnYieldIfDue = false;
        Assert.Equal(0L, await database.CountRowsAsync("T"));
    }

    [Fact]
    public async Task FailingInitialization_YieldsAnInternalErrorAndIsLogged()
    {
        RecordingLogger<SandboxDatabase> logger = new RecordingLogger<SandboxDatabase>();
        FaultyCooperativeYield yield = new FaultyCooperativeYield { ThrowOnYield = true };
        await using SandboxDatabase database = SandboxFactory.Create(
            logger: logger,
            cooperativeYield: yield
        );

        ExecutionResult result = await database.ExecuteAsync("SELECT 1");

        ExecutionError error = result.ExpectError(ExecutionErrorKind.Internal);
        Assert.Contains("initialisiert", error.Message);
        Assert.False(database.Status.IsInitialized);
        Assert.Contains(logger.Entries, (LogEntry e) => e.Level == LogLevel.Error);
    }

    [Fact]
    public async Task FailingInitialization_CanBeRetriedAfterTheCauseIsGone()
    {
        FaultyCooperativeYield yield = new FaultyCooperativeYield { ThrowOnYield = true };
        await using SandboxDatabase database = SandboxFactory.Create(cooperativeYield: yield);
        await database.ExecuteAsync("SELECT 1");

        yield.ThrowOnYield = false;
        ExecutionResult result = await database.ExecuteAsync("SELECT 6");

        Assert.True(result.Succeeded, SandboxFactory.Describe(result));
        Assert.True(database.Status.IsInitialized);
    }

    [Fact]
    public async Task FailingInitialization_InProvisioning_YieldsAnInternalProvisioningError()
    {
        FaultyCooperativeYield yield = new FaultyCooperativeYield { ThrowOnYield = true };
        await using SandboxDatabase database = SandboxFactory.Create(cooperativeYield: yield);

        ProvisionResult result = await database.ProvisionScenarioAsync("starter");

        Assert.False(result.Succeeded);
        Assert.Equal(ProvisioningErrorKind.Internal, result.Error?.Kind);
        Assert.Null(result.ScenarioId);
    }

    [Fact]
    public async Task ExpectedUserErrors_AreNotLoggedAsErrors()
    {
        RecordingLogger<SandboxDatabase> logger = new RecordingLogger<SandboxDatabase>();
        await using SandboxDatabase database = SandboxFactory.Create(logger: logger);

        await database.ExecuteAsync("SELECT * FROM Missing");
        await database.ExecuteAsync("SELECT FROM");
        await database.ExecuteAsync("DROP VIEW COLUMNS");
        await database.ExecuteAsync(
            SandboxFactory.EndlessRecursiveQuery,
            SandboxFactory.Timeboxed()
        );

        Assert.DoesNotContain(logger.Entries, (LogEntry e) => e.Level >= LogLevel.Error);
    }
}
