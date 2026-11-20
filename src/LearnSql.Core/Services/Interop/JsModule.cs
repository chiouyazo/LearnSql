using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Interop;

public sealed class JsModule : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly string _path;
    private Task<IJSObjectReference>? _module;

    public JsModule(IJSRuntime js, string path)
    {
        _js = js;
        _path = path;
    }

    public async ValueTask<TResult> InvokeAsync<TResult>(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] arguments
    )
    {
        IJSObjectReference module = await LoadAsync();
        return await module.InvokeAsync<TResult>(identifier, cancellationToken, arguments);
    }

    public async ValueTask InvokeVoidAsync(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] arguments
    )
    {
        IJSObjectReference module = await LoadAsync();
        await module.InvokeVoidAsync(identifier, cancellationToken, arguments);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            IJSObjectReference module = await _module;
            await module.DisposeAsync();
        }
        catch (Exception exception) when (exception is JSException or JSDisconnectedException)
        {
            return;
        }
    }

    private Task<IJSObjectReference> LoadAsync()
    {
        _module ??= _js.InvokeAsync<IJSObjectReference>("import", _path).AsTask();
        return _module;
    }
}
