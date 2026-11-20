using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DefaultExpressionRule : TranslationRule<DefaultConstraintDefinition>
{
    protected override bool AppliesTo(
        DefaultConstraintDefinition constraint,
        FragmentNode node,
        TranslationContext context
    )
    {
        return constraint.Expression
            is not (
                ParenthesisExpression
                or IntegerLiteral
                or NumericLiteral
                or RealLiteral
                or StringLiteral
                or NullLiteral
                or UnaryExpression
            );
    }

    protected override void Translate(
        DefaultConstraintDefinition constraint,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.InsertBefore(constraint.Expression, "(");
        context.InsertAfter(constraint.Expression, ")");
    }
}
