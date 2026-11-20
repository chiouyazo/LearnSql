namespace LearnSql.Core.Services.Scenarios;

public interface IScenarioTextProvider
{
    bool CoversCurrentLanguage { get; }

    ScenarioDefinition Localize(ScenarioDefinition scenario);

    string GetTitle(ScenarioDefinition scenario);

    string GetDescription(ScenarioDefinition scenario);
}
