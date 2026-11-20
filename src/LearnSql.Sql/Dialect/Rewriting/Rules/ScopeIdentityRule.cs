using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class ScopeIdentityRule : TranslationRule<FunctionCall>
{
    protected override bool AppliesTo(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        return call.CallTarget is null
            && call.Parameters.Count == 0
            && string.Equals(
                call.FunctionName.Value,
                "SCOPE_IDENTITY",
                StringComparison.OrdinalIgnoreCase
            );
    }

    protected override void Translate(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(call, InternalFunctionNames.LastInsertRowId + "()");
    }
}
