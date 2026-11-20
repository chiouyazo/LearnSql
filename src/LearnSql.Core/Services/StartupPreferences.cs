namespace LearnSql.Core.Services;

public sealed class StartupPreferences
{
    private readonly ThemeState _theme;
    private readonly LocalizationState _localization;
    private readonly IAppSession _session;

    public StartupPreferences(ThemeState theme, LocalizationState localization, IAppSession session)
    {
        _theme = theme;
        _localization = localization;
        _session = session;
    }

    public async Task LoadAsync()
    {
        await _localization.InitializeAsync();
        await _theme.InitializeAsync();
        await _session.RestoreScenarioAsync();
    }
}
