using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class LeftRightFunctionRule : ITranslationRule
{
    public void Apply(FragmentNode node, TranslationContext context)
    {
        if (node.Fragment is not (LeftFunctionCall or RightFunctionCall))
            return;

        TSqlParserToken nameToken = context.TokenAt(node.Fragment.FirstTokenIndex);
        context.ReplaceToken(
            nameToken,
            SqliteSyntax.QuoteIdentifier(nameToken.Text.ToUpperInvariant())
        );
    }
}
