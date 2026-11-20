using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Models;
using LearnSql.Sql.MsSql;

namespace LearnSql.Sql.Execution;

public sealed record ExecutionError(
    ExecutionErrorKind Kind,
    string Message,
    string? Detail = null,
    MsSqlError? MsSql = null,
    SqliteError? Sqlite = null,
    SourceSpan? Origin = null
)
{
    public string Display => MsSql?.ToString() ?? Message;
}
