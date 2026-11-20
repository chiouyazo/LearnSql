using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class VariableColumnNameRule : TranslationRule<SelectScalarExpression>
{
    private const string UnnamedColumn = "(No column name)";

    protected override bool AppliesTo(
        SelectScalarExpression element,
        FragmentNode node,
        TranslationContext context
    )
    {
        return element.ColumnName is null && element.Expression is VariableReference;
    }

    protected override void Translate(
        SelectScalarExpression element,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.InsertAfter(element, " AS " + SqliteSyntax.QuoteIdentifier(UnnamedColumn));
    }
}
