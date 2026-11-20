using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class ConvertCallRule : TranslationRule<ConvertCall>
{
    protected override void Translate(
        ConvertCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        string? style = call.Style is Literal literal ? literal.Value : null;
        context.Replace(
            call,
            () => SqliteCastBuilder.Build(context.Render(call.Parameter), call.DataType, style)
        );
    }
}
