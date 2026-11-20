using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class BinaryLiteralRule : TranslationRule<BinaryLiteral>
{
    protected override void Translate(
        BinaryLiteral literal,
        FragmentNode node,
        TranslationContext context
    )
    {
        string digits = literal.Value.Length >= 2 ? literal.Value.Substring(2) : string.Empty;
        if (digits.Length % 2 != 0)
            digits = "0" + digits;

        context.Replace(literal, "X'" + digits + "'");
    }
}
