using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Unsupported;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Parsing;

public sealed class ParseDiagnosticFactory
{
    private const int IncorrectSyntaxErrorNumber = 102;

    private readonly SourceText _source;

    public ParseDiagnosticFactory(SourceText source)
    {
        _source = source;
    }

    public DialectDiagnostic Create(ParseError error, int batchStartOffset)
    {
        int offset = Math.Clamp(batchStartOffset + error.Offset, 0, _source.Length);
        int length = MeasureErrorLength(offset);
        return new DialectDiagnostic(
            DiagnosticKind.ParseError,
            DiagnosticSeverity.Error,
            "SQL" + error.Number,
            error.Message,
            MistypedStatementSuggestion.ForParseError(_source.Text, offset, length),
            _source.GetSpan(offset, length),
            IncorrectSyntaxErrorNumber
        );
    }

    private int MeasureErrorLength(int offset)
    {
        string text = _source.Text;
        if (offset >= text.Length)
            return 0;

        int end = offset;
        while (end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_'))
            end++;

        return end > offset ? end - offset : 1;
    }
}
