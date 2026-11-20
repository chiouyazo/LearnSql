using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class ColumnSchemaQualifierRule : TranslationRule<MultiPartIdentifier>
{
    private const int SchemaTableColumnParts = 3;

    protected override bool AppliesTo(
        MultiPartIdentifier identifier,
        FragmentNode node,
        TranslationContext context
    )
    {
        return identifier is not SchemaObjectName
            && identifier.Count == SchemaTableColumnParts
            && SandboxSchemas.IsLocal(identifier.Identifiers[0].Value);
    }

    protected override void Translate(
        MultiPartIdentifier identifier,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.DeleteRange(
            identifier.Identifiers[0].StartOffset,
            identifier.Identifiers[1].StartOffset
        );
    }
}
