using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class NegatedComparisonRule : TranslationRule<BooleanComparisonExpression>
{
    protected override bool AppliesTo(
        BooleanComparisonExpression comparison,
        FragmentNode node,
        TranslationContext context
    )
    {
        return comparison.ComparisonType
            is BooleanComparisonType.NotLessThan
                or BooleanComparisonType.NotGreaterThan;
    }

    protected override void Translate(
        BooleanComparisonExpression comparison,
        FragmentNode node,
        TranslationContext context
    )
    {
        IReadOnlyList<TSqlParserToken> tokens = OperatorTokens.Between(
            context,
            comparison.FirstExpression,
            comparison.SecondExpression
        );
        if (tokens.Count == 0)
            return;

        TSqlParserToken last = tokens[tokens.Count - 1];
        string replacement =
            comparison.ComparisonType == BooleanComparisonType.NotLessThan ? ">=" : "<=";
        context.ReplaceRange(tokens[0].Offset, last.Offset + last.Text.Length, replacement);
    }
}
