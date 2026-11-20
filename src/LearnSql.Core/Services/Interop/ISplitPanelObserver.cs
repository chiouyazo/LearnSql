namespace LearnSql.Core.Services.Interop;

public interface ISplitPanelObserver
{
    Task<IAsyncDisposable> ObserveAsync(
        string wrapperId,
        int? restoreWidth,
        Func<int, Task> onWidthChanged,
        CancellationToken cancellationToken = default
    );
}
