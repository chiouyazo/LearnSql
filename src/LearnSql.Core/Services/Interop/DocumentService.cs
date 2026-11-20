using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Interop;

public sealed class DocumentService : IDocumentService, IAsyncDisposable
{
    private readonly JsModule _module;

    public DocumentService(IJSRuntime js)
    {
        _module = new JsModule(js, "./js/document.js");
    }

    public async Task SetLanguageAsync(
        string languageCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _module.InvokeVoidAsync("setLanguage", cancellationToken, languageCode);
        }
        catch (JSException)
        {
            return;
        }
    }

    public async Task<bool> PrefersDarkAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _module.InvokeAsync<bool>("prefersDark", cancellationToken);
        }
        catch (JSException)
        {
            return true;
        }
    }

    public async Task ScrollToTopAsync(
        ElementReference element,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _module.InvokeVoidAsync("scrollToTop", cancellationToken, element);
        }
        catch (JSException)
        {
            return;
        }
    }

    public ValueTask DisposeAsync()
    {
        return _module.DisposeAsync();
    }
}
