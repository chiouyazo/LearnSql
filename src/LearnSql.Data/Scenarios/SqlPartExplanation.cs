namespace LearnSql.Data.Scenarios;

public sealed record SqlPartExplanation
{
    public required string Fragment { get; init; }
    public required string Explanation { get; init; }
}
