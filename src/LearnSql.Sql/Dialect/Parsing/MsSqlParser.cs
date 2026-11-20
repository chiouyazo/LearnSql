using LearnSql.Sql.Dialect.Batching;
using LearnSql.Sql.Dialect.Diagnostics;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Parsing;

public sealed class MsSqlParser
{
    private readonly SqlVersion _version;

    public MsSqlParser(SqlVersion version)
    {
        _version = version;
    }

    public ParsedBatch Parse(SqlBatchSource batch, ParseDiagnosticFactory diagnostics)
    {
        TSqlParser parser = TSqlParser.CreateParser(_version, true);
        TSqlFragment fragment;
        IList<ParseError> errors;
        using (StringReader reader = new StringReader(batch.Text))
        {
            fragment = parser.Parse(reader, out errors);
        }

        List<DialectDiagnostic> converted = [];
        foreach (ParseError error in errors)
            converted.Add(diagnostics.Create(error, batch.StartOffset));

        return new ParsedBatch(batch, (TSqlScript)fragment, converted);
    }
}
