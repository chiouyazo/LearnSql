using LearnSql.Core.Services.Templates;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Editor;

public partial class TemplateMenu : IDisposable
{
    private IReadOnlyList<QueryTemplate> _templates = [];
    private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

    [Parameter]
    public EventCallback<string> OnTemplateSelected { get; set; }

    [Inject]
    private IAppSession Session { get; set; } = default!;

    [Inject]
    private IQueryTemplateFactory Factory { get; set; } = default!;

    [Inject]
    private IStringLocalizer<EditorStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<TemplateMenu> Logger { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Session.SchemaChanged += OnSchemaChanged;
        await RefreshAsync();
    }

    public void Dispose()
    {
        Session.SchemaChanged -= OnSchemaChanged;
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private void OnSchemaChanged(object? sender, EventArgs e)
    {
        _ = InvokeAsync(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        if (!Session.IsReady)
        {
            _templates = Factory.Create(DatabaseSchema.Empty);
            return;
        }

        try
        {
            DatabaseSchema schema = await Session.GetSchemaAsync(_lifetime.Token);
            _templates = Factory.Create(schema);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Reading the schema for the templates failed.");
            _templates = Factory.Create(DatabaseSchema.Empty);
        }

        StateHasChanged();
    }
}
