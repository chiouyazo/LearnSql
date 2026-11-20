using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class PrimaryKeyClusteringRule : TranslationRule<UniqueConstraintDefinition>
{
    protected override bool AppliesTo(
        UniqueConstraintDefinition constraint,
        FragmentNode node,
        TranslationContext context
    )
    {
        return constraint.Clustered is not null;
    }

    protected override void Translate(
        UniqueConstraintDefinition constraint,
        FragmentNode node,
        TranslationContext context
    )
    {
        ClusteringKeywords.RemoveFrom(context, constraint);
    }
}
