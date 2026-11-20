using LearnSql.Core.Services.Execution;
using LearnSql.Core.Services.Exercises;
using LearnSql.Core.State;
using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using AppQueryRunner = LearnSql.Core.Services.Execution.QueryRunner;

namespace LearnSql.Tests.Core;

public sealed class ExerciseCheckerTests : IAsyncLifetime
{
    private SandboxDatabase _database = null!;
    private AppSession _session = null!;
    private AppQueryRunner _runner = null!;
    private ExerciseChecker _checker = null!;

    public async Task InitializeAsync()
    {
        _database = SandboxDatabase.CreateDefault();
        _session = new AppSession(
            _database,
            ScenarioData.Catalog,
            new ExerciseMemoryStorage(),
            NullLogger<AppSession>.Instance
        );
        _runner = new AppQueryRunner(
            _database,
            _session,
            TimeProvider.System,
            NullLogger<QueryRunner>.Instance
        );
        _checker = new ExerciseChecker(_runner, _database, _session);
        await _session.SwitchScenarioAsync("starter");
        Assert.True(_session.IsReady);
    }

    public async Task DisposeAsync()
    {
        _checker.Dispose();
        _runner.Dispose();
        _session.Dispose();
        await _database.DisposeAsync();
    }

    private static ExerciseDefinition Starter(string id)
    {
        return ScenarioData.Exercise("starter", id);
    }

    private Task<ExerciseCheckResult> CheckAsync(string exerciseId, string sql)
    {
        return _checker.CheckAsync(Starter(exerciseId), sql);
    }

    [Theory]
    [InlineData("select-03", "SELECT TOP 3 Name, Price FROM Product ORDER BY Price DESC")]
    [InlineData("select-02", "select firstname, lastname, city from customer")]
    [InlineData("filter-01", "SELECT * FROM Customer WHERE City LIKE 'Hamburg'")]
    [InlineData("agg-01", "SELECT COUNT(Id) AS n FROM Orders")]
    [InlineData(
        "join-03",
        "SELECT FirstName, LastName FROM Customer WHERE Id NOT IN (SELECT CustomerId FROM Orders)"
    )]
    public async Task Check_WithAnEquivalentQuery_IsCorrect(string exerciseId, string sql)
    {
        ExerciseCheckResult result = await CheckAsync(exerciseId, sql);

        Assert.Equal(ExerciseCheckOutcome.Correct, result.Outcome);
        Assert.True(result.CountsAsAttempt);
        Assert.NotNull(result.Comparison);
        Assert.True(result.Comparison.IsEqual);
    }

    [Fact]
    public async Task Check_WithTheModelSolution_IsCorrectForEveryStarterExercise()
    {
        foreach (
            ExerciseDefinition exercise in ScenarioData.Catalog.GetScenario("starter").Exercises
        )
        {
            ExerciseCheckResult result = await _checker.CheckAsync(exercise, exercise.SolutionSql);

            Assert.True(result.Outcome == ExerciseCheckOutcome.Correct, exercise.Id);
        }
    }

    [Fact]
    public async Task Check_UnorderedExercise_AcceptsAnyRowOrder()
    {
        ExerciseCheckResult result = await CheckAsync(
            "select-01",
            "SELECT * FROM Customer ORDER BY LastName DESC"
        );

        Assert.Equal(ExerciseCheckOutcome.Correct, result.Outcome);
    }

    [Fact]
    public async Task Check_OrderedExercise_WithReversedOrder_IsWrongBecauseOfTheOrder()
    {
        ExerciseCheckResult result = await CheckAsync(
            "join-01",
            "SELECT o.Id, o.OrderDate, c.FirstName, c.LastName FROM Orders o INNER JOIN Customer c ON c.Id = o.CustomerId ORDER BY o.Id DESC"
        );

        Assert.Equal(ExerciseCheckOutcome.Wrong, result.Outcome);
        Assert.NotNull(result.Comparison);
        Assert.True(result.Comparison.OrderDiffers);
        Assert.Empty(result.Comparison.MissingRows);
        Assert.Empty(result.Comparison.ExtraRows);
    }

    [Fact]
    public async Task Check_OrderedExercise_WithTheOppositeSortDirection_IsWrong()
    {
        ExerciseCheckResult result = await CheckAsync(
            "select-03",
            "SELECT TOP 3 Name, Price FROM Product ORDER BY Price ASC"
        );

        Assert.Equal(ExerciseCheckOutcome.Wrong, result.Outcome);
    }

    [Fact]
    public async Task Check_MissingWhere_ReportsTheSurplusRows()
    {
        ExerciseCheckResult result = await CheckAsync("filter-01", "SELECT * FROM Customer");

        Assert.Equal(ExerciseCheckOutcome.Wrong, result.Outcome);
        ResultComparison comparison = Assert.IsType<ResultComparison>(result.Comparison);
        Assert.Equal(3, comparison.ExpectedRowCount);
        Assert.Equal(11, comparison.ActualRowCount);
        Assert.Equal(8, comparison.ExtraRows.Count);
        Assert.Empty(comparison.MissingRows);
    }

    [Fact]
    public async Task Check_TooRestrictiveWhere_ReportsMissingRows()
    {
        ExerciseCheckResult result = await CheckAsync(
            "filter-01",
            "SELECT * FROM Customer WHERE City = 'Hamburg' AND Id < 0"
        );

        ResultComparison comparison = Assert.IsType<ResultComparison>(result.Comparison);
        Assert.Equal(ExerciseCheckOutcome.Wrong, result.Outcome);
        Assert.Equal(3, comparison.MissingRows.Count);
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public async Task Check_WrongColumnCount_IsWrongWithColumnDetails()
    {
        ExerciseCheckResult result = await CheckAsync(
            "select-02",
            "SELECT FirstName FROM Customer"
        );

        ResultComparison comparison = Assert.IsType<ResultComparison>(result.Comparison);
        Assert.Equal(ExerciseCheckOutcome.Wrong, result.Outcome);
        Assert.True(comparison.ColumnCountDiffers);
        Assert.Equal(3, comparison.ExpectedColumns.Count);
        Assert.Single(comparison.ActualColumns);
    }

    [Fact]
    public async Task Check_SyntaxError_IsAFailedQueryNotAWrongResult()
    {
        ExerciseCheckResult result = await CheckAsync("select-01", "SELEC * FROM Customer");

        Assert.Equal(ExerciseCheckOutcome.QueryFailed, result.Outcome);
        Assert.NotNull(result.Error);
        Assert.Null(result.Comparison);
        Assert.True(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_UnknownTable_IsAFailedQuery()
    {
        ExerciseCheckResult result = await CheckAsync("select-01", "SELECT * FROM Missing");

        Assert.Equal(ExerciseCheckOutcome.QueryFailed, result.Outcome);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Check_ScriptWithoutResultSet_ExplainsThatASelectIsExpected()
    {
        ExerciseCheckResult result = await CheckAsync("select-01", "DECLARE @x INT = 1");

        Assert.Equal(ExerciseCheckOutcome.NoResultSet, result.Outcome);
        Assert.True(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_UsesTheLastResultSetOfAScript()
    {
        ExerciseDefinition exercise = Starter("agg-01");

        ExerciseCheckResult lastIsRight = await _checker.CheckAsync(
            exercise,
            "SELECT 1 AS x; " + exercise.SolutionSql
        );
        ExerciseCheckResult lastIsWrong = await _checker.CheckAsync(
            exercise,
            exercise.SolutionSql + "; SELECT 1 AS x"
        );

        Assert.Equal(ExerciseCheckOutcome.Correct, lastIsRight.Outcome);
        Assert.Equal(2, lastIsRight.ResultSetCount);
        Assert.Equal(ExerciseCheckOutcome.Wrong, lastIsWrong.Outcome);
        Assert.Equal(2, lastIsWrong.ResultSetCount);
    }

    [Fact]
    public async Task Check_EmptyEditor_ReportsEmptyInputWithoutCountingAnAttempt()
    {
        ExerciseCheckResult result = await CheckAsync("select-01", "   ");

        Assert.Equal(ExerciseCheckOutcome.EmptyInput, result.Outcome);
        Assert.False(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_BeforeTheDatabaseIsReady_ReportsNotReady()
    {
        await using SandboxDatabase cold = SandboxDatabase.CreateDefault();
        using AppSession session = new AppSession(
            cold,
            ScenarioData.Catalog,
            new ExerciseMemoryStorage(),
            NullLogger<AppSession>.Instance
        );
        using AppQueryRunner runner = new AppQueryRunner(
            cold,
            session,
            TimeProvider.System,
            NullLogger<QueryRunner>.Instance
        );
        using ExerciseChecker checker = new ExerciseChecker(runner, cold, session);

        ExerciseCheckResult result = await checker.CheckAsync(Starter("select-01"), "SELECT 1");

        Assert.Equal(ExerciseCheckOutcome.NotReady, result.Outcome);
        Assert.False(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_WhileAnotherQueryRuns_DoesNotStartASecondRun()
    {
        _runner.Options = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(30) };
        Task<QueryRunStatus> running = _runner.RunAsync(SandboxFactory.EndlessRecursiveQuery);
        await WaitUntilAsync(() => _runner.IsRunning);

        ExerciseCheckResult result = await CheckAsync("select-01", "SELECT * FROM Customer");
        _runner.Cancel();
        await running;

        Assert.Equal(ExerciseCheckOutcome.Busy, result.Outcome);
        Assert.False(result.CountsAsAttempt);
        Assert.False(_runner.IsRunning);
    }

    [Fact]
    public async Task Check_CancelledWhileRunning_ReportsCancelled()
    {
        _runner.Options = new ExecutionOptions { Timeout = TimeSpan.FromSeconds(30) };
        Task<ExerciseCheckResult> check = CheckAsync(
            "select-01",
            SandboxFactory.EndlessRecursiveQuery
        );
        await WaitUntilAsync(() => _runner.IsRunning);

        _runner.Cancel();
        ExerciseCheckResult result = await check;

        Assert.Equal(ExerciseCheckOutcome.Cancelled, result.Outcome);
        Assert.False(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_TimeoutOfTheUserQuery_IsAFailedQuery()
    {
        _runner.Options = new ExecutionOptions { Timeout = TimeSpan.FromMilliseconds(200) };

        ExerciseCheckResult result = await CheckAsync(
            "select-01",
            SandboxFactory.EndlessRecursiveQuery
        );

        Assert.Equal(ExerciseCheckOutcome.QueryFailed, result.Outcome);
        Assert.Equal(ExecutionErrorKind.Timeout, result.Error?.Kind);
    }

    [Fact]
    public async Task Check_AfterTheUserChangedData_RefusesToJudgeUntilTheDatabaseIsReset()
    {
        await _runner.RunAsync("UPDATE Product SET Price = Price + 1");
        Assert.True(_checker.DatabaseModified);

        ExerciseCheckResult blocked = await CheckAsync("select-01", "SELECT * FROM Customer");
        await _session.ResetDatabaseAsync();
        ExerciseCheckResult afterReset = await CheckAsync("select-01", "SELECT * FROM Customer");

        Assert.Equal(ExerciseCheckOutcome.DatabaseModified, blocked.Outcome);
        Assert.False(blocked.CountsAsAttempt);
        Assert.False(_checker.DatabaseModified);
        Assert.Equal(ExerciseCheckOutcome.Correct, afterReset.Outcome);
    }

    [Fact]
    public async Task Check_WithAScriptThatChangesData_IsNotJudged()
    {
        ExerciseCheckResult result = await CheckAsync(
            "select-01",
            "UPDATE Product SET Stock = Stock; SELECT * FROM Customer"
        );

        Assert.Equal(ExerciseCheckOutcome.DatabaseModified, result.Outcome);
        Assert.True(_checker.DatabaseModified);
    }

    [Fact]
    public async Task Check_WithRollbackMode_DoesNotMarkTheDatabaseAsModified()
    {
        _runner.Options = new ExecutionOptions { RollbackAfterExecution = true };

        ExerciseCheckResult result = await CheckAsync(
            "select-01",
            "UPDATE Product SET Stock = 0; SELECT * FROM Customer"
        );

        Assert.Equal(ExerciseCheckOutcome.Correct, result.Outcome);
        Assert.False(_checker.DatabaseModified);
    }

    [Fact]
    public async Task Check_ModelSolutionRunsIsolated_AndNeverChangesTheUserDatabase()
    {
        ExerciseDefinition mutating = ExerciseTestData.Exercise(
            "mutating",
            solutionSql: "UPDATE Product SET Price = 0; SELECT SUM(Price) AS Total FROM Product"
        );

        ExerciseCheckResult first = await _checker.CheckAsync(mutating, "SELECT 0 AS Total");
        object? afterwards = await _database.ScalarAsync("SELECT MIN(Price) FROM Product");

        Assert.Equal(ExerciseCheckOutcome.Correct, first.Outcome);
        Assert.NotEqual(
            0.0,
            Convert.ToDouble(afterwards, System.Globalization.CultureInfo.InvariantCulture)
        );
        Assert.False(_checker.DatabaseModified);
    }

    [Fact]
    public async Task Check_FailingModelSolution_IsReportedAsAnExerciseProblem()
    {
        ExerciseDefinition broken = ExerciseTestData.Exercise(
            "broken",
            solutionSql: "SELECT * FROM Missing"
        );

        ExerciseCheckResult result = await _checker.CheckAsync(broken, "SELECT 1");

        Assert.Equal(ExerciseCheckOutcome.ReferenceFailed, result.Outcome);
        Assert.NotNull(result.Error);
        Assert.False(result.CountsAsAttempt);
    }

    [Fact]
    public async Task Check_ModelSolutionWithoutResultSet_IsReportedAsAnExerciseProblem()
    {
        ExerciseDefinition silent = ExerciseTestData.Exercise(
            "silent",
            solutionSql: "DECLARE @x INT = 1"
        );

        ExerciseCheckResult result = await _checker.CheckAsync(silent, "SELECT 1");

        Assert.Equal(ExerciseCheckOutcome.ReferenceFailed, result.Outcome);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Check_AfterSwitchingTheScenario_UsesTheDataOfTheNewScenario()
    {
        await _session.SwitchScenarioAsync("analytics");
        ExerciseDefinition exercise = ScenarioData.Exercise("analytics", "select-02");

        ExerciseCheckResult result = await _checker.CheckAsync(exercise, exercise.SolutionSql);

        Assert.Equal(ExerciseCheckOutcome.Correct, result.Outcome);
    }

    [Fact]
    public async Task Check_RunsTheUserQueryThroughTheSharedRunner()
    {
        QueryRun? completed = null;
        _runner.Completed += (object? sender, QueryRun run) => completed = run;

        await CheckAsync("select-01", "SELECT * FROM Customer");

        Assert.NotNull(completed);
        Assert.Same(completed, _runner.LastRun);
        Assert.Equal("SELECT * FROM Customer", completed.Sql);
    }

    [Fact]
    public async Task Check_NullArguments_Throw()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _checker.CheckAsync(null!, "SELECT 1")
        );
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _checker.CheckAsync(Starter("select-01"), null!)
        );
    }

    [Theory]
    [InlineData(StatementKind.Insert, false, true)]
    [InlineData(StatementKind.Update, false, true)]
    [InlineData(StatementKind.Delete, false, true)]
    [InlineData(StatementKind.TruncateTable, false, true)]
    [InlineData(StatementKind.CreateTable, false, true)]
    [InlineData(StatementKind.AlterTable, false, true)]
    [InlineData(StatementKind.DropTable, false, true)]
    [InlineData(StatementKind.CreateView, false, true)]
    [InlineData(StatementKind.DropView, false, true)]
    [InlineData(StatementKind.CreateIndex, false, true)]
    [InlineData(StatementKind.DropIndex, false, true)]
    [InlineData(StatementKind.SelectInto, false, true)]
    [InlineData(StatementKind.Update, true, false)]
    [InlineData(StatementKind.Select, false, false)]
    [InlineData(StatementKind.DeclareVariable, false, false)]
    [InlineData(StatementKind.SetVariable, false, false)]
    [InlineData(StatementKind.SetOption, false, false)]
    public void DataChangeDetector_ClassifiesStatementKinds(
        StatementKind kind,
        bool rolledBack,
        bool expected
    )
    {
        StatementResult statement = new StatementResult(
            0,
            0,
            kind,
            default,
            [],
            null,
            TimeSpan.Zero,
            []
        );
        ExecutionResult result = new ExecutionResult(
            [statement],
            [],
            null,
            TimeSpan.Zero,
            rolledBack
        );

        Assert.Equal(expected, DataChangeDetector.ChangesDatabase(result));
    }

    [Fact]
    public void DataChangeDetector_WithoutStatements_IsFalse()
    {
        ExecutionResult result = new ExecutionResult([], [], null, TimeSpan.Zero, false);

        Assert.False(DataChangeDetector.ChangesDatabase(result));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "The condition was never met.");
            await Task.Delay(5);
        }
    }
}
