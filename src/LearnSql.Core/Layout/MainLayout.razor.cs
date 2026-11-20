using LearnSql.Core.Components.Shell;
using LearnSql.Core.Services.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Layout;

public partial class MainLayout : IDisposable
{
    private readonly MudTheme _theme = AppTheme.Create();

    private bool _drawerOpen = true;
    private ElementReference _main;
    private PageErrorBoundary? _boundary;

    [Inject]
    private ThemeState Theme { get; set; } = default!;

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IStringLocalizer<AppStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<MainLayout> Logger { get; set; } = default!;

    protected override void OnInitialized()
    {
        Theme.Changed += OnThemeChanged;
        Navigation.LocationChanged += OnLocationChanged;
        if (
            ScenarioRoutes.TryGetScenarioId(
                Navigation.ToBaseRelativePath(Navigation.Uri),
                out string scenarioId
            )
        )
        {
            Session.SelectInitialScenario(scenarioId);
        }

        _ = StartSessionAsync();
    }

    public void Dispose()
    {
        Theme.Changed -= OnThemeChanged;
        Navigation.LocationChanged -= OnLocationChanged;
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private async Task FocusMainAsync()
    {
        await _main.FocusAsync();
    }

    private async Task StartSessionAsync()
    {
        try
        {
            await Session.EnsureStartedAsync();
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Starting the application session failed.");
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _boundary?.Recover();
    }
}
