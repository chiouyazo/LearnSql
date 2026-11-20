using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DataTypeMaxRule : TranslationRule<SqlDataTypeReference>
{
    public const string MaxMarker = " MAX";

    protected override bool AppliesTo(
        SqlDataTypeReference type,
        FragmentNode node,
        TranslationContext context
    )
    {
        bool hasMax = type.Parameters.Any(parameter => parameter is MaxLiteral);
        bool handledByCast =
            node.Parent?.Fragment is CastCall or ConvertCall or TryCastCall or TryConvertCall;
        return hasMax && !handledByCast;
    }

    protected override void Translate(
        SqlDataTypeReference type,
        FragmentNode node,
        TranslationContext context
    )
    {
        string original = context.Original(type);
        int parenthesis = original.IndexOf('(', StringComparison.Ordinal);
        string typeName = parenthesis > 0 ? original.Substring(0, parenthesis).TrimEnd() : original;
        context.Replace(type, typeName + MaxMarker);
    }
}
