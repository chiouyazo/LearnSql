using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Syntax;

public static class DropIndexForms
{
    public static bool IsTranslatable(DropIndexStatement statement)
    {
        return statement.DropIndexClauses.Count == 1
            && statement.DropIndexClauses[0] is DropIndexClause { Options.Count: 0 };
    }
}
