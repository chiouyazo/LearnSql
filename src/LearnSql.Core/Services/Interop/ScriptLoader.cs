using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Interop;

public sealed class ScriptLoader : IScriptLoader, IAsyncDisposable
{
    private readonly JsModule _module;

    public ScriptLoader(IJSRuntime js)
    {
        _module = new JsModule(js, "./js/script-loader.js");
    }

    public async Task LoadAsync(string relativeUrl, CancellationToken cancellationToken = default)
    {
        await _module.InvokeVoidAsync("loadScript", cancellationToken, relativeUrl);
    }

    public async Task ConfigureAmdLanguageAsync(
        string language,
        CancellationToken cancellationToken = default
    )
    {
        await _module.InvokeVoidAsync("configureAmdLanguage", cancellationToken, language);
    }

    public async Task LoadAmdModuleAsync(
        string moduleId,
        CancellationToken cancellationToken = default
    )
    {
        await _module.InvokeVoidAsync("loadAmdModule", cancellationToken, moduleId);
    }

    public ValueTask DisposeAsync()
    {
        return _module.DisposeAsync();
    }
}
