using LearnSql.Core.Services.Interop;
using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.State;

public sealed class ThemeState
{
    private const string Dark = "dark";
    private const string Light = "light";

    private readonly IBrowserStorage _storage;
    private readonly IDocumentService _document;

    public ThemeState(IBrowserStorage storage, IDocumentService document)
    {
        _storage = storage;
        _document = document;
    }

    public event EventHandler? Changed;

    public bool IsDark { get; private set; } = true;

    public string MonacoTheme => IsDark ? "vs-dark" : "vs";

    public async Task InitializeAsync()
    {
        string? stored = await _storage.GetAsync(StorageKeys.Theme);
        IsDark = stored switch
        {
            Dark => true,
            Light => false,
            _ => await _document.PrefersDarkAsync(),
        };
    }

    public Task ToggleAsync()
    {
        return SetDarkAsync(!IsDark);
    }

    public async Task SetDarkAsync(bool isDark)
    {
        if (IsDark == isDark)
        {
            return;
        }

        IsDark = isDark;
        Changed?.Invoke(this, EventArgs.Empty);
        await _storage.SetAsync(StorageKeys.Theme, isDark ? Dark : Light);
    }
}
