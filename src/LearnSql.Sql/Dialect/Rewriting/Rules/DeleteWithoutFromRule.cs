using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DeleteWithoutFromRule : TranslationRule<DeleteSpecification>
{
    protected override bool AppliesTo(
        DeleteSpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        if (specification.FromClause is not null)
            return false;

        int next = context.NextSignificantTokenIndex(specification.FirstTokenIndex);
        return context.TokenAt(next).TokenType != TSqlTokenType.From;
    }

    protected override void Translate(
        DeleteSpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.InsertBefore(specification.Target, "FROM ");
    }
}
