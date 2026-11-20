using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Dialect.Rewriting;

public sealed class TranslationEngine
{
    private readonly IReadOnlyList<ITranslationRule> _rules;

    public TranslationEngine(IReadOnlyList<ITranslationRule> rules)
    {
        _rules = rules;
    }

    public void Run(TranslationContext context)
    {
        foreach (FragmentNode node in context.Tree.Nodes)
        {
            foreach (ITranslationRule rule in _rules)
                rule.Apply(node, context);
        }
    }
}
