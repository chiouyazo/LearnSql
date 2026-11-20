using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class DatePartArgumentRule : TranslationRule<FunctionCall>
{
    private static readonly HashSet<string> DatePartFunctions = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "DATEADD",
        "DATEDIFF",
        "DATEDIFF_BIG",
        "DATEPART",
        "DATENAME",
    };

    protected override bool AppliesTo(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        return call.CallTarget is null
            && call.Parameters.Count > 0
            && DatePartFunctions.Contains(call.FunctionName.Value)
            && ReadDatePartName(call.Parameters[0]) is not null;
    }

    protected override void Translate(
        FunctionCall call,
        FragmentNode node,
        TranslationContext context
    )
    {
        string name = ReadDatePartName(call.Parameters[0])!;
        context.Replace(call.Parameters[0], SqliteSyntax.QuoteString(name.ToLowerInvariant()));
    }

    private static string? ReadDatePartName(ScalarExpression parameter)
    {
        if (parameter is IdentifierLiteral identifier)
            return identifier.Value;

        if (parameter is ColumnReferenceExpression { MultiPartIdentifier.Count: 1 } column)
            return column.MultiPartIdentifier.Identifiers[0].Value;

        return null;
    }
}
