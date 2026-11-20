using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.Services.LearningPanel;

public sealed class WorkspaceLayoutPreferences
{
    public const int MinPanelWidth = 160;
    public const int MaxPanelWidth = 4000;

    private readonly IBrowserStorage _storage;
    private Task? _loading;

    public WorkspaceLayoutPreferences(IBrowserStorage storage)
    {
        _storage = storage;
    }

    public bool PanelOpen { get; private set; } = true;

    public bool SchemaOpen { get; private set; } = true;

    public int? PanelWidth { get; private set; }

    public string? ActiveTabId { get; private set; }

    public Task EnsureLoadedAsync()
    {
        _loading ??= LoadAsync();
        return _loading;
    }

    public async Task SetPanelOpenAsync(bool open)
    {
        PanelOpen = open;
        await _storage.SetAsync(StorageKeys.LearningPanelCollapsed, !open);
    }

    public async Task SetSchemaOpenAsync(bool open)
    {
        SchemaOpen = open;
        await _storage.SetAsync(StorageKeys.SchemaPaneCollapsed, !open);
    }

    public async Task SetPanelWidthAsync(int width)
    {
        if (!IsValidWidth(width))
        {
            return;
        }

        PanelWidth = width;
        await _storage.SetAsync(StorageKeys.LearningPanelWidth, width);
    }

    public async Task SetActiveTabAsync(string tabId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tabId);

        ActiveTabId = tabId;
        await _storage.SetAsync(StorageKeys.LearningPanelTab, tabId);
    }

    public static bool IsValidWidth(int width)
    {
        return width is >= MinPanelWidth and <= MaxPanelWidth;
    }

    private async Task LoadAsync()
    {
        PanelOpen = !await _storage.GetAsync(StorageKeys.LearningPanelCollapsed);
        SchemaOpen = !await _storage.GetAsync(StorageKeys.SchemaPaneCollapsed);
        int storedWidth = await _storage.GetAsync(StorageKeys.LearningPanelWidth);
        PanelWidth = IsValidWidth(storedWidth) ? storedWidth : null;
        ActiveTabId = await _storage.GetAsync(StorageKeys.LearningPanelTab);
    }
}
