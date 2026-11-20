using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class SelectIntoRule : TranslationRule<SelectStatement>
{
    protected override bool AppliesTo(
        SelectStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return statement.Into is not null;
    }

    protected override void Translate(
        SelectStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        SchemaObjectName target = statement.Into;
        string temporary = TemporaryTables.IsTemporary(target) ? "TEMP " : string.Empty;

        context.InsertBefore(
            statement,
            () => "CREATE " + temporary + "TABLE " + context.Render(target) + " AS "
        );

        int intoIndex = context.PreviousSignificantTokenIndex(target.FirstTokenIndex);
        int followingIndex = context.NextSignificantTokenIndex(target.LastTokenIndex);
        context.DeleteRange(
            context.TokenAt(intoIndex).Offset,
            context.TokenAt(followingIndex).Offset
        );
    }
}
