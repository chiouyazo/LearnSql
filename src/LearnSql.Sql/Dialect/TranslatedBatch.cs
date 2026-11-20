using LearnSql.Sql.Dialect.Diagnostics;

namespace LearnSql.Sql.Dialect;

public sealed record TranslatedBatch(
    IReadOnlyList<TranslatedStatement> Statements,
    int RepeatCount,
    SourceSpan Span
);
