using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace LearnSql.Core.Services.Storage;

public sealed class BrowserStorage : IBrowserStorage
{
    private readonly IJSRuntime _js;
    private readonly ILogger<BrowserStorage> _logger;

    public BrowserStorage(IJSRuntime js, ILogger<BrowserStorage> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async ValueTask<T?> GetAsync<T>(
        StorageKey<T> key,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            string? raw = await _js.InvokeAsync<string?>(
                "localStorage.getItem",
                cancellationToken,
                key.Name
            );
            return raw is null ? default : JsonSerializer.Deserialize(raw, key.TypeInfo);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Reading storage key {Key} failed.", key.Name);
            return default;
        }
    }

    public async ValueTask<bool> SetAsync<T>(
        StorageKey<T> key,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            string raw = JsonSerializer.Serialize(value, key.TypeInfo);
            await _js.InvokeVoidAsync("localStorage.setItem", cancellationToken, key.Name, raw);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Writing storage key {Key} failed.", key.Name);
            return false;
        }
    }

    public async ValueTask<bool> RemoveAsync<T>(
        StorageKey<T> key,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key.Name);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Removing storage key {Key} failed.", key.Name);
            return false;
        }
    }
}
