using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class ReservedFunctionNameRule : TranslationRule<FunctionCall>
{
    private static readonly HashSet<string> SqliteKeywordNames = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "ISNULL",
    };

    protected override bool AppliesTo(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        return call.CallTarget is null && SqliteKeywordNames.Contains(call.FunctionName.Value);
    }

    protected override void Translate(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(
            call.FunctionName,
            SqliteSyntax.QuoteIdentifier(call.FunctionName.Value.ToUpperInvariant())
        );
    }
}
