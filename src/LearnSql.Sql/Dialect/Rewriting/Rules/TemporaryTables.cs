using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public static class TemporaryTables
{
    public static bool IsTemporary(SchemaObjectName name)
    {
        return name.BaseIdentifier.Value.StartsWith('#');
    }
}
