using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting;

public abstract class TranslationRule<TFragment> : ITranslationRule
    where TFragment : TSqlFragment
{
    public void Apply(FragmentNode node, TranslationContext context)
    {
        if (node.Fragment is TFragment fragment && AppliesTo(fragment, node, context))
            Translate(fragment, node, context);
    }

    protected virtual bool AppliesTo(
        TFragment fragment,
        FragmentNode node,
        TranslationContext context
    )
    {
        return true;
    }

    protected abstract void Translate(
        TFragment fragment,
        FragmentNode node,
        TranslationContext context
    );
}
