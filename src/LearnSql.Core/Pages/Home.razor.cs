using LearnSql.Core.Services.Navigation;
using LearnSql.Core.Services.Scenarios;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Pages;

public partial class Home : IDisposable
{
    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IScenarioTextProvider Texts { get; set; } = default!;

    [Inject]
    private IStringLocalizer<HomeStrings> L { get; set; } = default!;

    [Inject]
    private IStringLocalizer<AppStrings> LApp { get; set; } = default!;

    protected override void OnInitialized()
    {
        Session.StateChanged += OnStateChanged;
    }

    public void Dispose()
    {
        Session.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }
}
