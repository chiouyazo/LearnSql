using LearnSql.Sql.Dialect.Diagnostics;

namespace LearnSql.Sql.Dialect;

public sealed record DialectTranslationResult(
    bool Success,
    IReadOnlyList<TranslatedBatch> Batches,
    IReadOnlyList<DialectDiagnostic> Diagnostics
)
{
    public IEnumerable<DialectDiagnostic> Errors =>
        Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
