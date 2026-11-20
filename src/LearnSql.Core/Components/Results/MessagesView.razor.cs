using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Results;

public partial class MessagesView
{
    [Parameter, EditorRequired]
    public ExecutionResult Result { get; set; } = default!;

    [Parameter, EditorRequired]
    public ExecutionOptions Options { get; set; } = default!;

    [Parameter]
    public int HiddenResultSets { get; set; }

    [Parameter]
    public SourceOrigin Origin { get; set; } = SourceOrigin.Start;

    [Parameter]
    public EventCallback<EditorRange> OnNavigate { get; set; }

    [Inject]
    private IStringLocalizer<ResultStrings> L { get; set; } = default!;

    private IReadOnlyList<string> Messages => Result.Messages;

    private IReadOnlyList<DialectDiagnostic> Hints =>
        [
            .. Result.Diagnostics.Where(
                (DialectDiagnostic diagnostic) => diagnostic.Severity != DiagnosticSeverity.Error
            ),
        ];
}
