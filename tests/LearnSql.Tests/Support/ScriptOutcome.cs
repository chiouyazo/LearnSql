using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Exceptions;

namespace LearnSql.Tests.Support;

public sealed record ScriptOutcome(
    DialectTranslationResult Translation,
    IReadOnlyList<ResultSet> ResultSets,
    int RecordsAffected,
    TranslatedException? Error
)
{
    public bool TranslationSucceeded => Translation.Success;

    public bool Succeeded => Translation.Success && Error is null;

    public ResultSet LastResultSet => ResultSets[^1];

    public string Describe()
    {
        if (!Translation.Success)
        {
            return string.Join(
                "; ",
                Translation.Errors.Select(
                    (DialectDiagnostic diagnostic) =>
                        $"[{diagnostic.Code}] {diagnostic.Message} @{diagnostic.Span.Start.Line}:{diagnostic.Span.Start.Column}"
                )
            );
        }

        return Error is null ? "ok" : Error.Message;
    }
}
