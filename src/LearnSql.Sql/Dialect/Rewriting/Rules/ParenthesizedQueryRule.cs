using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class ParenthesizedQueryRule : TranslationRule<QueryParenthesisExpression>
{
    protected override void Translate(
        QueryParenthesisExpression query,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.InsertBefore(query, "SELECT * FROM ");
    }
}
