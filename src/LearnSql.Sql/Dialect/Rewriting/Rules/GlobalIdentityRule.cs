using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class GlobalIdentityRule : TranslationRule<GlobalVariableExpression>
{
    protected override bool AppliesTo(
        GlobalVariableExpression variable,
        FragmentNode node,
        TranslationContext context
    )
    {
        return string.Equals(variable.Name, "@@IDENTITY", StringComparison.OrdinalIgnoreCase);
    }

    protected override void Translate(
        GlobalVariableExpression variable,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(variable, InternalFunctionNames.LastInsertRowId + "()");
    }
}
