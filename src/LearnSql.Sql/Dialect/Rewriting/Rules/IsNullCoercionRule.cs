using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class IsNullCoercionRule : TranslationRule<FunctionCall>
{
    private const int ParameterCount = 2;

    protected override bool AppliesTo(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        if (call.CallTarget is not null || call.Parameters.Count != ParameterCount)
            return false;
        if (!string.Equals(call.FunctionName.Value, "ISNULL", StringComparison.OrdinalIgnoreCase))
            return false;

        ValueKind check = context.Types.Infer(call.Parameters[0]);
        ValueKind replacement = context.Types.Infer(call.Parameters[1]);
        return IsConvertible(check) && IsConvertible(replacement) && check != replacement;
    }

    protected override void Translate(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        ScalarExpression replacement = call.Parameters[1];
        string targetType =
            context.Types.Infer(call.Parameters[0]) == ValueKind.Text ? "TEXT" : "NUMERIC";
        context.InsertBefore(replacement, "CAST(");
        context.InsertAfter(replacement, " AS " + targetType + ")");
    }

    private static bool IsConvertible(ValueKind kind)
    {
        return kind is ValueKind.Text or ValueKind.Numeric;
    }
}
