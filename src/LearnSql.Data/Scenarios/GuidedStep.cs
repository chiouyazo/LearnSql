namespace LearnSql.Data.Scenarios;

public sealed record GuidedStep
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Sql { get; init; }
    public required GuidedStepKind Kind { get; init; }
    public required QueryExplanation QueryExplanation { get; init; }
    public required string ResultExplanation { get; init; }
    public required string DatabaseEffect { get; init; }
    public int? ExpectedResultRows { get; init; }
    public int? ExpectedRowsAffected { get; init; }
    public bool ExpectsError { get; init; }
}
