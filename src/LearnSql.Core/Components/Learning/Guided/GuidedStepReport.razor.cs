using LearnSql.Core.Services.Guided;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning.Guided;

public partial class GuidedStepReport
{
    [Parameter, EditorRequired]
    public GuidedStep Step { get; set; } = default!;

    [Parameter]
    public GuidedCheck? Check { get; set; }

    [Inject]
    private IStringLocalizer<GuidedStrings> L { get; set; } = default!;

    private string IdOf(string part)
    {
        return "guided-report-" + Step.Id + "-" + part;
    }

    private string CheckText(GuidedCheck check)
    {
        return check.Kind switch
        {
            GuidedCheckKind.RowCountDiffers => L[
                "CheckRowCount",
                check.Actual ?? 0,
                check.Expected ?? 0
            ],
            GuidedCheckKind.RowsAffectedDiffers => L[
                "CheckRowsAffected",
                check.Actual ?? 0,
                check.Expected ?? 0
            ],
            GuidedCheckKind.ExpectedErrorMissing => L["CheckErrorMissing"],
            GuidedCheckKind.UnexpectedError => L[
                "CheckUnexpectedError",
                check.ErrorText ?? string.Empty
            ],
            GuidedCheckKind.RolledBack => L["CheckRolledBack"],
            _ => L["CheckInterrupted"],
        };
    }
}
