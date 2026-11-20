namespace LearnSql.Data.Scenarios;

public sealed record ScenarioDefinition
{
    public required string Id { get; init; }
    public required int SortOrder { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> SchemaResources { get; init; }
    public required IReadOnlyList<string> ViewResources { get; init; }
    public required IReadOnlyList<string> SeedResources { get; init; }
    public IReadOnlyList<ExerciseDefinition> Exercises { get; init; } = [];
    public IReadOnlyList<GuidedScenarioDefinition> GuidedScenarios { get; init; } = [];
}
