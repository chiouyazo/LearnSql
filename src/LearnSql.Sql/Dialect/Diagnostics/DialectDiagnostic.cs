namespace LearnSql.Sql.Dialect.Diagnostics;

public sealed record DialectDiagnostic(
    DiagnosticKind Kind,
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    string? Hint,
    SourceSpan Span,
    int? MsSqlErrorNumber
);
