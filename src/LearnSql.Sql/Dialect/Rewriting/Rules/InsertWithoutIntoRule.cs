using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class InsertWithoutIntoRule : TranslationRule<InsertSpecification>
{
    protected override bool AppliesTo(
        InsertSpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        int next = context.NextSignificantTokenIndex(specification.FirstTokenIndex);
        return context.TokenAt(next).TokenType != TSqlTokenType.Into;
    }

    protected override void Translate(
        InsertSpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.InsertBefore(specification.Target, "INTO ");
    }
}
