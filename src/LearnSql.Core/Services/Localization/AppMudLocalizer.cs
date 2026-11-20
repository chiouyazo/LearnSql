namespace LearnSql.Core.Services.Localization;

public sealed class AppMudLocalizer : MudLocalizer
{
    private readonly IStringLocalizer<MudStrings> _strings;

    public AppMudLocalizer(IStringLocalizer<MudStrings> strings)
    {
        _strings = strings;
    }

    public override LocalizedString this[string key]
    {
        get
        {
            LocalizedString value = _strings[key];
            return value.ResourceNotFound ? new LocalizedString(key, key, true) : value;
        }
    }

    public override LocalizedString this[string key, params object[] arguments]
    {
        get
        {
            LocalizedString value = _strings[key, arguments];
            return value.ResourceNotFound ? new LocalizedString(key, key, true) : value;
        }
    }
}
