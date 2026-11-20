using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace LearnSql.Core.Components.Shell;

public partial class PageErrorBoundary
{
    [Inject]
    private ILogger<PageErrorBoundary> Logger { get; set; } = default!;

    [Inject]
    private IStringLocalizer<AppStrings> L { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(exception, "A page raised an unhandled exception.");
        return Task.CompletedTask;
    }
}
