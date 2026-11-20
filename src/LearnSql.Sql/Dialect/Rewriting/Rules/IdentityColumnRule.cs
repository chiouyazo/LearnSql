using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class IdentityColumnRule : TranslationRule<ColumnDefinition>
{
    protected override bool AppliesTo(
        ColumnDefinition column,
        FragmentNode node,
        TranslationContext context
    )
    {
        return column.IdentityOptions is not null && IdentityColumns.IsTranslatable(column);
    }

    protected override void Translate(
        ColumnDefinition column,
        FragmentNode node,
        TranslationContext context
    )
    {
        UniqueConstraintDefinition primaryKey = IdentityColumns.FindPrimaryKey(column)!;
        int next = context.NextSignificantTokenIndex(column.IdentityOptions.LastTokenIndex);

        context.Replace(column.DataType, "INTEGER");
        context.DeleteRange(column.IdentityOptions.StartOffset, context.TokenAt(next).Offset);
        context.InsertAfter(primaryKey, " AUTOINCREMENT");
    }
}
