using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DropIndexRule : TranslationRule<DropIndexStatement>
{
    protected override bool AppliesTo(
        DropIndexStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return DropIndexForms.IsTranslatable(statement);
    }

    protected override void Translate(
        DropIndexStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        DropIndexClause clause = (DropIndexClause)statement.DropIndexClauses[0];
        string existence = statement.IsIfExists ? "IF EXISTS " : string.Empty;
        context.Replace(
            statement,
            "DROP INDEX " + existence + SqliteSyntax.QuoteIdentifier(clause.Index.Value)
        );
    }
}
