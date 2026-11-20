using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public static class TransactionNames
{
    public static bool IsPlain(TransactionStatement statement)
    {
        return statement.Name is null;
    }
}
