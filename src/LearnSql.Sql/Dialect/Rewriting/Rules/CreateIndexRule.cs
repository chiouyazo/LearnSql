using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class CreateIndexRule : TranslationRule<CreateIndexStatement>
{
    protected override bool AppliesTo(
        CreateIndexStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return statement.Clustered is not null;
    }

    protected override void Translate(
        CreateIndexStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        ClusteringKeywords.RemoveFrom(context, statement);
    }
}
