using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class CurrentTimestampRule : TranslationRule<ParameterlessCall>
{
    protected override bool AppliesTo(
        ParameterlessCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        return call.ParameterlessCallType == ParameterlessCallType.CurrentTimestamp;
    }

    protected override void Translate(
        ParameterlessCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(call, InternalFunctionNames.GetDate + "()");
    }
}
