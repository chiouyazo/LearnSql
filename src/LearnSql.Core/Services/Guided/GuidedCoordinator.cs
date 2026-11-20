using LearnSql.Core.Services.Drafts;
using LearnSql.Core.Services.Storage;
using LearnSql.Core.Services.Templates;
using LearnSql.Core.Services.Workspace;

namespace LearnSql.Core.Services.Guided;

public sealed class GuidedCoordinator : IDisposable
{
    private readonly IAppSession _session;
    private readonly IQueryRunner _runner;
    private readonly IWorkspaceEditor _editor;
    private readonly IDraftStore _drafts;
    private readonly IBrowserStorage _storage;
    private readonly IQueryTemplateFactory _templates;
    private readonly TimeProvider _time;

    private GuidedRunMachine? _machine;
    private string _runScenarioId = string.Empty;
    private string? _lastInserted;
    private string? _backup;
    private string _backupScenarioId = string.Empty;
    private bool _working;
    private bool _selfReset;
    private CancellationTokenSource? _countdown;

    public GuidedCoordinator(
        IAppSession session,
        IQueryRunner runner,
        IWorkspaceEditor editor,
        IDraftStore drafts,
        IBrowserStorage storage,
        IQueryTemplateFactory templates,
        TimeProvider time
    )
    {
        _session = session;
        _runner = runner;
        _editor = editor;
        _drafts = drafts;
        _storage = storage;
        _templates = templates;
        _time = time;
        _session.DatabaseProvisioned += OnDatabaseProvisioned;
        _runner.Completed += OnRunCompleted;
        _runner.Changed += OnRunnerChanged;
    }

    public event EventHandler? Changed;

    public GuidedRunMachine? Machine => _machine;

    public bool IsBusy => _working || _runner.IsRunning || _session.Phase == AppPhase.Loading;

    public bool IsPlaying { get; private set; }

    public bool IsCountingDown => _countdown is not null;

    public int CountdownSeconds { get; private set; }

    public bool ShowsEarlierRun { get; private set; }

    public GuidedNotice Notice { get; private set; }

    public bool HasBackup => _backup is not null && _backupScenarioId == _session.Scenario.Id;

    public async Task InitializeAsync()
    {
        await LoadBackupAsync(_session.Scenario.Id);
        RaiseChanged();
    }

    public async Task<GuidedActionResult> StartAsync(GuidedScenarioDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (IsBusy)
        {
            return GuidedActionResult.Busy;
        }

        if (!_session.IsReady)
        {
            return GuidedActionResult.NotReady;
        }

        CancelCountdown();
        _working = true;
        Notice = GuidedNotice.None;
        RaiseChanged();
        try
        {
            if (definition.RequiresFreshDatabase && !await ResetCoreAsync())
            {
                Notice = GuidedNotice.ResetFailed;
                return GuidedActionResult.Unavailable;
            }

            _runScenarioId = _session.Scenario.Id;
            _lastInserted = null;
            _machine = new GuidedRunMachine(definition);
            IsPlaying = true;
            GuidedActionResult result = await EnterCurrentAsync();
            if (result != GuidedActionResult.Done)
            {
                _machine = null;
                IsPlaying = false;
            }

            return result;
        }
        finally
        {
            _working = false;
            BeginCountdown();
            RaiseChanged();
        }
    }

    public async Task<GuidedActionResult> NextAsync()
    {
        GuidedRunMachine? machine = _machine;
        if (machine is null || !machine.CanMoveNext)
        {
            return GuidedActionResult.Unavailable;
        }

        if (IsBusy)
        {
            return GuidedActionResult.Busy;
        }

        CancelCountdown();
        _working = true;
        Notice = GuidedNotice.None;
        RaiseChanged();
        try
        {
            machine.MoveNext();
            GuidedActionResult result = await EnterCurrentAsync();
            if (result != GuidedActionResult.Done)
            {
                machine.MovePrevious();
                IsPlaying = false;
            }

            return result;
        }
        finally
        {
            _working = false;
            BeginCountdown();
            RaiseChanged();
        }
    }

    public async Task<GuidedActionResult> PreviousAsync()
    {
        GuidedRunMachine? machine = _machine;
        if (machine is null || !machine.CanMoveBack)
        {
            return GuidedActionResult.Unavailable;
        }

        if (IsBusy)
        {
            return GuidedActionResult.Busy;
        }

        CancelCountdown();
        IsPlaying = false;
        _working = true;
        Notice = GuidedNotice.None;
        RaiseChanged();
        try
        {
            machine.MovePrevious();
            GuidedActionResult result = await EnterCurrentAsync();
            if (result != GuidedActionResult.Done)
            {
                machine.MoveNext();
            }

            return result;
        }
        finally
        {
            _working = false;
            RaiseChanged();
        }
    }

    public async Task<GuidedActionResult> ResumeAsync()
    {
        GuidedRunMachine? machine = _machine;
        if (machine is null)
        {
            return GuidedActionResult.Unavailable;
        }

        if (IsPlaying)
        {
            return GuidedActionResult.Done;
        }

        if (IsBusy)
        {
            return GuidedActionResult.Busy;
        }

        IsPlaying = true;
        if (machine.CanMoveNext)
        {
            return await NextAsync();
        }

        if (machine.CurrentCompleted)
        {
            IsPlaying = false;
            RaiseChanged();
            return GuidedActionResult.Done;
        }

        _working = true;
        Notice = GuidedNotice.None;
        RaiseChanged();
        try
        {
            GuidedActionResult result = await EnterCurrentAsync();
            if (result != GuidedActionResult.Done)
            {
                IsPlaying = false;
            }

            return result;
        }
        finally
        {
            _working = false;
            BeginCountdown();
            RaiseChanged();
        }
    }

    public void Pause()
    {
        CancelCountdown();
        IsPlaying = false;
        RaiseChanged();
    }

    public async Task EndAsync()
    {
        if (_machine is null)
        {
            return;
        }

        CancelCountdown();
        GuidedNotice notice = GuidedNotice.None;
        if (HasBackup)
        {
            string? current = await _drafts.LoadAsync(_session.Scenario.Id);
            if (!await IsForeignTextAsync(current) && _editor.IsAvailable)
            {
                await _editor.ReplaceTextAsync(_backup!);
                await SetBackupAsync(null);
                notice = GuidedNotice.TextRestored;
            }
            else
            {
                notice = GuidedNotice.TextKept;
            }
        }

        ClearRun();
        Notice = notice;
        RaiseChanged();
    }

    public async Task<GuidedActionResult> RestoreTextAsync()
    {
        if (_backup is null || !HasBackup)
        {
            return GuidedActionResult.Unavailable;
        }

        if (!_editor.IsAvailable)
        {
            return GuidedActionResult.EditorUnavailable;
        }

        string restored = _backup;
        string? current = await _drafts.LoadAsync(_session.Scenario.Id);
        await SetBackupAsync(await IsForeignTextAsync(current) ? current : null);
        _lastInserted = null;
        await _editor.ReplaceTextAsync(restored);
        Notice = GuidedNotice.None;
        RaiseChanged();
        return GuidedActionResult.Done;
    }

    public async Task DiscardBackupAsync()
    {
        await SetBackupAsync(null);
        RaiseChanged();
    }

    public void DismissNotice()
    {
        if (Notice == GuidedNotice.None)
        {
            return;
        }

        Notice = GuidedNotice.None;
        RaiseChanged();
    }

    public void Dispose()
    {
        _session.DatabaseProvisioned -= OnDatabaseProvisioned;
        _runner.Completed -= OnRunCompleted;
        _runner.Changed -= OnRunnerChanged;
        CancelCountdown();
    }

    private async Task<GuidedActionResult> EnterCurrentAsync()
    {
        GuidedRunMachine machine = _machine!;
        GuidedStep step = machine.CurrentStep;
        ShowsEarlierRun = !machine.RunsOnEnter;
        GuidedActionResult inserted = await InsertAsync(step);
        if (inserted != GuidedActionResult.Done || ShowsEarlierRun)
        {
            return inserted;
        }

        QueryRunStatus status = await RunSqlAsync(
            step.Sql,
            step.Kind == GuidedStepKind.ChangesData
        );
        return status switch
        {
            QueryRunStatus.Completed => GuidedActionResult.Done,
            QueryRunStatus.NotReady => GuidedActionResult.NotReady,
            QueryRunStatus.AlreadyRunning => GuidedActionResult.Busy,
            _ => GuidedActionResult.Unavailable,
        };
    }

    private async Task<GuidedActionResult> InsertAsync(GuidedStep step)
    {
        if (!_editor.IsAvailable)
        {
            return GuidedActionResult.EditorUnavailable;
        }

        string scenarioId = _session.Scenario.Id;
        string? current = await _drafts.LoadAsync(scenarioId);
        if (await IsForeignTextAsync(current) && !BackupHolds(current))
        {
            bool occupied = _backup is not null && _backupScenarioId == scenarioId;
            if (occupied)
            {
                Notice = GuidedNotice.BackupOccupied;
                return GuidedActionResult.BackupOccupied;
            }

            _backupScenarioId = scenarioId;
            await SetBackupAsync(current);
        }

        _lastInserted = step.Sql;
        await _editor.ReplaceTextAsync(step.Sql);
        return GuidedActionResult.Done;
    }

    private async Task<QueryRunStatus> RunSqlAsync(string sql, bool keepChanges)
    {
        ExecutionOptions saved = _runner.Options;
        bool overridden = keepChanges && saved.RollbackAfterExecution;
        if (overridden)
        {
            _runner.Options = saved with { RollbackAfterExecution = false };
        }

        try
        {
            return await _runner.RunAsync(sql);
        }
        finally
        {
            if (overridden)
            {
                _runner.Options = saved;
            }
        }
    }

    private async Task<bool> ResetCoreAsync()
    {
        _selfReset = true;
        try
        {
            await _session.ResetDatabaseAsync();
        }
        finally
        {
            _selfReset = false;
        }

        return _session.IsReady;
    }

    private void BeginCountdown()
    {
        CancelCountdown();
        GuidedRunMachine? machine = _machine;
        if (machine is null || !IsPlaying)
        {
            return;
        }

        GuidedCheck? check = machine.CurrentCheck;
        bool onTrack = machine.CurrentCompleted && check is { IsExpected: true };
        if (!onTrack || machine.IsLast)
        {
            IsPlaying = false;
            return;
        }

        CountdownSeconds = GuidedPacing.ReadingSeconds(machine.CurrentStep);
        CancellationTokenSource countdown = new CancellationTokenSource();
        _countdown = countdown;
        _ = CountdownAsync(countdown);
    }

    private async Task CountdownAsync(CancellationTokenSource countdown)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(CountdownSeconds), _time, countdown.Token);
            if (ReferenceEquals(_countdown, countdown))
            {
                _countdown = null;
            }

            GuidedActionResult result = await NextAsync();
            if (result != GuidedActionResult.Done)
            {
                IsPlaying = false;
                Notice = NoticeFor(result);
                RaiseChanged();
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            countdown.Dispose();
        }
    }

    private void CancelCountdown()
    {
        CancellationTokenSource? countdown = _countdown;
        _countdown = null;
        countdown?.Cancel();
    }

    private bool BackupHolds(string? text)
    {
        return HasBackup && GuidedSql.Matches(_backup, text);
    }

    private async Task<bool> IsForeignTextAsync(string? current)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return false;
        }

        if (_lastInserted is not null && GuidedSql.Matches(current, _lastInserted))
        {
            return false;
        }

        GuidedRunMachine? machine = _machine;
        if (
            machine is not null
            && machine.Definition.Steps.Any(
                (GuidedStep step) => GuidedSql.Matches(current, step.Sql)
            )
        )
        {
            return false;
        }

        try
        {
            string starter = _templates.CreateStarterQuery(await _session.GetSchemaAsync());
            return !GuidedSql.Matches(current, starter);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return true;
        }
    }

    private async Task LoadBackupAsync(string scenarioId)
    {
        _backupScenarioId = scenarioId;
        _backup = await _storage.GetAsync(GuidedStorageKeys.EditorBackup(scenarioId));
    }

    private async Task SetBackupAsync(string? text)
    {
        _backup = text;
        if (text is null)
        {
            await _storage.RemoveAsync(GuidedStorageKeys.EditorBackup(_backupScenarioId));
        }
        else
        {
            await _storage.SetAsync(GuidedStorageKeys.EditorBackup(_backupScenarioId), text);
        }
    }

    private void OnRunCompleted(object? sender, QueryRun run)
    {
        GuidedRunMachine? machine = _machine;
        if (machine is null || !GuidedSql.Matches(run.Sql, machine.CurrentStep.Sql))
        {
            return;
        }

        machine.Record(run.Result);
        RaiseChanged();
    }

    private void OnRunnerChanged(object? sender, EventArgs e)
    {
        RaiseChanged();
    }

    private void OnDatabaseProvisioned(object? sender, DatabaseProvisionedEventArgs e)
    {
        _ = HandleProvisionedAsync(e);
    }

    private async Task HandleProvisionedAsync(DatabaseProvisionedEventArgs e)
    {
        string newScenarioId = e.Scenario.Id;
        string previousBackupScenario = _backupScenarioId;
        if (_machine is not null && !_selfReset)
        {
            CancelCountdown();
            if (newScenarioId != _runScenarioId)
            {
                await CloseForSwitchAsync();
                Notice = GuidedNotice.ScenarioSwitched;
            }
            else
            {
                ClearRun();
                Notice = GuidedNotice.DatabaseWasReset;
            }
        }

        if (previousBackupScenario != newScenarioId)
        {
            await LoadBackupAsync(newScenarioId);
        }

        RaiseChanged();
    }

    private async Task CloseForSwitchAsync()
    {
        if (_backup is not null)
        {
            string? draft = await _drafts.LoadAsync(_runScenarioId);
            if (!await IsForeignTextAsync(draft))
            {
                _drafts.Save(_runScenarioId, _backup);
                await _drafts.FlushAsync();
                await SetBackupAsync(null);
            }
        }

        ClearRun();
    }

    private void ClearRun()
    {
        _machine = null;
        _lastInserted = null;
        IsPlaying = false;
        ShowsEarlierRun = false;
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static GuidedNotice NoticeFor(GuidedActionResult result)
    {
        return result switch
        {
            GuidedActionResult.EditorUnavailable => GuidedNotice.EditorUnavailable,
            GuidedActionResult.NotReady => GuidedNotice.NotReady,
            GuidedActionResult.Busy => GuidedNotice.Busy,
            GuidedActionResult.BackupOccupied => GuidedNotice.BackupOccupied,
            _ => GuidedNotice.None,
        };
    }
}
