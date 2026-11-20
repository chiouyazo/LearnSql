using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Tests.Support;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Tests.Dialect;

public class RuleConflictTests
{
    private static TSqlDialectTranslator CreateTranslator(params ITranslationRule[] rules) =>
        new TSqlDialectTranslator(SchemaCatalog.Empty, [], rules);

    private static ITranslationRule OverlappingReplacements() =>
        new DelegateTranslationRule(
            (node, context) =>
            {
                if (node.Fragment is not SelectStatement)
                    return;

                context.ReplaceRange(0, 5, "AAAAA");
                context.ReplaceRange(3, 8, "BBBBB");
            }
        );

    [Fact]
    public void Translate_PartiallyOverlappingEdits_ReportsAnInternalTranslationError()
    {
        TSqlDialectTranslator translator = CreateTranslator(OverlappingReplacements());

        DialectTranslationResult result = translator.Translate("SELECT 1");

        Assert.False(result.Success);
        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InternalTranslationError", diagnostic.Code);
        Assert.Equal(DiagnosticKind.TranslationError, diagnostic.Kind);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.NotNull(diagnostic.Hint);
        Assert.Null(diagnostic.MsSqlErrorNumber);
        Assert.Empty(result.Batches);
    }

    [Fact]
    public void Translate_InternalTranslationError_CoversTheWholeBatch()
    {
        TSqlDialectTranslator translator = CreateTranslator(OverlappingReplacements());

        DialectTranslationResult result = translator.Translate("SET NOCOUNT ON\nGO\nSELECT 1");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal("SET NOCOUNT ON\nGO\n".Length, diagnostic.Span.Start.Offset);
        Assert.Equal("SET NOCOUNT ON\nGO\nSELECT 1".Length, diagnostic.Span.End.Offset);
    }

    [Fact]
    public void Translate_RuleThrowingInvalidOperation_IsReportedInsteadOfEscaping()
    {
        TSqlDialectTranslator translator = CreateTranslator(
            new DelegateTranslationRule(
                (node, context) =>
                {
                    if (node.Fragment is SelectStatement)
                        throw new InvalidOperationException("rule failed");
                }
            )
        );

        DialectTranslationResult result = translator.Translate("SELECT 1");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("InternalTranslationError", diagnostic.Code);
        Assert.Equal("rule failed", diagnostic.Hint);
    }

    [Fact]
    public void Translate_NestedEditsOfTheSameRegion_AreComposedWithoutConflict()
    {
        TSqlDialectTranslator translator = CreateTranslator(
            new DelegateTranslationRule(
                (node, context) =>
                {
                    if (node.Fragment is IntegerLiteral literal && literal.Value == "1")
                        context.Replace(literal, "2");
                    if (node.Fragment is SelectStatement select)
                        context.InsertBefore(select, "/* x */ ");
                }
            )
        );

        DialectTranslationResult result = translator.Translate("SELECT 1");

        Assert.True(result.Success);
        Assert.Equal(
            "/* x */ SELECT 2",
            Assert.Single(Assert.Single(result.Batches).Statements).Sql
        );
    }

    [Fact]
    public void Translate_WithoutRulesAndCatalog_ReturnsTheTrimmedStatementsUnchanged()
    {
        TSqlDialectTranslator translator = new TSqlDialectTranslator(
            SchemaCatalog.Empty,
            new List<UnsupportedConstruct>(),
            []
        );

        DialectTranslationResult result = translator.Translate("  SELECT 1 ;  ");

        Assert.Equal("SELECT 1", Assert.Single(Assert.Single(result.Batches).Statements).Sql);
    }

    [Fact]
    public void Translate_AllDefaultRulesTogether_DoNotConflictOnACombinedStatement()
    {
        TSqlDialectTranslator translator = TSqlDialectTranslator.CreateDefault();

        DialectTranslationResult result = translator.Translate(
            "SELECT TOP 2 [Name] + 'x', ISNULL(NULL, 1), DATEADD(day, 1, GETDATE()) FROM [dbo].[T] WHERE [a] <> 1 ORDER BY 1"
        );

        Assert.True(result.Success);
        Assert.DoesNotContain(
            result.Diagnostics,
            (DialectDiagnostic d) => d.Code == "InternalTranslationError"
        );
    }
}
