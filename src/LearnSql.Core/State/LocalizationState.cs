using LearnSql.Core.Services.Interop;
using LearnSql.Core.Services.Storage;

namespace LearnSql.Core.State;

public sealed class LocalizationState
{
    public const string DefaultLanguage = "de";

    private readonly IBrowserStorage _storage;
    private readonly IDocumentService _document;

    public LocalizationState(IBrowserStorage storage, IDocumentService document)
    {
        _storage = storage;
        _document = document;
        Supported = [new CultureInfo("de"), new CultureInfo("en")];
        Current = Supported[0];
    }

    public IReadOnlyList<CultureInfo> Supported { get; }

    public CultureInfo Current { get; private set; }

    public async Task InitializeAsync()
    {
        string? stored = await _storage.GetAsync(StorageKeys.Language);
        Apply(Resolve(stored));
        await _document.SetLanguageAsync(Current.TwoLetterISOLanguageName);
    }

    public async Task<bool> SelectAsync(string languageCode)
    {
        CultureInfo target = Resolve(languageCode);
        if (target.Name == Current.Name)
        {
            return false;
        }

        await _storage.SetAsync(StorageKeys.Language, target.Name);
        return true;
    }

    private CultureInfo Resolve(string? languageCode)
    {
        CultureInfo? match = Supported.FirstOrDefault(
            (CultureInfo culture) =>
                string.Equals(culture.Name, languageCode, StringComparison.OrdinalIgnoreCase)
        );
        return match ?? Supported[0];
    }

    private void Apply(CultureInfo culture)
    {
        Current = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
