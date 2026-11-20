using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Unsupported;

namespace LearnSql.Tests.Dialect;

public class UnsupportedConstructCatalogTests
{
    private static readonly IReadOnlyList<UnsupportedConstruct> Catalog =
        UnsupportedConstructs.CreateDefault();

    private readonly TSqlDialectTranslator _translator = TSqlDialectTranslator.CreateDefault();

    public static TheoryData<string> CatalogIds()
    {
        TheoryData<string> data = new TheoryData<string>();
        foreach (UnsupportedConstruct construct in Catalog)
            data.Add(construct.Id);
        return data;
    }

    [Fact]
    public void Catalog_Ids_AreUnique()
    {
        Assert.Equal(
            Catalog.Count,
            Catalog.Select((UnsupportedConstruct c) => c.Id).Distinct().Count()
        );
    }

    [Fact]
    public void Catalog_EveryEntryHasANameAndAnAlternative()
    {
        Assert.All(
            Catalog,
            (UnsupportedConstruct construct) =>
            {
                Assert.False(string.IsNullOrWhiteSpace(construct.Name));
                Assert.False(string.IsNullOrWhiteSpace(construct.Alternative));
            }
        );
    }

    [Fact]
    public void Catalog_LastEntryIsTheCatchAllForUnsupportedStatements()
    {
        Assert.Equal("UnsupportedStatement", Catalog[^1].Id);
    }

    [Fact]
    public void Examples_CoverExactlyTheCatalogEntries()
    {
        Assert.Equal(
            Catalog.Select((UnsupportedConstruct c) => c.Id).Order(),
            UnsupportedConstructExamples.BySqlId.Keys.Order()
        );
    }

    [Theory]
    [MemberData(nameof(CatalogIds))]
    public void Translate_ExampleOfEntry_IsRejectedWithExactlyThatEntry(string id)
    {
        UnsupportedConstruct entry = Catalog.Single((UnsupportedConstruct c) => c.Id == id);
        string sql = UnsupportedConstructExamples.BySqlId[id];

        DialectTranslationResult result = _translator.Translate(sql);

        Assert.False(result.Success);
        Assert.Empty(result.Batches);
        DialectDiagnostic diagnostic = Assert.Single(
            result.Diagnostics,
            (DialectDiagnostic d) => d.Severity == DiagnosticSeverity.Error
        );
        Assert.Equal(id, diagnostic.Code);
        Assert.Equal(DiagnosticKind.UnsupportedConstruct, diagnostic.Kind);
        Assert.Equal(entry.Alternative, diagnostic.Hint);
        Assert.Contains(entry.Name, diagnostic.Message);
        Assert.Null(diagnostic.MsSqlErrorNumber);
        Assert.True(diagnostic.Span.Length > 0);
    }

    [Fact]
    public void Translate_UnsupportedConstructWithUnsupportedChildren_ReportsOnlyTheOuterConstruct()
    {
        const string sql =
            "BEGIN TRY SELECT TOP 1 WITH TIES a FROM T ORDER BY a END TRY BEGIN CATCH SELECT dbo.f(1) END CATCH";

        DialectTranslationResult result = _translator.Translate(sql);

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("TryCatch", diagnostic.Code);
    }

    [Fact]
    public void Translate_TwoIndependentUnsupportedStatements_ReportsBothInSourceOrder()
    {
        DialectTranslationResult result = _translator.Translate("PRINT 'a'; USE master");

        Assert.Equal(
            ["PrintRaiseError", "Use"],
            result.Diagnostics.Select((DialectDiagnostic d) => d.Code)
        );
    }

    [Fact]
    public void Translate_UnsupportedConstructsInDifferentBatches_ReportsEachBatch()
    {
        DialectTranslationResult result = _translator.Translate("PRINT 1\nGO\nUSE x");

        Assert.Equal(
            ["PrintRaiseError", "Use"],
            result.Diagnostics.Select((DialectDiagnostic d) => d.Code)
        );
        Assert.Equal([1, 3], result.Diagnostics.Select((DialectDiagnostic d) => d.Span.Start.Line));
    }

    [Fact]
    public void Translate_SpecificEntry_TakesPrecedenceOverTheCatchAll()
    {
        DialectTranslationResult result = _translator.Translate("CREATE PROCEDURE p AS SELECT 1");

        Assert.DoesNotContain(
            result.Diagnostics,
            (DialectDiagnostic d) => d.Code == "UnsupportedStatement"
        );
        Assert.Contains(result.Diagnostics, (DialectDiagnostic d) => d.Code == "StoredProcedure");
    }

    [Fact]
    public void Translate_UnsupportedExpressionInsideSupportedStatement_IsReportedAtTheExpression()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1, 1 ^ 2");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("BitwiseXor", diagnostic.Code);
        Assert.Equal(11, diagnostic.Span.Start.Column);
        Assert.Equal(16, diagnostic.Span.End.Column);
    }

    [Fact]
    public void Translate_UnsupportedBatchNextToSupportedBatch_SuppressesAllExecution()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO\nUSE x");

        Assert.False(result.Success);
        Assert.Empty(result.Batches);
    }

    [Theory]
    [InlineData("SELECT * FROM dbo.Customer")]
    [InlineData("SELECT * FROM INFORMATION_SCHEMA.TABLES")]
    [InlineData("SELECT * FROM [dbo].[Customer]")]
    [InlineData("SELECT * FROM DBO.Customer")]
    public void Translate_LocalSchemaQualifier_IsNotReportedAsForeignSchema(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);

        Assert.True(result.Success);
    }

    [Theory]
    [InlineData("SELECT * FROM master.dbo.Customer")]
    [InlineData("SELECT * FROM other.Customer")]
    [InlineData("SELECT * FROM srv.db.dbo.Customer")]
    public void Translate_ForeignQualifier_IsReportedAsForeignSchema(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);

        Assert.Equal("ForeignSchema", Assert.Single(result.Diagnostics).Code);
    }
}
