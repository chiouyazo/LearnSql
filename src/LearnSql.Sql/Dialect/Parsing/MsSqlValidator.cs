using LearnSql.Sql.Dialect.Batching;
using LearnSql.Sql.Dialect.Diagnostics;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Parsing;

public static class MsSqlValidator
{
    public static IReadOnlyList<DialectDiagnostic> Validate(
        string sql,
        SqlVersion version = SqlVersion.Sql180
    )
    {
        SourceText source = new SourceText(sql);
        ParseDiagnosticFactory diagnostics = new ParseDiagnosticFactory(source);
        MsSqlParser parser = new MsSqlParser(version);

        List<DialectDiagnostic> result = [];
        foreach (SqlBatchSource batch in new BatchSplitter().Split(sql))
            result.AddRange(parser.Parse(batch, diagnostics).Diagnostics);

        return result;
    }
}
