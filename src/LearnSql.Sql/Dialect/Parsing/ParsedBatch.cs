using LearnSql.Sql.Dialect.Batching;
using LearnSql.Sql.Dialect.Diagnostics;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Parsing;

public sealed record ParsedBatch(
    SqlBatchSource Source,
    TSqlScript Script,
    IReadOnlyList<DialectDiagnostic> Diagnostics
)
{
    public bool HasErrors => Diagnostics.Count > 0;
}
