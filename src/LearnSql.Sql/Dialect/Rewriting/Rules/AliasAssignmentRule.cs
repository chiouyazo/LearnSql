using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class AliasAssignmentRule : TranslationRule<SelectScalarExpression>
{
    protected override bool AppliesTo(
        SelectScalarExpression element,
        FragmentNode node,
        TranslationContext context
    )
    {
        return element.ColumnName is not null
            && element.ColumnName.FirstTokenIndex < element.Expression.FirstTokenIndex;
    }

    protected override void Translate(
        SelectScalarExpression element,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.DeleteRange(element.ColumnName.StartOffset, element.Expression.StartOffset);
        context.InsertAfter(
            element.Expression,
            " AS " + SqliteSyntax.QuoteIdentifier(element.ColumnName.Value)
        );
    }
}
