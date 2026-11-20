using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class SchemaObjectNameRule : TranslationRule<SchemaObjectName>
{
    protected override bool AppliesTo(
        SchemaObjectName name,
        FragmentNode node,
        TranslationContext context
    )
    {
        return name.SchemaIdentifier is not null
            && name.DatabaseIdentifier is null
            && name.ServerIdentifier is null
            && SandboxSchemas.IsLocal(name.SchemaIdentifier.Value);
    }

    protected override void Translate(
        SchemaObjectName name,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.DeleteRange(name.SchemaIdentifier.StartOffset, name.BaseIdentifier.StartOffset);
    }
}
