using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Resources;

namespace LearnSql.Sql.Dialect.Unsupported;

public sealed class UnsupportedConstructDetector
{
    private readonly IReadOnlyList<UnsupportedConstruct> _catalog;

    public UnsupportedConstructDetector(IReadOnlyList<UnsupportedConstruct> catalog)
    {
        _catalog = catalog;
    }

    public IReadOnlyList<DialectDiagnostic> Detect(FragmentTree tree, FragmentLocator locator)
    {
        List<DialectDiagnostic> diagnostics = [];
        HashSet<FragmentNode> flagged = [];

        foreach (FragmentNode node in tree.Nodes)
        {
            if (node.Ancestors().Any(flagged.Contains))
                continue;

            UnsupportedConstruct? match = _catalog.FirstOrDefault(entry =>
                entry.Matches(node.Fragment)
            );
            if (match is null)
                continue;

            flagged.Add(node);
            diagnostics.Add(CreateDiagnostic(match, node, locator));
        }

        return diagnostics;
    }

    private static DialectDiagnostic CreateDiagnostic(
        UnsupportedConstruct entry,
        FragmentNode node,
        FragmentLocator locator
    )
    {
        return new DialectDiagnostic(
            DiagnosticKind.UnsupportedConstruct,
            DiagnosticSeverity.Error,
            entry.Id,
            DialectStrings.ConstructNotAvailable(entry.Name),
            entry.Suggest?.Invoke(node.Fragment) ?? entry.Alternative,
            locator.GetSpan(node.Fragment),
            null
        );
    }
}
