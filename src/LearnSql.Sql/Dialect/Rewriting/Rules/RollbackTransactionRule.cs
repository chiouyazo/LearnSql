using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class RollbackTransactionRule : TranslationRule<RollbackTransactionStatement>
{
    protected override bool AppliesTo(
        RollbackTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return statement.Name is null || statement.Name.Identifier is not null;
    }

    protected override void Translate(
        RollbackTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        string text = statement.Name is null
            ? "ROLLBACK"
            : "ROLLBACK TO " + SqliteSyntax.QuoteIdentifier(statement.Name.Identifier.Value);
        context.Replace(statement, text);
    }
}
