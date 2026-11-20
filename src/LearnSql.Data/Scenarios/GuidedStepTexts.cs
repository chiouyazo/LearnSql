namespace LearnSql.Data.Scenarios;

public sealed record GuidedStepTexts
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required QueryExplanation QueryExplanation { get; init; }
    public required string ResultExplanation { get; init; }
    public required string DatabaseEffect { get; init; }
}
