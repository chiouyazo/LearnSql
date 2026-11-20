using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Translation;

public sealed record ErrorContext(
    StatementKind StatementKind,
    string? TargetTable,
    ForeignKeyConflict? ForeignKey = null
)
{
    public static ErrorContext None { get; } = new ErrorContext(StatementKind.Other, null);
}
