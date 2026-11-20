using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.Services.Drafts;

public sealed class DraftStore : IDraftStore, IDisposable
{
    private static readonly TimeSpan Debounce = TimeSpan.FromMilliseconds(400);

    private readonly IBrowserStorage _storage;
    private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _pending = new Dictionary<string, string>();

    private CancellationTokenSource? _debounce;

    public DraftStore(IBrowserStorage storage)
    {
        _storage = storage;
    }

    public async Task<string?> LoadAsync(
        string scenarioId,
        CancellationToken cancellationToken = default
    )
    {
        if (_cache.TryGetValue(scenarioId, out string? cached))
        {
            return cached;
        }

        string? stored = await _storage.GetAsync(StorageKeys.Draft(scenarioId), cancellationToken);
        if (stored is not null)
        {
            _cache[scenarioId] = stored;
        }

        return stored;
    }

    public void Save(string scenarioId, string text)
    {
        _cache[scenarioId] = text;
        _pending[scenarioId] = text;
        _debounce?.Cancel();
        _debounce?.Dispose();
        _debounce = new CancellationTokenSource();
        _ = FlushAfterDelayAsync(_debounce.Token);
    }

    public async Task FlushAsync()
    {
        KeyValuePair<string, string>[] drafts = [.. _pending];
        _pending.Clear();
        foreach (KeyValuePair<string, string> draft in drafts)
        {
            await _storage.SetAsync(StorageKeys.Draft(draft.Key), draft.Value);
        }
    }

    public void Dispose()
    {
        _debounce?.Cancel();
        _debounce?.Dispose();
    }

    private async Task FlushAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Debounce, cancellationToken);
            await FlushAsync();
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }
}
