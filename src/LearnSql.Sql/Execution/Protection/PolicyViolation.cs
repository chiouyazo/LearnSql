using LearnSql.Sql.Dialect;

namespace LearnSql.Sql.Execution;

public sealed record PolicyViolation(
    TranslatedStatement Statement,
    string ObjectName,
    string Message
);
