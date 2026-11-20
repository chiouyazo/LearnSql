using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class TruncateTableRule : TranslationRule<TruncateTableStatement>
{
    protected override void Translate(
        TruncateTableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(statement, () => "DELETE FROM " + context.Render(statement.TableName));
    }
}
