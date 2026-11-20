using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class IntersectPrecedenceRule : TranslationRule<BinaryQueryExpression>
{
    protected override bool AppliesTo(
        BinaryQueryExpression query,
        FragmentNode node,
        TranslationContext context
    )
    {
        return query.BinaryQueryExpressionType == BinaryQueryExpressionType.Intersect
            && query.FirstQueryExpression
                is BinaryQueryExpression
                {
                    BinaryQueryExpressionType: not BinaryQueryExpressionType.Intersect
                };
    }

    protected override void Translate(
        BinaryQueryExpression query,
        FragmentNode node,
        TranslationContext context
    )
    {
        BinaryQueryExpression left = (BinaryQueryExpression)query.FirstQueryExpression;
        QueryExpression groupStart = left.SecondQueryExpression;
        QueryExpression groupEnd = FindLastOperandOfIntersectGroup(query, node);

        context.InsertBefore(groupStart, "SELECT * FROM (");
        context.InsertAfter(groupEnd, ")");
    }

    private static QueryExpression FindLastOperandOfIntersectGroup(
        BinaryQueryExpression query,
        FragmentNode node
    )
    {
        BinaryQueryExpression top = query;
        FragmentNode current = node;
        while (
            current.Parent?.Fragment is BinaryQueryExpression parent
            && parent.BinaryQueryExpressionType == BinaryQueryExpressionType.Intersect
            && ReferenceEquals(parent.FirstQueryExpression, top)
        )
        {
            top = parent;
            current = current.Parent;
        }
        return top.SecondQueryExpression;
    }
}
