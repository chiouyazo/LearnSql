using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class TopRowFilterRule : TranslationRule<QuerySpecification>
{
    protected override bool AppliesTo(
        QuerySpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        TopRowFilter? top = specification.TopRowFilter;
        return top is not null && !top.Percent && !top.WithTies && top.Expression is not null;
    }

    protected override void Translate(
        QuerySpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        TopRowFilter top = specification.TopRowFilter;
        int nextTokenIndex = context.NextSignificantTokenIndex(top.LastTokenIndex);
        context.DeleteRange(top.StartOffset, context.TokenAt(nextTokenIndex).Offset);

        Func<string> limit = () => " LIMIT " + context.Render(top.Expression);

        if (node.Parent?.Fragment is BinaryQueryExpression)
        {
            context.InsertBefore(specification, "SELECT * FROM (");
            context.InsertAfter(specification, () => limit() + ")");
        }
        else
        {
            context.InsertAfter(specification, limit);
        }
    }
}
