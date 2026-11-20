namespace LearnSql.Core.Layout;

public partial class LanguageSwitcher
{
    private async Task SelectAsync(CultureInfo culture)
    {
        if (!await Localization.SelectAsync(culture.Name))
        {
            return;
        }

        await Drafts.FlushAsync();
        Navigation.Refresh(forceReload: true);
    }
}
