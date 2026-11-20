using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Errors;

public partial class ErrorPanel
{
    [Parameter, EditorRequired]
    public ExecutionError Error { get; set; } = default!;

    [Parameter]
    public IReadOnlyList<DialectDiagnostic> Diagnostics { get; set; } = [];

    [Parameter]
    public SourceOrigin Origin { get; set; } = SourceOrigin.Start;

    [Parameter]
    public EventCallback<EditorRange> OnNavigate { get; set; }

    [Inject]
    private IStringLocalizer<ResultStrings> L { get; set; } = default!;

    private bool HasDetail =>
        !string.IsNullOrWhiteSpace(Error.Detail)
        && !string.Equals(Error.Detail, Error.Message, StringComparison.Ordinal);

    private Task NavigateToOriginAsync()
    {
        return Error.Origin is SourceSpan span
            ? OnNavigate.InvokeAsync(Origin.ToEditorRange(span))
            : Task.CompletedTask;
    }
}
