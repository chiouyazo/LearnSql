using LearnSql.Core.Services.Exercises;
using LearnSql.Core.Services.Templates;
using LearnSql.Core.Services.Workspace;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Learning.Exercises;

public partial class ExerciseDetailView : IDisposable
{
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

    private ElementReference _heading;
    private ExerciseCheckResult? _result;
    private bool _checking;
    private bool _solutionVisible;

    [Parameter, EditorRequired]
    public ScenarioDefinition Scenario { get; set; } = default!;

    [Parameter, EditorRequired]
    public ExerciseDefinition Exercise { get; set; } = default!;

    [Parameter, EditorRequired]
    public ExerciseProgress Progress { get; set; } = default!;

    [Parameter]
    public bool HasNext { get; set; }

    [Parameter]
    public EventCallback OnBack { get; set; }

    [Parameter]
    public EventCallback OnNext { get; set; }

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IQueryRunner Runner { get; set; } = default!;

    [Inject]
    private IExerciseChecker Checker { get; set; } = default!;

    [Inject]
    private IExerciseProgressService ProgressService { get; set; } = default!;

    [Inject]
    private IWorkspaceEditor Editor { get; set; } = default!;

    [Inject]
    private IWorkspaceEditorText EditorText { get; set; } = default!;

    [Inject]
    private IQueryTemplateFactory Templates { get; set; } = default!;

    [Inject]
    private IDialogService Dialogs { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ExerciseStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<ExerciseDetailView> Logger { get; set; } = default!;

    private bool IsBusy => _checking || Runner.IsRunning;

    protected override void OnInitialized()
    {
        Runner.Changed += OnRunnerChanged;
        Session.StateChanged += OnRunnerChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _heading.FocusAsync();
        }
    }

    public void Dispose()
    {
        Runner.Changed -= OnRunnerChanged;
        Session.StateChanged -= OnRunnerChanged;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private void OnRunnerChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task CheckAsync()
    {
        string? text = await EditorText.GetTextAsync();
        if (text is null)
        {
            Snackbar.Add(L["EditorUnavailable"], Severity.Warning);
            return;
        }

        _checking = true;
        _result = null;
        StateHasChanged();
        ExerciseCheckResult result;
        try
        {
            result = await Checker.CheckAsync(Exercise, text, _lifetime.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        finally
        {
            _checking = false;
        }

        _result = result;
        if (result.CountsAsAttempt)
        {
            await ProgressService.RecordAttemptAsync(
                Scenario,
                Exercise.Id,
                result.Outcome == ExerciseCheckOutcome.Correct,
                text
            );
        }
    }

    private async Task LoadIntoEditorAsync()
    {
        string? draft = Progress.LastSql;
        if (await PlaceInEditorAsync(draft ?? string.Empty))
        {
            Snackbar.Add(draft is null ? L["LoadedEmpty"] : L["LoadedDraft"], Severity.Info);
        }
    }

    private async Task UseSolutionAsync()
    {
        if (await PlaceInEditorAsync(Exercise.SolutionSql))
        {
            Snackbar.Add(L["SolutionApplied"], Severity.Info);
        }
    }

    private async Task<bool> PlaceInEditorAsync(string text)
    {
        if (!Editor.IsAvailable)
        {
            Snackbar.Add(L["EditorUnavailable"], Severity.Warning);
            return false;
        }

        string? current = await EditorText.GetTextAsync();
        if (!await CanReplaceSilentlyAsync(current, text))
        {
            bool? confirmed = await Dialogs.ShowMessageBoxAsync(
                L["ReplaceDialogTitle"],
                L["ReplaceDialogMessage"],
                yesText: L["ReplaceDialogConfirm"],
                cancelText: L["Cancel"]
            );
            if (confirmed != true)
            {
                return false;
            }
        }

        await Editor.ReplaceTextAsync(text);
        return true;
    }

    private async Task<bool> CanReplaceSilentlyAsync(string? current, string replacement)
    {
        if (string.IsNullOrWhiteSpace(current))
        {
            return true;
        }

        string normalized = Normalize(current);
        if (normalized == Normalize(replacement))
        {
            return true;
        }

        try
        {
            string starter = Templates.CreateStarterQuery(await Session.GetSchemaAsync());
            return normalized == Normalize(starter);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Logger.LogWarning(exception, "Reading the starter query failed.");
            return false;
        }
    }

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
    }

    private async Task RevealHintAsync()
    {
        await ProgressService.RevealNextHintAsync(Scenario, Exercise);
    }

    private async Task ShowSolutionAsync()
    {
        if (!Progress.SolutionViewed)
        {
            bool? confirmed = await Dialogs.ShowMessageBoxAsync(
                L["SolutionDialogTitle"],
                L["SolutionDialogMessage"],
                yesText: L["SolutionDialogConfirm"],
                cancelText: L["Cancel"]
            );
            if (confirmed != true)
            {
                return;
            }
        }

        _solutionVisible = true;
        await ProgressService.MarkSolutionViewedAsync(Scenario, Exercise.Id);
    }

    private void HideSolution()
    {
        _solutionVisible = false;
    }
}
