using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Interop;

public sealed class SplitPanelObserver : ISplitPanelObserver, IAsyncDisposable
{
    private readonly JsModule _module;

    public SplitPanelObserver(IJSRuntime js)
    {
        _module = new JsModule(js, "./js/split-panel.js");
    }

    public async Task<IAsyncDisposable> ObserveAsync(
        string wrapperId,
        int? restoreWidth,
        Func<int, Task> onWidthChanged,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(onWidthChanged);

        DotNetObjectReference<WidthCallback> reference = DotNetObjectReference.Create(
            new WidthCallback(onWidthChanged)
        );
        try
        {
            IJSObjectReference handle = await _module.InvokeAsync<IJSObjectReference>(
                "observe",
                cancellationToken,
                wrapperId,
                restoreWidth,
                reference
            );
            return new Subscription(handle, reference);
        }
        catch
        {
            reference.Dispose();
            throw;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _module.DisposeAsync();
    }

    private sealed class WidthCallback
    {
        private readonly Func<int, Task> _handler;

        public WidthCallback(Func<int, Task> handler)
        {
            _handler = handler;
        }

        [JSInvokable]
        public Task OnWidthChanged(int width)
        {
            return _handler(width);
        }
    }

    private sealed class Subscription : IAsyncDisposable
    {
        private readonly IJSObjectReference _handle;
        private readonly DotNetObjectReference<WidthCallback> _reference;

        public Subscription(
            IJSObjectReference handle,
            DotNetObjectReference<WidthCallback> reference
        )
        {
            _handle = handle;
            _reference = reference;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _handle.InvokeVoidAsync("dispose");
                await _handle.DisposeAsync();
            }
            catch (Exception exception) when (exception is JSException or JSDisconnectedException)
            {
                return;
            }
            finally
            {
                _reference.Dispose();
            }
        }
    }
}
