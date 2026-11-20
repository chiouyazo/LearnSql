using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Tests.Support;

public sealed class DelegateTranslationRule : ITranslationRule
{
    private readonly Action<FragmentNode, TranslationContext> _apply;

    public DelegateTranslationRule(Action<FragmentNode, TranslationContext> apply)
    {
        _apply = apply;
    }

    public void Apply(FragmentNode node, TranslationContext context)
    {
        _apply(node, context);
    }
}
