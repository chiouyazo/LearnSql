using LearnSql.Core.Services.Exercises;
using LearnSql.Core.Services.Scenarios;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning.Exercises;

public partial class ExercisesTab : IDisposable
{
    private ElementReference _heading;
    private ExerciseProgressBook _book = new ExerciseProgressBook();
    private IReadOnlyList<ExercisePath> _paths = [];
    private IReadOnlyList<ExerciseDefinition> _ordered = [];
    private string _scenarioId = string.Empty;
    private string? _selectedId;
    private bool _loaded;
    private bool _focusHeading;
    private int _solved;

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IScenarioTextProvider Texts { get; set; } = default!;

    [Inject]
    private IExerciseProgressService Progress { get; set; } = default!;

    [Inject]
    private IDialogService Dialogs { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ExerciseStrings> L { get; set; } = default!;

    private ExerciseDefinition? SelectedExercise =>
        _selectedId is null
            ? null
            : _ordered.FirstOrDefault((ExerciseDefinition e) => e.Id == _selectedId);

    protected override async Task OnInitializedAsync()
    {
        Session.DatabaseProvisioned += OnDatabaseProvisioned;
        Progress.Changed += OnProgressChanged;
        await LoadAsync(Session.Scenario);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_focusHeading && _selectedId is null)
        {
            _focusHeading = false;
            await _heading.FocusAsync();
        }
    }

    public void Dispose()
    {
        Session.DatabaseProvisioned -= OnDatabaseProvisioned;
        Progress.Changed -= OnProgressChanged;
    }

    private async Task LoadAsync(ScenarioDefinition scenario)
    {
        ExerciseProgressBook book = await Progress.GetBookAsync(scenario);
        if (scenario.Id != Session.Scenario.Id)
        {
            return;
        }

        if (scenario.Id != _scenarioId)
        {
            _selectedId = null;
        }

        _scenarioId = scenario.Id;
        _paths = ExercisePaths.Build(Texts.Localize(scenario).Exercises);
        _ordered = ExercisePaths.Flatten(_paths);
        _book = book;
        _solved = _book.SolvedCount(_ordered);
        _loaded = true;
    }

    private void OnDatabaseProvisioned(object? sender, DatabaseProvisionedEventArgs e)
    {
        if (e.Scenario.Id != _scenarioId)
        {
            _ = InvokeAsync(async () =>
            {
                await LoadAsync(e.Scenario);
                StateHasChanged();
            });
        }
    }

    private void OnProgressChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            _solved = _book.SolvedCount(_ordered);
            StateHasChanged();
        });
    }

    private void Open(string exerciseId)
    {
        _selectedId = exerciseId;
    }

    private void OpenNext()
    {
        int index = SelectedIndex;
        if (index >= 0 && index + 1 < _ordered.Count)
        {
            _selectedId = _ordered[index + 1].Id;
        }
    }

    private int SelectedIndex
    {
        get
        {
            for (int index = 0; index < _ordered.Count; index++)
            {
                if (_ordered[index].Id == _selectedId)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    private bool HasNext(ExerciseDefinition exercise)
    {
        return _ordered.Count > 0 && _ordered[^1].Id != exercise.Id;
    }

    private void Back()
    {
        _selectedId = null;
        _focusHeading = true;
    }

    private async Task ConfirmResetAsync()
    {
        bool? confirmed = await Dialogs.ShowMessageBoxAsync(
            L["ResetDialogTitle"],
            L["ResetDialogMessage"],
            yesText: L["ResetDialogConfirm"],
            cancelText: L["Cancel"]
        );
        if (confirmed == true)
        {
            await Progress.ResetAsync(Session.Scenario);
            Snackbar.Add(L["ResetDone"], Severity.Info);
        }
    }
}
