namespace LearnSql.Core.Services.Exercises;

public sealed class ExerciseChecker : IExerciseChecker, IDisposable
{
    private static readonly ExecutionOptions ReferenceOptions = new ExecutionOptions
    {
        RollbackAfterExecution = true,
    };

    private readonly IQueryRunner _runner;
    private readonly ISandboxDatabase _database;
    private readonly IAppSession _session;

    public ExerciseChecker(IQueryRunner runner, ISandboxDatabase database, IAppSession session)
    {
        _runner = runner;
        _database = database;
        _session = session;
        _runner.Completed += OnRunCompleted;
        _session.DatabaseProvisioned += OnDatabaseProvisioned;
    }

    public bool DatabaseModified { get; private set; }

    public async Task<ExerciseCheckResult> CheckAsync(
        ExerciseDefinition exercise,
        string sql,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(sql);

        QueryRunStatus status = await _runner.RunAsync(sql, SourceOrigin.Start, cancellationToken);
        switch (status)
        {
            case QueryRunStatus.EmptyInput:
                return new ExerciseCheckResult(ExerciseCheckOutcome.EmptyInput);
            case QueryRunStatus.NotReady:
                return new ExerciseCheckResult(ExerciseCheckOutcome.NotReady);
            case QueryRunStatus.AlreadyRunning:
                return new ExerciseCheckResult(ExerciseCheckOutcome.Busy);
        }

        QueryRun? run = _runner.LastRun;
        if (run is null || !ReferenceEquals(run.Sql, sql))
        {
            return new ExerciseCheckResult(ExerciseCheckOutcome.Cancelled);
        }

        return await EvaluateAsync(exercise, run.Result, cancellationToken);
    }

    public void Dispose()
    {
        _runner.Completed -= OnRunCompleted;
        _session.DatabaseProvisioned -= OnDatabaseProvisioned;
    }

    private async Task<ExerciseCheckResult> EvaluateAsync(
        ExerciseDefinition exercise,
        ExecutionResult user,
        CancellationToken cancellationToken
    )
    {
        if (user.Error is ExecutionError error)
        {
            return error.Kind == ExecutionErrorKind.Cancelled
                ? new ExerciseCheckResult(ExerciseCheckOutcome.Cancelled)
                : new ExerciseCheckResult(ExerciseCheckOutcome.QueryFailed, Error: error);
        }

        if (DatabaseModified)
        {
            return new ExerciseCheckResult(ExerciseCheckOutcome.DatabaseModified);
        }

        IReadOnlyList<ResultSet> userSets = user.ResultSets;
        if (userSets.Count == 0)
        {
            return new ExerciseCheckResult(ExerciseCheckOutcome.NoResultSet);
        }

        ExecutionResult reference = await _database.ExecuteAsync(
            exercise.SolutionSql,
            ReferenceOptions,
            cancellationToken
        );
        if (reference.Error is ExecutionError referenceError)
        {
            return referenceError.Kind == ExecutionErrorKind.Cancelled
                ? new ExerciseCheckResult(ExerciseCheckOutcome.Cancelled)
                : new ExerciseCheckResult(
                    ExerciseCheckOutcome.ReferenceFailed,
                    Error: referenceError
                );
        }

        IReadOnlyList<ResultSet> referenceSets = reference.ResultSets;
        if (referenceSets.Count == 0)
        {
            return new ExerciseCheckResult(ExerciseCheckOutcome.ReferenceFailed);
        }

        ResultComparison comparison = ResultComparer.Compare(
            referenceSets[^1],
            userSets[^1],
            exercise.OrderMatters
        );
        return new ExerciseCheckResult(
            comparison.IsEqual ? ExerciseCheckOutcome.Correct : ExerciseCheckOutcome.Wrong,
            comparison,
            ResultSetCount: userSets.Count
        );
    }

    private void OnRunCompleted(object? sender, QueryRun run)
    {
        if (DataChangeDetector.ChangesDatabase(run.Result))
        {
            DatabaseModified = true;
        }
    }

    private void OnDatabaseProvisioned(object? sender, DatabaseProvisionedEventArgs e)
    {
        DatabaseModified = false;
    }
}
