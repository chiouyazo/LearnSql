namespace LearnSql.Data.Scenarios;

public sealed record ExerciseDefinition
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required ExerciseTopic Topic { get; init; }
    public required ExerciseDifficulty Difficulty { get; init; }
    public required IReadOnlyList<string> Hints { get; init; }
    public required string SolutionSql { get; init; }
    public required bool OrderMatters { get; init; }
    public required int ExpectedRowCount { get; init; }
}
