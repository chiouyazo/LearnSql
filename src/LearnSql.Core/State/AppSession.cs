using LearnSql.Core.Services.Storage;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.State;

public sealed class AppSession : IAppSession, IDisposable
{
    private const double ProgressRenderStep = 0.01;

    private readonly ISandboxDatabase _database;
    private readonly ScenarioCatalog _catalog;
    private readonly IBrowserStorage _storage;
    private readonly ILogger<AppSession> _logger;
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

    private Func<Task>? _retry;
    private Task<DatabaseSchema>? _schema;
    private bool _engineReady;
    private bool _operationRunning;
    private bool _disposed;

    public AppSession(
        ISandboxDatabase database,
        ScenarioCatalog catalog,
        IBrowserStorage storage,
        ILogger<AppSession> logger
    )
    {
        _database = database;
        _catalog = catalog;
        _storage = storage;
        _logger = logger;
        Scenario = catalog.Scenarios[0];
    }

    public event EventHandler? StateChanged;

    public event EventHandler<DatabaseProvisionedEventArgs>? DatabaseProvisioned;

    public event EventHandler? SchemaChanged;

    public AppPhase Phase { get; private set; } = AppPhase.NotStarted;

    public AppOperation? Operation { get; private set; }

    public SandboxProgress? Progress { get; private set; }

    public AppFailure? Failure { get; private set; }

    public ScenarioDefinition Scenario { get; private set; }

    public IReadOnlyList<ScenarioDefinition> Scenarios => _catalog.Scenarios;

    public bool IsReady => Phase == AppPhase.Ready;

    public async Task RestoreScenarioAsync()
    {
        string? stored = await _storage.GetAsync(StorageKeys.Scenario);
        if (stored is not null && _catalog.TryGetScenario(stored, out ScenarioDefinition? known))
        {
            Scenario = known!;
        }
    }

    public void SelectInitialScenario(string scenarioId)
    {
        if (
            Phase == AppPhase.NotStarted
            && _catalog.TryGetScenario(scenarioId, out ScenarioDefinition? known)
        )
        {
            Scenario = known!;
        }
    }

    public Task EnsureStartedAsync()
    {
        if (Phase != AppPhase.NotStarted)
        {
            return Task.CompletedTask;
        }

        return StartOperationAsync(AppOperation.Startup, Scenario);
    }

    public Task RetryAsync()
    {
        if (Phase != AppPhase.Failed || _retry is null)
        {
            return Task.CompletedTask;
        }

        return _retry();
    }

    public Task SwitchScenarioAsync(string scenarioId)
    {
        if (
            !_catalog.TryGetScenario(scenarioId, out ScenarioDefinition? target)
            || (IsReady && target!.Id == Scenario.Id)
        )
        {
            return Task.CompletedTask;
        }

        return StartOperationAsync(AppOperation.SwitchScenario, target!);
    }

    public Task ResetDatabaseAsync()
    {
        return StartOperationAsync(AppOperation.ResetDatabase, Scenario);
    }

    public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        _schema ??= _database.GetSchemaAsync(_lifetime.Token);
        return _schema.WaitAsync(cancellationToken);
    }

    public void NotifySchemaChanged()
    {
        _schema = null;
        SchemaChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private async Task StartOperationAsync(AppOperation operation, ScenarioDefinition target)
    {
        if (_operationRunning)
        {
            return;
        }

        _operationRunning = true;
        _retry = () => StartOperationAsync(operation, target);
        Phase = AppPhase.Loading;
        Operation = operation;
        Progress = null;
        Failure = null;
        RaiseStateChanged();

        try
        {
            AppFailure? failure = await ProvisionAsync(operation, target);
            if (failure is null)
            {
                await CompleteAsync(operation, target);
            }
            else
            {
                Fail(failure);
            }
        }
        finally
        {
            _operationRunning = false;
        }
    }

    private async Task<AppFailure?> ProvisionAsync(
        AppOperation operation,
        ScenarioDefinition target
    )
    {
        Progress<SandboxProgress> progress = new Progress<SandboxProgress>(OnProgress);
        try
        {
            if (!_engineReady)
            {
                try
                {
                    await _database.InitializeAsync(progress, _lifetime.Token);
                }
                catch (SandboxInitializationException exception)
                {
                    _logger.LogError(exception, "The database engine could not be initialized.");
                    return new AppFailure(
                        AppFailureKind.EngineInitialization,
                        exception.InnerException?.Message ?? exception.Message
                    );
                }

                _engineReady = true;
            }

            ProvisionResult result =
                operation == AppOperation.ResetDatabase && _database.Status.ScenarioId is not null
                    ? await _database.ResetAsync(progress, _lifetime.Token)
                    : await _database.ProvisionScenarioAsync(target.Id, progress, _lifetime.Token);
            return result.Error is null ? null : ToFailure(result.Error);
        }
        catch (OperationCanceledException)
        {
            return new AppFailure(AppFailureKind.Cancelled, string.Empty);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Provisioning scenario {Scenario} failed.", target.Id);
            return new AppFailure(AppFailureKind.Internal, exception.Message);
        }
    }

    private async Task CompleteAsync(AppOperation operation, ScenarioDefinition target)
    {
        Scenario = target;
        Phase = AppPhase.Ready;
        Operation = null;
        Progress = null;
        _schema = null;
        RaiseStateChanged();
        DatabaseProvisioned?.Invoke(this, new DatabaseProvisionedEventArgs(target, operation));
        SchemaChanged?.Invoke(this, EventArgs.Empty);
        await _storage.SetAsync(StorageKeys.Scenario, target.Id);
    }

    private void Fail(AppFailure failure)
    {
        Phase = AppPhase.Failed;
        Failure = failure;
        Progress = null;
        RaiseStateChanged();
    }

    private void OnProgress(SandboxProgress progress)
    {
        bool significant =
            Progress is null
            || Progress.Phase != progress.Phase
            || Math.Abs(progress.Fraction - Progress.Fraction) >= ProgressRenderStep;
        if (!significant)
        {
            return;
        }

        Progress = progress;
        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static AppFailure ToFailure(ProvisioningError error)
    {
        AppFailureKind kind = error.Kind switch
        {
            ProvisioningErrorKind.UnknownScenario => AppFailureKind.UnknownScenario,
            ProvisioningErrorKind.InvalidScenario => AppFailureKind.InvalidScenario,
            ProvisioningErrorKind.ScriptFailed => AppFailureKind.ScriptFailed,
            ProvisioningErrorKind.Cancelled => AppFailureKind.Cancelled,
            _ => AppFailureKind.Internal,
        };
        string detail = error.MsSql?.ToString() ?? error.Message;
        return new AppFailure(
            kind,
            error.Resource is null ? detail : $"{error.Resource}\n{detail}"
        );
    }
}
