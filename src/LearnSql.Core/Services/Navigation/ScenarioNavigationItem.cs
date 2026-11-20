using LearnSql.Core.Services.Scenarios;

namespace LearnSql.Core.Services.Navigation;

public sealed record ScenarioNavigationItem(string ScenarioId, string Href, string Title)
{
    public static IReadOnlyList<ScenarioNavigationItem> Create(
        IEnumerable<ScenarioDefinition> scenarios,
        IScenarioTextProvider texts
    )
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(texts);

        return
        [
            .. scenarios
                .OrderBy((ScenarioDefinition scenario) => scenario.SortOrder)
                .ThenBy((ScenarioDefinition scenario) => scenario.Id, StringComparer.Ordinal)
                .Select(
                    (ScenarioDefinition scenario) =>
                        new ScenarioNavigationItem(
                            scenario.Id,
                            ScenarioRoutes.Path(scenario.Id),
                            texts.GetTitle(scenario)
                        )
                ),
        ];
    }

    public bool IsActive(string? activeScenarioId)
    {
        return string.Equals(ScenarioId, activeScenarioId, StringComparison.OrdinalIgnoreCase);
    }
}
