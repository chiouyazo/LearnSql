using LearnSql.Core.Services.Interop;
using LearnSql.Core.Services.LearningPanel;
using LearnSql.Core.Services.Navigation;
using LearnSql.Core.Services.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace LearnSql.Core.Pages;

public partial class ScenarioWorkspace : IAsyncDisposable
{
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

    private ScenarioDefinition? _scenario;
    private IAsyncDisposable? _splitSubscription;
    private bool _panelOpen = true;
    private bool _initialized;
    private bool _observing;

    [Parameter]
    public string ScenarioId { get; set; } = string.Empty;

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IScenarioTextProvider Texts { get; set; } = default!;

    [Inject]
    private WorkspaceLayoutPreferences Layout { get; set; } = default!;

    [Inject]
    private ISplitPanelObserver SplitObserver { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ScenarioStrings> L { get; set; } = default!;

    [Inject]
    private IStringLocalizer<AppStrings> LApp { get; set; } = default!;

    [Inject]
    private ILogger<ScenarioWorkspace> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Layout.EnsureLoadedAsync();
        _panelOpen = Layout.PanelOpen;
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        _scenario = ScenarioRoutes.Resolve(Session.Scenarios, ScenarioId);
        if (_scenario is not null && _scenario.Id != Session.Scenario.Id)
        {
            await Session.SwitchScenarioAsync(_scenario.Id);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_initialized || _scenario is null || _observing)
        {
            return;
        }

        _observing = true;
        try
        {
            IAsyncDisposable subscription = await SplitObserver.ObserveAsync(
                "scenario-split",
                Layout.PanelWidth,
                Layout.SetPanelWidthAsync,
                _lifetime.Token
            );
            if (_lifetime.IsCancellationRequested)
            {
                await subscription.DisposeAsync();
            }
            else
            {
                _splitSubscription = subscription;
            }
        }
        catch (Exception exception) when (exception is JSException or OperationCanceledException)
        {
            Logger.LogWarning(exception, "Observing the learning panel width failed.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _lifetime.CancelAsync();
        _lifetime.Dispose();
        if (_splitSubscription is not null)
        {
            await _splitSubscription.DisposeAsync();
        }
    }

    private async Task TogglePanelAsync()
    {
        _panelOpen = !_panelOpen;
        await Layout.SetPanelOpenAsync(_panelOpen);
    }
}
