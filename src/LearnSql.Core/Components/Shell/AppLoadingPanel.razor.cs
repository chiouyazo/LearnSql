using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Shell;

public partial class AppLoadingPanel
{
    [Parameter]
    public AppOperation Operation { get; set; }

    [Parameter]
    public SandboxProgress? Progress { get; set; }

    [Inject]
    private IStringLocalizer<AppStrings> L { get; set; } = default!;

    private double Percent => (Progress?.Fraction ?? 0.0) * 100.0;

    private string Title =>
        Operation switch
        {
            AppOperation.SwitchScenario => L["LoadingSwitchScenario"],
            AppOperation.ResetDatabase => L["LoadingReset"],
            _ => L["LoadingStartup"],
        };

    private string PhaseText =>
        Progress?.Phase switch
        {
            SandboxPhase.PreparingEngine => L["PhasePreparingEngine"],
            SandboxPhase.ResettingDatabase => L["PhaseResettingDatabase"],
            SandboxPhase.CreatingSchema => L["PhaseCreatingSchema"],
            SandboxPhase.CreatingViews => L["PhaseCreatingViews"],
            SandboxPhase.SeedingData => L["PhaseSeedingData"],
            SandboxPhase.ReadingSchema => L["PhaseReadingSchema"],
            SandboxPhase.Ready => L["PhaseReady"],
            _ => L["PhaseStarting"],
        };
}
