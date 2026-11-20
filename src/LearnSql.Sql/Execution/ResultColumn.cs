using LearnSql.Sql.Dialect.Typing;

namespace LearnSql.Sql.Execution;

public sealed record ResultColumn(string Name, string? TypeName, ValueKind Kind);
