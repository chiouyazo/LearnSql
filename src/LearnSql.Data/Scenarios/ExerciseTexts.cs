namespace LearnSql.Data.Scenarios;

public sealed record ExerciseTexts
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> Hints { get; init; }
}
