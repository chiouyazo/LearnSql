namespace LearnSql.Data.Scenarios;

public sealed record ScenarioScripts(
    string ScenarioId,
    IReadOnlyList<SqlScript> Schema,
    IReadOnlyList<SqlScript> Views,
    IReadOnlyList<SqlScript> Seed
)
{
    public IEnumerable<SqlScript> InExecutionOrder => Schema.Concat(Views).Concat(Seed);
}
