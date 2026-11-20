namespace LearnSql.Core.Services.Navigation;

public static class ScenarioRoutes
{
    public const string Template = "/scenarios/{ScenarioId}";

    private const string Prefix = "scenarios/";

    public static string Path(string scenarioId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioId);

        return Prefix + Uri.EscapeDataString(scenarioId);
    }

    public static bool TryGetScenarioId(string? relativePath, out string scenarioId)
    {
        scenarioId = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        int end = relativePath.IndexOfAny(['?', '#']);
        string path = (end < 0 ? relativePath : relativePath[..end]).Trim('/');
        if (!path.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string segment = path[Prefix.Length..];
        if (segment.Length == 0 || segment.Contains('/', StringComparison.Ordinal))
        {
            return false;
        }

        scenarioId = Uri.UnescapeDataString(segment);
        return scenarioId.Length > 0;
    }

    public static ScenarioDefinition? Resolve(
        IEnumerable<ScenarioDefinition> scenarios,
        string? scenarioId
    )
    {
        ArgumentNullException.ThrowIfNull(scenarios);

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            return null;
        }

        return scenarios.FirstOrDefault(
            (ScenarioDefinition scenario) =>
                string.Equals(scenario.Id, scenarioId, StringComparison.OrdinalIgnoreCase)
        );
    }
}
