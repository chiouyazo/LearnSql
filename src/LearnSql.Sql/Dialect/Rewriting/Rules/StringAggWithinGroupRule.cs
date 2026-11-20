using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class StringAggWithinGroupRule : TranslationRule<FunctionCall>
{
    protected override bool AppliesTo(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        return call.WithinGroupClause is not null
            && call.Parameters.Count > 0
            && string.Equals(
                call.FunctionName.Value,
                "STRING_AGG",
                StringComparison.OrdinalIgnoreCase
            );
    }

    protected override void Translate(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        ScalarExpression lastParameter = call.Parameters[call.Parameters.Count - 1];
        OrderByClause orderBy = call.WithinGroupClause.OrderByClause;

        context.ReplaceRange(
            lastParameter.StartOffset + lastParameter.FragmentLength,
            orderBy.StartOffset,
            " "
        );
        context.ReplaceRange(
            orderBy.StartOffset + orderBy.FragmentLength,
            call.StartOffset + call.FragmentLength,
            ")"
        );
    }
}
