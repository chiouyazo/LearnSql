using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Interop;

public sealed class FileDownloader : IFileDownloader, IAsyncDisposable
{
    private readonly JsModule _module;

    public FileDownloader(IJSRuntime js)
    {
        _module = new JsModule(js, "./js/file-download.js");
    }

    public async Task DownloadAsync(
        string fileName,
        string contentType,
        byte[] content,
        CancellationToken cancellationToken = default
    )
    {
        await _module.InvokeVoidAsync(
            "downloadFile",
            cancellationToken,
            fileName,
            contentType,
            content
        );
    }

    public ValueTask DisposeAsync()
    {
        return _module.DisposeAsync();
    }
}
