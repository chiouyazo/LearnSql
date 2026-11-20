using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Resources;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class TableHintRule : TranslationRule<NamedTableReference>
{
    protected override bool AppliesTo(
        NamedTableReference reference,
        FragmentNode node,
        TranslationContext context
    )
    {
        return reference.TableHints.Count > 0
            && reference.TableHints.All(TableHintClassifier.IsIgnorable);
    }

    protected override void Translate(
        NamedTableReference reference,
        FragmentNode node,
        TranslationContext context
    )
    {
        TableHint first = reference.TableHints[0];
        TableHint last = reference.TableHints[reference.TableHints.Count - 1];

        int openIndex = context.PreviousSignificantTokenIndex(first.FirstTokenIndex);
        int startIndex = openIndex;
        int beforeOpen = context.PreviousSignificantTokenIndex(openIndex);
        if (beforeOpen >= 0 && context.TokenAt(beforeOpen).TokenType == TSqlTokenType.With)
            startIndex = beforeOpen;

        int closeIndex = context.NextSignificantTokenIndex(last.LastTokenIndex);
        int end = context.EndOfToken(closeIndex);

        context.DeleteRange(context.TokenAt(startIndex).Offset, end);
        context.Report(
            reference,
            DiagnosticKind.IgnoredConstruct,
            DiagnosticSeverity.Information,
            "IgnoredTableHint",
            DialectStrings.IgnoredTableHint
        );
    }
}
