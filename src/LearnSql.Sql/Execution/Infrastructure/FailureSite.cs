using LearnSql.Sql.Dialect;

namespace LearnSql.Sql.Execution;

internal sealed record FailureSite(TranslatedStatement Statement, int RelativeLine);
