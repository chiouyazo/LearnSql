namespace LearnSql.Core.Services.Execution;

public interface IQueryRunner
{
    bool IsRunning { get; }

    TimeSpan Elapsed { get; }

    QueryRun? LastRun { get; }

    ExecutionOptions Options { get; set; }

    event EventHandler? Changed;

    event EventHandler<QueryRun>? Completed;

    Task<QueryRunStatus> RunAsync(
        string sql,
        SourceOrigin? origin = null,
        CancellationToken cancellationToken = default
    );

    void Cancel();

    void ClearLastRun();
}
