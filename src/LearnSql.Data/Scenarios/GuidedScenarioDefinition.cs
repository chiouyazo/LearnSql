namespace LearnSql.Data.Scenarios;

public sealed record GuidedScenarioDefinition
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required ExerciseTopic Topic { get; init; }
    public required bool RequiresFreshDatabase { get; init; }
    public required IReadOnlyList<GuidedStep> Steps { get; init; }
}
