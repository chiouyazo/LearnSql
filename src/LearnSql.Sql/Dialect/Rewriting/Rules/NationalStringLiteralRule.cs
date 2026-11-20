using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class NationalStringLiteralRule : TranslationRule<StringLiteral>
{
    protected override bool AppliesTo(
        StringLiteral literal,
        FragmentNode node,
        TranslationContext context
    )
    {
        return literal.IsNational;
    }

    protected override void Translate(
        StringLiteral literal,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.DeleteRange(literal.StartOffset, literal.StartOffset + 1);
    }
}
