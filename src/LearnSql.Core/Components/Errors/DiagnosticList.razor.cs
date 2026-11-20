using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Errors;

public partial class DiagnosticList
{
    [Parameter, EditorRequired]
    public IReadOnlyList<DialectDiagnostic> Diagnostics { get; set; } = [];

    [Parameter]
    public SourceOrigin Origin { get; set; } = SourceOrigin.Start;

    [Parameter]
    public EventCallback<EditorRange> OnNavigate { get; set; }

    [Inject]
    private IStringLocalizer<ResultStrings> L { get; set; } = default!;

    private Task NavigateAsync(DialectDiagnostic diagnostic)
    {
        return OnNavigate.InvokeAsync(Origin.ToEditorRange(diagnostic.Span));
    }

    private static string IconFor(DialectDiagnostic diagnostic)
    {
        return diagnostic.Severity switch
        {
            DiagnosticSeverity.Error => Icons.Material.Filled.Error,
            DiagnosticSeverity.Warning => Icons.Material.Filled.Warning,
            _ => Icons.Material.Filled.Info,
        };
    }

    private static Color ColorFor(DialectDiagnostic diagnostic)
    {
        return diagnostic.Severity switch
        {
            DiagnosticSeverity.Error => Color.Error,
            DiagnosticSeverity.Warning => Color.Warning,
            _ => Color.Info,
        };
    }
}
