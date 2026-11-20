using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class IdentifierRule : TranslationRule<Identifier>
{
    private static readonly HashSet<string> SqliteOnlyKeywords = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "AUTOINCREMENT",
        "DEFERRABLE",
        "FILTER",
        "GLOB",
        "INDEXED",
        "LIMIT",
        "NATURAL",
        "NOTHING",
        "NOTNULL",
        "REGEXP",
        "RETURNING",
        "WINDOW",
    };

    protected override bool AppliesTo(
        Identifier identifier,
        FragmentNode node,
        TranslationContext context
    )
    {
        switch (identifier.QuoteType)
        {
            case QuoteType.SquareBracket:
                return true;
            case QuoteType.NotQuoted:
                return identifier.Value.StartsWith('#')
                    || SqliteOnlyKeywords.Contains(identifier.Value);
            default:
                return false;
        }
    }

    protected override void Translate(
        Identifier identifier,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Replace(identifier, SqliteSyntax.QuoteIdentifier(identifier.Value));
    }
}
