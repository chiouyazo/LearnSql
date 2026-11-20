using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LearnSql.Core.Components.Results;

public partial class ResultPanel : IDisposable
{
    private const int MaxVisibleResultSets = 20;

    private int _hiddenResultSets;
    private QueryRun? _run;
    private IReadOnlyList<ResultSet> _resultSets = [];
    private int _activeTab;
    private bool _wasRunning;
    private string _announcement = string.Empty;

    [Parameter]
    public EventCallback OnRunRequested { get; set; }

    [Parameter]
    public EventCallback<EditorRange> OnNavigate { get; set; }

    [Inject]
    private IQueryRunner Runner { get; set; } = default!;

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ResultStrings> L { get; set; } = default!;

    private string Summary
    {
        get
        {
            if (_run is null)
            {
                return string.Empty;
            }

            ExecutionResult result = _run.Result;
            return result.Error?.Kind switch
            {
                null when _resultSets.Count == 0 => L["SummaryNoResultSet"],
                null => L[
                    "SummarySuccess",
                    _resultSets.Count,
                    _resultSets.Sum((ResultSet set) => set.Rows.Count)
                ],
                ExecutionErrorKind.Cancelled => L["SummaryCancelled"],
                ExecutionErrorKind.Timeout => L["SummaryTimeout"],
                _ => L["SummaryFailed"],
            };
        }
    }

    protected override void OnInitialized()
    {
        Runner.Changed += OnRunnerChanged;
        Session.StateChanged += OnSessionChanged;
        _wasRunning = Runner.IsRunning;
        Bind(Runner.LastRun);
    }

    public void Dispose()
    {
        Runner.Changed -= OnRunnerChanged;
        Session.StateChanged -= OnSessionChanged;
    }

    private Task PrimaryActionAsync()
    {
        if (Runner.IsRunning)
        {
            Runner.Cancel();
            return Task.CompletedTask;
        }

        return OnRunRequested.InvokeAsync();
    }

    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && Runner.IsRunning)
        {
            Runner.Cancel();
        }
        else if (args.Key == "Enter" && (args.CtrlKey || args.MetaKey) && !Runner.IsRunning)
        {
            _ = OnRunRequested.InvokeAsync();
        }
    }

    private void OnRollbackChanged(bool rollback)
    {
        Runner.Options = Runner.Options with { RollbackAfterExecution = rollback };
    }

    private void OnRunnerChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            if (!ReferenceEquals(_run, Runner.LastRun))
            {
                Bind(Runner.LastRun);
            }

            UpdateAnnouncement();
            _wasRunning = Runner.IsRunning;
            StateHasChanged();
        });
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void Bind(QueryRun? run)
    {
        _run = run;
        IReadOnlyList<ResultSet> all = run?.Result.ResultSets ?? [];
        _resultSets = [.. all.Take(MaxVisibleResultSets)];
        _hiddenResultSets = all.Count - _resultSets.Count;
        _activeTab = 0;
    }

    private void UpdateAnnouncement()
    {
        if (Runner.IsRunning && !_wasRunning)
        {
            _announcement = L["AnnounceRunning"];
        }
        else if (!Runner.IsRunning && _wasRunning && _run is not null)
        {
            _announcement = Summary;
        }
    }
}
