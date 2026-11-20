using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class CommitTransactionRule : TranslationRule<CommitTransactionStatement>
{
    protected override bool AppliesTo(
        CommitTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return TransactionNames.IsPlain(statement);
    }

    protected override void Translate(
        CommitTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(statement, "COMMIT");
    }
}
