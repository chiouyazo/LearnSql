using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Pages;

public partial class Playground
{
    [Inject]
    private IStringLocalizer<WorkspaceStrings> L { get; set; } = default!;

    [Inject]
    private IStringLocalizer<AppStrings> LApp { get; set; } = default!;
}
