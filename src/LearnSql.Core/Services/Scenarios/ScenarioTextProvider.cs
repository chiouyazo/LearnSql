namespace LearnSql.Core.Services.Scenarios;

public sealed class ScenarioTextProvider : IScenarioTextProvider
{
    private readonly ScenarioCatalog _catalog;

    public ScenarioTextProvider(ScenarioCatalog catalog)
    {
        _catalog = catalog;
    }

    public bool CoversCurrentLanguage =>
        _catalog.Scenarios.All(
            (ScenarioDefinition scenario) =>
                _catalog.HasTexts(scenario.Id, CultureInfo.CurrentUICulture)
        );

    public ScenarioDefinition Localize(ScenarioDefinition scenario)
    {
        return _catalog.Localize(scenario, CultureInfo.CurrentUICulture);
    }

    public string GetTitle(ScenarioDefinition scenario)
    {
        return Localize(scenario).Title;
    }

    public string GetDescription(ScenarioDefinition scenario)
    {
        return Localize(scenario).Description;
    }
}
