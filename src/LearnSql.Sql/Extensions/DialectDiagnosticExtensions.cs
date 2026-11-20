using LearnSql.Sql.Dialect.Diagnostics;

namespace LearnSql.Sql.Extensions;

public static class DialectDiagnosticExtensions
{
    public static string ToSummary(this DialectDiagnostic diagnostic)
    {
        return $"L: {diagnostic.Span.Start.Line}:{diagnostic.Span.Start.Column} [{diagnostic.Code}] {diagnostic.Message}";
    }
}
