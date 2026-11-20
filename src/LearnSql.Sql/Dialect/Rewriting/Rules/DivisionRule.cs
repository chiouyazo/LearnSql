using System.Globalization;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DivisionRule : TranslationRule<BinaryExpression>
{
    protected override bool AppliesTo(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        return expression.BinaryExpressionType
            is BinaryExpressionType.Divide
                or BinaryExpressionType.Modulo;
    }

    protected override void Translate(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        if (expression.BinaryExpressionType == BinaryExpressionType.Modulo)
        {
            context.Replace(
                expression,
                () =>
                    ArithmeticSqlBuilder.Modulo(
                        context.Render(expression.FirstExpression),
                        context.Render(expression.SecondExpression)
                    )
            );
            return;
        }

        bool divisorIsSafe = IsNonZeroNumber(expression.SecondExpression);
        context.Replace(
            expression,
            () =>
                ArithmeticSqlBuilder.Divide(
                    context.Render(expression.FirstExpression),
                    context.Render(expression.SecondExpression),
                    divisorIsSafe
                )
        );
    }

    private static bool IsNonZeroNumber(ScalarExpression expression)
    {
        switch (expression)
        {
            case ParenthesisExpression parenthesis:
                return IsNonZeroNumber(parenthesis.Expression);
            case UnaryExpression unary:
                return IsNonZeroNumber(unary.Expression);
            case IntegerLiteral or NumericLiteral or RealLiteral or MoneyLiteral:
                return IsNonZeroLiteral(((Literal)expression).Value);
            default:
                return false;
        }
    }

    private static bool IsNonZeroLiteral(string text)
    {
        string number = text.TrimStart('$');
        return double.TryParse(
                number,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double value
            )
            && value != 0;
    }
}
