namespace LearnSql.Data.Scenarios;

public sealed record ScenarioTexts
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<ExerciseTexts>? Exercises { get; init; }
    public IReadOnlyList<GuidedScenarioTexts>? GuidedScenarios { get; init; }
}
