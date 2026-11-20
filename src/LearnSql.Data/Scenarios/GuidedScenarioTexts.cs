namespace LearnSql.Data.Scenarios;

public sealed record GuidedScenarioTexts
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<GuidedStepTexts> Steps { get; init; }
}
