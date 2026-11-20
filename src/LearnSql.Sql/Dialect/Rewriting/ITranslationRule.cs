using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Dialect.Rewriting;

public interface ITranslationRule
{
    void Apply(FragmentNode node, TranslationContext context);
}
