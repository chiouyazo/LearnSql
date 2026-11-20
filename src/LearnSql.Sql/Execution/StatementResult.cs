using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Execution;

public sealed record StatementResult(
    int BatchIndex,
    int Iteration,
    StatementKind Kind,
    SourceSpan Origin,
    IReadOnlyList<ResultSet> ResultSets,
    int? RowsAffected,
    TimeSpan Duration,
    IReadOnlyList<string> Messages
)
{
    public bool Truncated => ResultSets.Any((ResultSet resultSet) => resultSet.Truncated);
}
