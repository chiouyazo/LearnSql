using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class CastCallRule : TranslationRule<CastCall>
{
    protected override void Translate(CastCall call, FragmentNode node, TranslationContext context)
    {
        context.Replace(
            call,
            () => SqliteCastBuilder.Build(context.Render(call.Parameter), call.DataType)
        );
    }
}
