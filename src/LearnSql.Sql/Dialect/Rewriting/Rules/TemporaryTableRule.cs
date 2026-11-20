using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class TemporaryTableRule : TranslationRule<CreateTableStatement>
{
    protected override bool AppliesTo(
        CreateTableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        return TemporaryTables.IsTemporary(statement.SchemaObjectName);
    }

    protected override void Translate(
        CreateTableStatement statement,
        FragmentNode node,
        TranslationContext context
    )
    {
        TSqlParserToken create = context.TokenAt(statement.FirstTokenIndex);
        context.ReplaceToken(create, create.Text + " TEMP");
    }
}
