using LearnSql.Core.Components.Editor;
using LearnSql.Core.Services.Drafts;
using LearnSql.Core.Services.LearningPanel;
using LearnSql.Core.Services.Templates;
using LearnSql.Core.Services.Workspace;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Workspace;

public partial class SqlWorkspace : IDisposable
{
    private SqlEditor? _editor;
    private string _text = string.Empty;
    private string _scenarioId = string.Empty;
    private bool _schemaOpen = true;
    private bool _initialized;
    private bool _attached;
    private bool _starterPending;
    private int _editVersion;
    private int _runStartVersion;
    private bool _runObserved;

    [Parameter]
    public bool ShowDatasetMenu { get; set; }

    [Parameter]
    public RenderFragment? ToolbarLeading { get; set; }

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IQueryRunner Runner { get; set; } = default!;

    [Inject]
    private IDraftStore Drafts { get; set; } = default!;

    [Inject]
    private WorkspaceLayoutPreferences Layout { get; set; } = default!;

    [Inject]
    private WorkspaceEditorBridge EditorBridge { get; set; } = default!;

    [Inject]
    private IQueryTemplateFactory Templates { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IStringLocalizer<WorkspaceStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<SqlWorkspace> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _scenarioId = Session.Scenario.Id;
        Session.DatabaseProvisioned += OnDatabaseProvisioned;
        Runner.Changed += OnRunnerChanged;
        Runner.Completed += OnRunCompleted;

        await Layout.EnsureLoadedAsync();
        _schemaOpen = Layout.SchemaOpen;

        string? draft = await Drafts.LoadAsync(_scenarioId);
        if (draft is not null)
        {
            _text = draft;
        }
        else if (Session.IsReady)
        {
            _text = await CreateStarterAsync();
        }
        else
        {
            _starterPending = true;
        }

        _initialized = true;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (_initialized && !_attached && _editor is not null)
        {
            _attached = true;
            EditorBridge.Attach(_editor);
        }
    }

    public void Dispose()
    {
        Session.DatabaseProvisioned -= OnDatabaseProvisioned;
        Runner.Changed -= OnRunnerChanged;
        Runner.Completed -= OnRunCompleted;
        if (_editor is not null)
        {
            EditorBridge.Detach(_editor);
        }
    }

    private async Task ToggleSchemaAsync()
    {
        _schemaOpen = !_schemaOpen;
        await Layout.SetSchemaOpenAsync(_schemaOpen);
    }

    private Task InsertTextAsync(string text)
    {
        return _editor is null ? Task.CompletedTask : _editor.InsertAtCursorAsync(text);
    }

    private void OnTextChanged(string text)
    {
        _text = text;
        _editVersion++;
        Drafts.Save(_scenarioId, text);
    }

    private async Task OnEditorReadyAsync()
    {
        QueryRun? run = Runner.LastRun;
        bool matches = run is not null && run.Origin == SourceOrigin.Start && run.Sql == _text;
        if (matches)
        {
            await ApplyMarkersAsync(run!);
        }
    }

    private async Task RunAsync()
    {
        ExecutableText executable = _editor is { IsReady: true }
            ? await _editor.GetExecutableTextAsync()
            : new ExecutableText(_text, SourceOrigin.Start, false);
        QueryRunStatus status = await Runner.RunAsync(executable.Text, executable.Origin);
        switch (status)
        {
            case QueryRunStatus.AlreadyRunning:
                Snackbar.Add(L["AlreadyRunning"], Severity.Warning);
                break;
            case QueryRunStatus.NotReady:
                Snackbar.Add(L["NotReady"], Severity.Warning);
                break;
            case QueryRunStatus.EmptyInput:
                Snackbar.Add(L["EmptyInput"], Severity.Info);
                break;
        }
    }

    private void OnRunnerChanged(object? sender, EventArgs e)
    {
        if (Runner.IsRunning && !_runObserved)
        {
            _runStartVersion = _editVersion;
        }

        _runObserved = Runner.IsRunning;
    }

    private void OnRunCompleted(object? sender, QueryRun run)
    {
        if (_runStartVersion == _editVersion)
        {
            _ = InvokeAsync(() => ApplyMarkersAsync(run));
        }
    }

    private async Task ApplyMarkersAsync(QueryRun run)
    {
        if (_editor is null)
        {
            return;
        }

        List<EditorMarker> markers =
        [
            .. DiagnosticMarkerMapper.ToMarkers(run.Result.Diagnostics, run.Origin),
        ];
        ExecutionError? error = run.Result.Error;
        if (error is { Kind: ExecutionErrorKind.Sql, Origin: SourceSpan span })
        {
            markers.Add(
                new EditorMarker(
                    run.Origin.ToEditorRange(span),
                    error.Display,
                    EditorMarkerSeverity.Error
                )
            );
        }

        await _editor.SetMarkersAsync(markers);
    }

    private async Task NavigateToAsync(EditorRange range)
    {
        if (_editor is null)
        {
            return;
        }

        if (Runner.LastRun is QueryRun run)
        {
            await ApplyMarkersAsync(run);
        }

        await _editor.RevealRangeAsync(range);
    }

    private Task ApplyTemplateAsync(string sql)
    {
        return ReplaceTextAsync(sql);
    }

    private Task ClearAsync()
    {
        return ReplaceTextAsync(string.Empty);
    }

    private async Task ReplaceTextAsync(string text)
    {
        if (_editor is null)
        {
            return;
        }

        await _editor.ReplaceTextAsync(text);
        await _editor.FocusAsync();
    }

    private async Task<string> CreateStarterAsync()
    {
        try
        {
            return Templates.CreateStarterQuery(await Session.GetSchemaAsync());
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogWarning(exception, "Creating the starter query failed.");
            return string.Empty;
        }
    }

    private void OnDatabaseProvisioned(object? sender, DatabaseProvisionedEventArgs e)
    {
        bool scenarioChanged = e.Scenario.Id != _scenarioId;
        if (scenarioChanged || _starterPending)
        {
            _ = InvokeAsync(() => ApplyScenarioTextAsync(e.Scenario.Id));
        }
    }

    private async Task ApplyScenarioTextAsync(string scenarioId)
    {
        _scenarioId = scenarioId;
        _starterPending = false;
        string text = await Drafts.LoadAsync(scenarioId) ?? await CreateStarterAsync();
        _text = text;
        if (_editor is not null)
        {
            await _editor.SetTextAsync(text);
        }

        StateHasChanged();
    }
}
