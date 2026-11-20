using LearnSql.Core.Services.Guided;
using LearnSql.Core.Services.Interop;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning.Guided;

public partial class GuidedPlayer : IDisposable
{
    private ElementReference _root;
    private int _shownIndex = -1;

    [Inject]
    private IDocumentService Document { get; set; } = default!;

    [Inject]
    private GuidedCoordinator Coordinator { get; set; } = default!;

    [Inject]
    private GuidedPrompts Prompts { get; set; } = default!;

    [Inject]
    private IStringLocalizer<GuidedStrings> L { get; set; } = default!;

    private GuidedRunMachine? Machine => Coordinator.Machine;

    private bool ResumeDisabled =>
        Coordinator.IsBusy || Machine is null || (Machine.IsLast && Machine.CurrentCompleted);

    protected override void OnInitialized()
    {
        Coordinator.Changed += OnChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        int index = Machine?.CurrentIndex ?? -1;
        if (index != _shownIndex)
        {
            _shownIndex = index;
            if (index >= 0)
            {
                await Document.ScrollToTopAsync(_root);
            }
        }
    }

    public void Dispose()
    {
        Coordinator.Changed -= OnChanged;
        Coordinator.Pause();
    }

    private void OnChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task NextAsync()
    {
        Prompts.Report(await Coordinator.NextAsync());
    }

    private async Task PreviousAsync()
    {
        Prompts.Report(await Coordinator.PreviousAsync());
    }

    private async Task ResumeAsync()
    {
        Prompts.Report(await Coordinator.ResumeAsync());
    }

    private async Task RestartAsync()
    {
        GuidedRunMachine? machine = Machine;
        if (machine is null)
        {
            return;
        }

        GuidedScenarioDefinition definition = machine.Definition;
        if (definition.RequiresFreshDatabase && !await Prompts.ConfirmResetAsync())
        {
            return;
        }

        await Coordinator.EndAsync();
        Prompts.Report(await Coordinator.StartAsync(definition));
    }

    private Task EndAsync()
    {
        return Coordinator.EndAsync();
    }
}
