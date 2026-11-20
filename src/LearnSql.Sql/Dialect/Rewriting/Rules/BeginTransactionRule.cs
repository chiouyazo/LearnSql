using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class BeginTransactionRule : TranslationRule<BeginTransactionStatement>
{
    protected override bool AppliesTo(
        BeginTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return TransactionNames.IsPlain(statement)
            && !statement.Distributed
            && !statement.MarkDefined;
    }

    protected override void Translate(
        BeginTransactionStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(statement, "BEGIN");
    }
}
