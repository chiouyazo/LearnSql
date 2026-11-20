using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Shell;

public partial class AppFailurePanel
{
    [Parameter, EditorRequired]
    public AppFailure Failure { get; set; } = default!;

    [Parameter]
    public EventCallback OnRetry { get; set; }

    [Inject]
    private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private string Title =>
        Failure.Kind switch
        {
            AppFailureKind.EngineInitialization => L["FailureEngineTitle"],
            AppFailureKind.UnknownScenario => L["FailureUnknownScenarioTitle"],
            AppFailureKind.Cancelled => L["FailureCancelledTitle"],
            _ => L["FailureScenarioTitle"],
        };

    private string Explanation =>
        Failure.Kind switch
        {
            AppFailureKind.EngineInitialization => L["FailureEngineExplanation"],
            AppFailureKind.UnknownScenario => L["FailureUnknownScenarioExplanation"],
            AppFailureKind.Cancelled => L["FailureCancelledExplanation"],
            _ => L["FailureScenarioExplanation"],
        };
}
