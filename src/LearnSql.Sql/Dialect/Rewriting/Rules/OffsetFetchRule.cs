using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class OffsetFetchRule : TranslationRule<OffsetClause>
{
    protected override void Translate(
        OffsetClause clause,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(
            clause,
            () =>
            {
                string fetch = clause.FetchExpression is null
                    ? "-1"
                    : context.Render(clause.FetchExpression);
                return "LIMIT " + fetch + " OFFSET " + context.Render(clause.OffsetExpression);
            }
        );
    }
}
