using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DateArithmeticRule : TranslationRule<BinaryExpression>
{
    protected override bool AppliesTo(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        ValueKind left = context.Types.Infer(expression.FirstExpression);
        ValueKind right = context.Types.Infer(expression.SecondExpression);

        switch (expression.BinaryExpressionType)
        {
            case BinaryExpressionType.Add:
                return AdditionClassifier.Classify(left, right) == AdditionMeaning.DateArithmetic;
            case BinaryExpressionType.Subtract:
                return left == ValueKind.DateTime && right == ValueKind.Numeric;
            default:
                return false;
        }
    }

    protected override void Translate(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        bool subtract = expression.BinaryExpressionType == BinaryExpressionType.Subtract;
        bool dateFirst = context.Types.Infer(expression.FirstExpression) == ValueKind.DateTime;
        ScalarExpression date = dateFirst
            ? expression.FirstExpression
            : expression.SecondExpression;
        ScalarExpression days = dateFirst
            ? expression.SecondExpression
            : expression.FirstExpression;

        context.Replace(
            expression,
            () =>
            {
                string dayCount = subtract
                    ? "-(" + context.Render(days) + ")"
                    : context.Render(days);
                return InternalFunctionNames.AddDays
                    + "("
                    + context.Render(date)
                    + ", "
                    + dayCount
                    + ")";
            }
        );
    }
}
