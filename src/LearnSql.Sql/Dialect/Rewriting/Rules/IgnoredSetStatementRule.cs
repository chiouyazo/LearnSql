using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Resources;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class IgnoredSetStatementRule : ITranslationRule
{
    public void Apply(FragmentNode node, TranslationContext context)
    {
        if (!SetStatementClassifier.IsIgnorable(node.Fragment))
            return;

        string statementText = context.Original(node.Fragment).TrimEnd().TrimEnd(';').TrimEnd();
        context.Replace(node.Fragment, string.Empty);
        context.Report(
            node.Fragment,
            DiagnosticKind.IgnoredConstruct,
            DiagnosticSeverity.Information,
            "IgnoredStatement",
            DialectStrings.IgnoredStatement(statementText)
        );
    }
}
