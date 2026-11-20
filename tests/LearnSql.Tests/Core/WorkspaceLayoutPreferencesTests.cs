using LearnSql.Core.Services.LearningPanel;
using LearnSql.Core.Services.Storage;

namespace LearnSql.Tests.Core;

public sealed class WorkspaceLayoutPreferencesTests
{
    [Fact]
    public async Task EnsureLoadedAsync_WithEmptyStorage_UsesDefaults()
    {
        WorkspaceLayoutPreferences preferences = new WorkspaceLayoutPreferences(
            new MemoryStorage()
        );

        await preferences.EnsureLoadedAsync();

        Assert.True(preferences.PanelOpen);
        Assert.True(preferences.SchemaOpen);
        Assert.Null(preferences.PanelWidth);
        Assert.Null(preferences.ActiveTabId);
    }

    [Fact]
    public async Task Setters_PersistAndALaterInstanceRestoresThem()
    {
        MemoryStorage storage = new MemoryStorage();
        WorkspaceLayoutPreferences first = new WorkspaceLayoutPreferences(storage);
        await first.EnsureLoadedAsync();
        await first.SetPanelOpenAsync(false);
        await first.SetSchemaOpenAsync(false);
        await first.SetPanelWidthAsync(420);
        await first.SetActiveTabAsync("exercises");

        WorkspaceLayoutPreferences second = new WorkspaceLayoutPreferences(storage);
        await second.EnsureLoadedAsync();

        Assert.False(second.PanelOpen);
        Assert.False(second.SchemaOpen);
        Assert.Equal(420, second.PanelWidth);
        Assert.Equal("exercises", second.ActiveTabId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(WorkspaceLayoutPreferences.MinPanelWidth - 1)]
    [InlineData(WorkspaceLayoutPreferences.MaxPanelWidth + 1)]
    public async Task SetPanelWidthAsync_OutOfRange_IsIgnored(int width)
    {
        MemoryStorage storage = new MemoryStorage();
        WorkspaceLayoutPreferences preferences = new WorkspaceLayoutPreferences(storage);

        await preferences.SetPanelWidthAsync(width);

        Assert.Null(preferences.PanelWidth);
        Assert.Equal(0, storage.Writes);
    }

    [Fact]
    public async Task EnsureLoadedAsync_StoredWidthOutOfRange_IsDiscarded()
    {
        MemoryStorage storage = new MemoryStorage();
        await storage.SetAsync(StorageKeys.LearningPanelWidth, 3);
        WorkspaceLayoutPreferences preferences = new WorkspaceLayoutPreferences(storage);

        await preferences.EnsureLoadedAsync();

        Assert.Null(preferences.PanelWidth);
    }

    [Fact]
    public async Task SetActiveTabAsync_Blank_Throws()
    {
        WorkspaceLayoutPreferences preferences = new WorkspaceLayoutPreferences(
            new MemoryStorage()
        );

        await Assert.ThrowsAsync<ArgumentException>(() => preferences.SetActiveTabAsync(" "));
    }

    [Fact]
    public async Task EnsureLoadedAsync_ReadsStorageOnlyOnce()
    {
        MemoryStorage storage = new MemoryStorage();
        WorkspaceLayoutPreferences preferences = new WorkspaceLayoutPreferences(storage);

        await preferences.EnsureLoadedAsync();
        int reads = storage.Reads;
        await preferences.EnsureLoadedAsync();

        Assert.Equal(reads, storage.Reads);
    }

    private sealed class MemoryStorage : IBrowserStorage
    {
        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();

        public int Reads { get; private set; }

        public int Writes { get; private set; }

        public ValueTask<T?> GetAsync<T>(
            StorageKey<T> key,
            CancellationToken cancellationToken = default
        )
        {
            Reads++;
            return ValueTask.FromResult(
                _values.TryGetValue(key.Name, out object? value) ? (T?)value : default
            );
        }

        public ValueTask<bool> SetAsync<T>(
            StorageKey<T> key,
            T value,
            CancellationToken cancellationToken = default
        )
        {
            Writes++;
            _values[key.Name] = value;
            return ValueTask.FromResult(true);
        }

        public ValueTask<bool> RemoveAsync<T>(
            StorageKey<T> key,
            CancellationToken cancellationToken = default
        )
        {
            return ValueTask.FromResult(_values.Remove(key.Name));
        }
    }
}
