using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Resources;
using LearnSql.Sql.Translation;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LearnSql.Sql.Execution;

public sealed class SandboxDatabase : ISandboxDatabase
{
    private const int InitializationSteps = 2;

    private readonly ScenarioCatalog _catalog;
    private readonly SandboxOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ICooperativeYield _yield;
    private readonly ILogger<SandboxDatabase> _logger;
    private readonly SqliteFunctionRegistrar _registrar;
    private readonly SqliteSchemaCatalogReader _catalogReader = new SqliteSchemaCatalogReader();
    private readonly SqliteSchemaReader _schemaReader = new SqliteSchemaReader();
    private readonly ScenarioProvisioner _provisioner;
    private readonly QueryRunner _runner;
    private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

    private SandboxConnection? _connection;
    private TSqlDialectTranslator? _translator;
    private DatabaseSchema? _schemaCache;
    private string? _scenarioId;
    private SandboxActivity _activity = SandboxActivity.Idle;
    private SandboxStatus _status = SandboxStatus.Uninitialized;
    private bool _disposed;

    public SandboxDatabase(
        ScenarioCatalog catalog,
        IErrorTranslator errorTranslator,
        IEnumerable<IStatementPolicy> policies,
        ICooperativeYield cooperativeYield,
        SandboxOptions options,
        TimeProvider timeProvider,
        ILogger<SandboxDatabase> logger
    )
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(errorTranslator);
        ArgumentNullException.ThrowIfNull(policies);
        ArgumentNullException.ThrowIfNull(cooperativeYield);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        options.Validate();

        _catalog = catalog;
        _options = options;
        _timeProvider = timeProvider;
        _yield = cooperativeYield;
        _logger = logger;
        _registrar = new SqliteFunctionRegistrar(timeProvider);
        _provisioner = new ScenarioProvisioner(errorTranslator, cooperativeYield, logger);
        ExecutionErrorFactory errors = new ExecutionErrorFactory(
            errorTranslator,
            new ForeignKeyConflictResolver(),
            logger
        );
        ResultSetReader resultReader = new ResultSetReader(
            cooperativeYield,
            options.RowsPerYieldCheck
        );
        _runner = new QueryRunner(
            [.. policies],
            new SandboxAuthorizationPolicy(),
            cooperativeYield,
            new StatementRunner(resultReader, timeProvider),
            errors,
            options,
            timeProvider,
            logger
        );
    }

    public event EventHandler<SandboxStatus>? StatusChanged;

    public SandboxStatus Status => _status;

    public static SandboxDatabase CreateDefault(
        ILoggerFactory? loggerFactory = null,
        SandboxOptions? options = null
    )
    {
        SandboxOptions effective = options ?? new SandboxOptions();
        return new SandboxDatabase(
            ScenarioCatalog.LoadDefault(),
            ErrorTranslatorFactory.Create(Models.Language.MsSql),
            [new SystemObjectPolicy()],
            new TimeSliceYield(effective.YieldInterval),
            effective,
            TimeProvider.System,
            loggerFactory?.CreateLogger<SandboxDatabase>() ?? NullLogger<SandboxDatabase>.Instance
        );
    }

    public Task InitializeAsync(
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return RunExclusiveAsync(
            SandboxActivity.Initializing,
            async () =>
            {
                await EnsureInitializedAsync(progress, cancellationToken);
                return true;
            },
            cancellationToken
        );
    }

    public Task<ProvisionResult> ProvisionScenarioAsync(
        string scenarioId,
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(scenarioId);

        return ProvisionExclusiveAsync(() => scenarioId, progress, cancellationToken);
    }

    public Task<ProvisionResult> ResetAsync(
        IProgress<SandboxProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        return ProvisionExclusiveAsync(() => _scenarioId, progress, cancellationToken);
    }

    public async Task<ExecutionResult> ExecuteAsync(
        string tsql,
        ExecutionOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(tsql);
        ExecutionOptions effective = options ?? ExecutionOptions.Default;
        effective.Validate();
        long start = _timeProvider.GetTimestamp();

        try
        {
            return await RunExclusiveAsync(
                SandboxActivity.Executing,
                async () =>
                {
                    await EnsureInitializedAsync(null, cancellationToken);
                    QueryRunOutcome outcome = await _runner.RunAsync(
                        tsql,
                        _connection!,
                        _translator!,
                        effective,
                        ReadSchema,
                        cancellationToken
                    );
                    if (outcome.SchemaMayHaveChanged)
                    {
                        RefreshDialect(_connection!);
                    }

                    return outcome.Result;
                },
                cancellationToken
            );
        }
        catch (OperationCanceledException)
        {
            return Failed(
                new ExecutionError(ExecutionErrorKind.Cancelled, ExecutionStrings.Cancelled),
                start
            );
        }
        catch (SandboxInitializationException exception)
        {
            return Failed(
                new ExecutionError(
                    ExecutionErrorKind.Internal,
                    ExecutionStrings.DatabaseInitializationFailed,
                    exception.InnerException?.Message
                ),
                start
            );
        }
        catch (Exception exception) when (exception is not ObjectDisposedException)
        {
            _logger.LogError(exception, "Unexpected failure in the execution pipeline.");
            return Failed(
                new ExecutionError(
                    ExecutionErrorKind.Internal,
                    ExecutionStrings.UnexpectedExecutionError,
                    $"{exception.GetType().Name}: {exception.Message}"
                ),
                start
            );
        }
    }

    public Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        return RunExclusiveAsync(
            SandboxActivity.ReadingSchema,
            async () =>
            {
                await EnsureInitializedAsync(null, cancellationToken);
                return ReadSchema();
            },
            cancellationToken
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            _translator = null;
            _schemaCache = null;
            _scenarioId = null;
            PublishStatus();
        }
        finally
        {
            _gate.Release();
        }
    }

    private ExecutionResult Failed(ExecutionError error, long start)
    {
        return new ExecutionResult([], [], error, _timeProvider.GetElapsedTime(start), false);
    }

    private DatabaseSchema ReadSchema()
    {
        SandboxConnection connection =
            _connection ?? throw new InvalidOperationException("The database is not initialized.");
        _schemaCache ??= _schemaReader.Read(connection.Connection);
        return _schemaCache;
    }

    private async Task<T> RunExclusiveAsync<T>(
        SandboxActivity activity,
        Func<Task<T>> body,
        CancellationToken cancellationToken
    )
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _activity = activity;
            PublishStatus();
            return await body();
        }
        finally
        {
            _activity = SandboxActivity.Idle;
            PublishStatus();
            _gate.Release();
        }
    }

    private void PublishStatus()
    {
        SandboxStatus status = new SandboxStatus(
            _connection is not null,
            _scenarioId,
            _activity,
            _connection is not null && !_connection.IsAutocommit
        );
        if (status == _status)
        {
            return;
        }

        _status = status;
        StatusChanged?.Invoke(this, status);
    }

    private async Task EnsureInitializedAsync(
        IProgress<SandboxProgress>? progress,
        CancellationToken cancellationToken
    )
    {
        if (_connection is not null)
        {
            return;
        }

        try
        {
            progress?.Report(
                new SandboxProgress(
                    SandboxPhase.PreparingEngine,
                    0,
                    InitializationSteps,
                    ExecutionStrings.ProgressInitializingEngine
                )
            );
            await _yield.YieldAsync(cancellationToken);
            SandboxConnection connection = await SandboxConnection.OpenAsync(
                _options,
                _registrar,
                cancellationToken
            );
            try
            {
                progress?.Report(
                    new SandboxProgress(
                        SandboxPhase.ReadingSchema,
                        1,
                        InitializationSteps,
                        ExecutionStrings.ProgressReadingSchema
                    )
                );
                await _yield.YieldAsync(cancellationToken);
                TSqlDialectTranslator translator = BuildTranslator(connection);
                _connection = connection;
                _translator = translator;
                _schemaCache = null;
                _scenarioId = null;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }

            progress?.Report(
                new SandboxProgress(
                    SandboxPhase.Ready,
                    InitializationSteps,
                    InitializationSteps,
                    ExecutionStrings.ProgressReady
                )
            );
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "The sandbox database could not be initialized.");
            throw new SandboxInitializationException(
                "The sandbox database could not be initialized.",
                exception
            );
        }
    }

    private async Task<ProvisionResult> ProvisionExclusiveAsync(
        Func<string?> scenarioSelector,
        IProgress<SandboxProgress>? progress,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await RunExclusiveAsync(
                SandboxActivity.Provisioning,
                async () =>
                {
                    await EnsureInitializedAsync(null, cancellationToken);
                    return await ProvisionAsync(scenarioSelector(), progress, cancellationToken);
                },
                cancellationToken
            );
        }
        catch (OperationCanceledException)
        {
            return new ProvisionResult(
                _scenarioId,
                new ProvisioningError(
                    ProvisioningErrorKind.Cancelled,
                    ExecutionStrings.ProvisioningCancelled
                )
            );
        }
        catch (SandboxInitializationException exception)
        {
            return new ProvisionResult(
                _scenarioId,
                new ProvisioningError(
                    ProvisioningErrorKind.Internal,
                    ExecutionStrings.DatabaseInitializationFailed,
                    Detail: exception.InnerException?.Message
                )
            );
        }
    }

    private async Task<ProvisionResult> ProvisionAsync(
        string? scenarioId,
        IProgress<SandboxProgress>? progress,
        CancellationToken cancellationToken
    )
    {
        ScenarioScripts? scripts = null;
        if (scenarioId is not null)
        {
            ProvisioningError? lookupError = TryGetScripts(scenarioId, out scripts);
            if (lookupError is not null)
            {
                return new ProvisionResult(_scenarioId, lookupError);
            }
        }

        int scriptCount = scripts?.InExecutionOrder.Count() ?? 0;
        int totalSteps = scriptCount + 2;
        progress?.Report(
            new SandboxProgress(
                SandboxPhase.ResettingDatabase,
                0,
                totalSteps,
                ExecutionStrings.ProgressResettingDatabase
            )
        );
        await _yield.YieldAsync(cancellationToken);

        SandboxConnection fresh;
        try
        {
            fresh = await SandboxConnection.OpenAsync(_options, _registrar, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new ProvisionResult(_scenarioId, InternalError(exception));
        }

        TSqlDialectTranslator? translator = null;
        ProvisioningError? error = null;
        try
        {
            if (scripts is not null)
            {
                error = await _provisioner.ApplyAsync(
                    fresh,
                    scripts,
                    1,
                    totalSteps,
                    progress,
                    cancellationToken
                );
            }

            if (error is null)
            {
                progress?.Report(
                    new SandboxProgress(
                        SandboxPhase.ReadingSchema,
                        totalSteps - 1,
                        totalSteps,
                        ExecutionStrings.ProgressReadingSchema
                    )
                );
                await _yield.YieldAsync(cancellationToken);
                translator = BuildTranslator(fresh);
            }
        }
        catch (OperationCanceledException)
        {
            await fresh.DisposeAsync();
            throw;
        }
        catch (Exception exception)
        {
            error = InternalError(exception);
        }

        if (error is not null || translator is null)
        {
            await fresh.DisposeAsync();
            return new ProvisionResult(_scenarioId, error ?? InternalError(null));
        }

        SandboxConnection? previous = _connection;
        _connection = fresh;
        _translator = translator;
        _schemaCache = null;
        _scenarioId = scripts?.ScenarioId;
        if (previous is not null)
        {
            await previous.DisposeAsync();
        }

        progress?.Report(
            new SandboxProgress(
                SandboxPhase.Ready,
                totalSteps,
                totalSteps,
                ExecutionStrings.ProgressReady
            )
        );
        return new ProvisionResult(_scenarioId, null);
    }

    private ProvisioningError? TryGetScripts(string scenarioId, out ScenarioScripts? scripts)
    {
        scripts = null;
        if (
            !_catalog.TryGetScenario(scenarioId, out ScenarioDefinition? definition)
            || definition is null
        )
        {
            string known = string.Join(
                ", ",
                _catalog.Scenarios.Select((ScenarioDefinition s) => s.Id)
            );
            return new ProvisioningError(
                ProvisioningErrorKind.UnknownScenario,
                ExecutionStrings.UnknownScenario(scenarioId, known)
            );
        }

        try
        {
            scripts = _catalog.GetScripts(definition.Id);
            return null;
        }
        catch (ScenarioCatalogException exception)
        {
            _logger.LogError(exception, "The scenario {ScenarioId} is invalid.", definition.Id);
            return new ProvisioningError(
                ProvisioningErrorKind.InvalidScenario,
                ExecutionStrings.InvalidScenario(definition.Id, exception.Message),
                exception.Resource,
                exception.Message
            );
        }
    }

    private ProvisioningError InternalError(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogError(exception, "Unexpected failure while provisioning the scenario.");
        }

        return new ProvisioningError(
            ProvisioningErrorKind.Internal,
            ExecutionStrings.UnexpectedProvisioningError,
            Detail: exception is null ? null : $"{exception.GetType().Name}: {exception.Message}"
        );
    }

    private TSqlDialectTranslator BuildTranslator(SandboxConnection connection)
    {
        SchemaCatalog catalog = _catalogReader.Read(connection.Connection);
        return TSqlDialectTranslator.CreateDefault(catalog);
    }

    private void RefreshDialect(SandboxConnection connection)
    {
        _schemaCache = null;
        try
        {
            _translator = BuildTranslator(connection);
        }
        catch (SqliteException exception)
        {
            _logger.LogError(
                exception,
                "The schema catalog could not be refreshed. Type inference may be outdated."
            );
        }
    }
}
