using LearnSql.Sql.Dialect.Syntax;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Services.Execution;

public sealed class QueryRunner : IQueryRunner, IDisposable
{
    private readonly ISandboxDatabase _database;
    private readonly IAppSession _session;
    private readonly TimeProvider _time;
    private readonly ILogger<QueryRunner> _logger;

    private CancellationTokenSource? _cancellation;
    private long _startedAt;
    private ExecutionOptions _options = ExecutionOptions.Default;

    public QueryRunner(
        ISandboxDatabase database,
        IAppSession session,
        TimeProvider time,
        ILogger<QueryRunner> logger
    )
    {
        _database = database;
        _session = session;
        _time = time;
        _logger = logger;
        _session.StateChanged += OnSessionStateChanged;
        _session.DatabaseProvisioned += OnDatabaseProvisioned;
    }

    public event EventHandler? Changed;

    public event EventHandler<QueryRun>? Completed;

    public bool IsRunning { get; private set; }

    public QueryRun? LastRun { get; private set; }

    public TimeSpan Elapsed => IsRunning ? _time.GetElapsedTime(_startedAt) : TimeSpan.Zero;

    public ExecutionOptions Options
    {
        get => _options;
        set
        {
            _options = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task<QueryRunStatus> RunAsync(
        string sql,
        SourceOrigin? origin = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!_session.IsReady)
        {
            return QueryRunStatus.NotReady;
        }

        if (IsRunning)
        {
            return QueryRunStatus.AlreadyRunning;
        }

        if (string.IsNullOrWhiteSpace(sql))
        {
            return QueryRunStatus.EmptyInput;
        }

        ExecutionOptions options = _options;
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _startedAt = _time.GetTimestamp();
        IsRunning = true;
        Changed?.Invoke(this, EventArgs.Empty);

        ExecutionResult result;
        try
        {
            await Task.Delay(1, CancellationToken.None);
            result = await _database.ExecuteAsync(sql, options, _cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            result = CreateFailure(ExecutionErrorKind.Cancelled, string.Empty);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Executing a query failed unexpectedly.");
            result = CreateFailure(ExecutionErrorKind.Internal, exception.Message);
        }
        finally
        {
            _cancellation.Dispose();
            _cancellation = null;
        }

        IsRunning = false;
        QueryRun run = new QueryRun(sql, origin ?? SourceOrigin.Start, result, options);
        LastRun = run;
        if (ChangesSchema(result))
        {
            _session.NotifySchemaChanged();
        }

        Changed?.Invoke(this, EventArgs.Empty);
        Completed?.Invoke(this, run);
        return QueryRunStatus.Completed;
    }

    public void Cancel()
    {
        _cancellation?.Cancel();
    }

    public void ClearLastRun()
    {
        if (LastRun is null)
        {
            return;
        }

        LastRun = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _session.StateChanged -= OnSessionStateChanged;
        _session.DatabaseProvisioned -= OnDatabaseProvisioned;
        Cancel();
    }

    private void OnSessionStateChanged(object? sender, EventArgs e)
    {
        if (_session.Phase == AppPhase.Loading)
        {
            Cancel();
        }
    }

    private void OnDatabaseProvisioned(object? sender, DatabaseProvisionedEventArgs e)
    {
        if (e.Operation != AppOperation.Startup)
        {
            ClearLastRun();
        }
    }

    private ExecutionResult CreateFailure(ExecutionErrorKind kind, string message)
    {
        return new ExecutionResult(
            [],
            [],
            new ExecutionError(kind, message),
            _time.GetElapsedTime(_startedAt),
            false
        );
    }

    private static bool ChangesSchema(ExecutionResult result)
    {
        return !result.RolledBack
            && result.Statements.Any(
                (StatementResult statement) =>
                    statement.Kind
                        is StatementKind.SelectInto
                            or StatementKind.CreateTable
                            or StatementKind.AlterTable
                            or StatementKind.DropTable
                            or StatementKind.CreateView
                            or StatementKind.DropView
            );
    }
}
