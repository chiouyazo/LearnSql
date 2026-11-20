namespace LearnSql.Data.Scenarios;

public sealed record QueryExplanation
{
    public required string Summary { get; init; }
    public required IReadOnlyList<SqlPartExplanation> Parts { get; init; }
}
