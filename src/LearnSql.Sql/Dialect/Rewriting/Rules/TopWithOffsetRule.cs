using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting.Rules;

public sealed class TopWithOffsetRule : TranslationRule<QuerySpecification>
{
    private const int TopWithOffsetErrorNumber = 10741;

    protected override bool AppliesTo(
        QuerySpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        return specification.TopRowFilter is not null && specification.OffsetClause is not null;
    }

    protected override void Translate(
        QuerySpecification specification,
        FragmentNode node,
        TranslationContext context
    )
    {
        context.Report(
            specification.TopRowFilter,
            DiagnosticKind.TranslationError,
            DiagnosticSeverity.Error,
            "TopWithOffset",
            "A TOP can not be used in the same query or sub-query as a OFFSET.",
            null,
            TopWithOffsetErrorNumber
        );
    }
}
