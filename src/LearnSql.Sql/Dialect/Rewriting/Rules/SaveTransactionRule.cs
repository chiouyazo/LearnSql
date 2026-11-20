using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class SaveTransactionRule : TranslationRule<SaveTransactionStatement>
{
    protected override bool AppliesTo(
        SaveTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return statement.Name?.Identifier is not null;
    }

    protected override void Translate(
        SaveTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(
            statement,
            "SAVEPOINT " + SqliteSyntax.QuoteIdentifier(statement.Name.Identifier.Value)
        );
    }
}
