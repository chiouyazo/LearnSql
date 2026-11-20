namespace LearnSql.Sql.Execution;

public interface ISandboxDatabase : IAsyncDisposable
{
    SandboxStatus Status { get; }

    event EventHandler<SandboxStatus>? StatusChanged;

    Task InitializeAsync(
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<ProvisionResult> ProvisionScenarioAsync(
        string scenarioId,
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<ProvisionResult> ResetAsync(
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<ExecutionResult> ExecuteAsync(
        string tsql,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default
    );

    Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
}
