using LearnSql.Core.Services.Guided;
using LearnSql.Core.Services.Scenarios;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Learning;

public partial class ScenarioInfoTab : IDisposable
{
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

    private int? _tables;
    private int? _views;
    private bool _schemaFailed;

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private GuidedCoordinator Coordinator { get; set; } = default!;

    [Inject]
    private GuidedPrompts Prompts { get; set; } = default!;

    [Inject]
    private IScenarioTextProvider Texts { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ScenarioStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<ScenarioInfoTab> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Session.StateChanged += OnStateChanged;
        Session.SchemaChanged += OnSchemaChanged;
        Coordinator.Changed += OnStateChanged;
        await Coordinator.InitializeAsync();
        if (Session.IsReady)
        {
            await RefreshAsync();
        }
    }

    public void Dispose()
    {
        Session.StateChanged -= OnStateChanged;
        Session.SchemaChanged -= OnSchemaChanged;
        Coordinator.Changed -= OnStateChanged;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task StartAsync(GuidedScenarioDefinition definition)
    {
        if (definition.RequiresFreshDatabase && !await Prompts.ConfirmResetAsync())
        {
            return;
        }

        Prompts.Report(await Coordinator.StartAsync(definition));
    }

    private void OnSchemaChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        try
        {
            DatabaseSchema schema = await Session.GetSchemaAsync(_lifetime.Token);
            _tables = CountVisible(schema.Tables);
            _views = CountVisible(schema.Views);
            _schemaFailed = false;
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Reading the schema summary failed.");
            _schemaFailed = true;
        }

        StateHasChanged();
    }

    private static int CountVisible(IEnumerable<RelationSchema> relations)
    {
        return relations.Count(
            (RelationSchema relation) => !relation.IsSystem && !relation.IsTemporary
        );
    }
}
