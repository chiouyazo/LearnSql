using LearnSql.Core.Services.Guided;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning.Guided;

public partial class GuidedExampleList
{
    [Parameter, EditorRequired]
    public IReadOnlyList<GuidedScenarioDefinition> Definitions { get; set; } = [];

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public EventCallback<GuidedScenarioDefinition> OnStart { get; set; }

    [Inject]
    private IStringLocalizer<GuidedStrings> L { get; set; } = default!;
}
