using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class StringConcatenationRule : TranslationRule<BinaryExpression>
{
    protected override bool AppliesTo(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        if (expression.BinaryExpressionType != BinaryExpressionType.Add)
            return false;

        ValueKind left = context.Types.Infer(expression.FirstExpression);
        ValueKind right = context.Types.Infer(expression.SecondExpression);
        return AdditionClassifier.Classify(left, right) == AdditionMeaning.Concatenation;
    }

    protected override void Translate(
        BinaryExpression expression,
        FragmentNode node,
        TranslationContext context
    )
    {
        TSqlParserToken? plus = OperatorTokens.FindSingle(
            context,
            expression.FirstExpression,
            expression.SecondExpression,
            TSqlTokenType.Plus
        );
        if (plus is not null)
            context.ReplaceToken(plus, "||");
    }
}
