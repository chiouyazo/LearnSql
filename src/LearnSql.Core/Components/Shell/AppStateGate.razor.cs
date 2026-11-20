using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Shell;

public partial class AppStateGate
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    private IAppSession Session { get; set; } = default!;

    private bool IsStartup =>
        Session.Phase == AppPhase.NotStarted || Session.Operation == AppOperation.Startup;

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
