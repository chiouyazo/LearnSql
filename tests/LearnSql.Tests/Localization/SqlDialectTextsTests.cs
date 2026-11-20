using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Hints;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Dialect.Unsupported;
using LearnSql.Tests.Dialect;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Localization;

public class SqlDialectTextsTests
{
    private readonly TSqlDialectTranslator _translator = TSqlDialectTranslator.CreateDefault();

    private DialectDiagnostic Single(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);
        return Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void UnsupportedConstruct_InEnglish_ReportsTheEnglishMessageAndAlternative()
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectDiagnostic diagnostic = Single(
            UnsupportedConstructExamples.BySqlId["CrossOuterApply"]
        );

        Assert.Equal("CrossOuterApply", diagnostic.Code);
        Assert.Equal(
            "\"CROSS APPLY and OUTER APPLY\" is not available in this environment yet.",
            diagnostic.Message
        );
        Assert.Equal("Use a JOIN or a correlated subquery instead.", diagnostic.Hint);
    }

    [Fact]
    public void UnsupportedConstruct_InGerman_StaysGerman()
    {
        using UiCulture culture = UiCulture.Use("de");

        DialectDiagnostic diagnostic = Single(
            UnsupportedConstructExamples.BySqlId["CrossOuterApply"]
        );

        Assert.Equal(
            "Das Konstrukt \"CROSS APPLY und OUTER APPLY\" ist in der Umgebung noch nicht verfügbar.",
            diagnostic.Message
        );
        Assert.Equal(
            "Verwende stattdessen einen JOIN oder eine korrelierte Unterabfrage.",
            diagnostic.Hint
        );
    }

    [Fact]
    public void UnsupportedConstruct_WithAFormattedAlternative_ListsTheSupportedStyleCodes()
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectDiagnostic diagnostic = Single(UnsupportedConstructExamples.BySqlId["ConvertStyle"]);

        Assert.StartsWith("Use one of the supported style codes (0, 1,", diagnostic.Hint);
        Assert.EndsWith(") or the FORMAT function.", diagnostic.Hint);
    }

    [Fact]
    public void Catalog_ResolvesItsTextsOnEveryAccess_SoALanguageChangeTakesEffect()
    {
        IReadOnlyList<UnsupportedConstruct> catalog = UnsupportedConstructs.CreateDefault();
        UnsupportedConstruct entry = catalog.Single((UnsupportedConstruct c) => c.Id == "Merge");
        string german;
        string english;

        using (UiCulture culture = UiCulture.Use("de"))
        {
            german = entry.Alternative;
        }

        using (UiCulture culture = UiCulture.Use("en"))
        {
            english = entry.Alternative;
        }

        Assert.Equal("Verwende getrennte INSERT-, UPDATE- und DELETE-Anweisungen.", german);
        Assert.Equal("Use separate INSERT, UPDATE and DELETE statements.", english);
    }

    [Fact]
    public void Topics_ResolveTheirTextsOnEveryAccess_SoALanguageChangeTakesEffect()
    {
        IReadOnlyList<UnsupportedConstruct> catalog = UnsupportedConstructs.CreateDefault();
        IReadOnlyList<DialectHintTopic> topics = DialectDifferences.CreateDefault(catalog);
        DialectHintTopic dataTypes = topics[0];
        DialectHintTopic notYetAvailable = topics[^1];
        string germanTitle;
        string germanPoint;

        using (UiCulture culture = UiCulture.Use("de"))
        {
            germanTitle = dataTypes.Title;
            germanPoint = notYetAvailable.Points[0];
        }

        using (UiCulture culture = UiCulture.Use("en"))
        {
            Assert.NotEqual(germanTitle, dataTypes.Title);
            Assert.Equal("Data types and dynamic typing", dataTypes.Title);
            Assert.NotEqual(germanPoint, notYetAvailable.Points[0]);
            Assert.Equal(
                "CROSS APPLY and OUTER APPLY. Use a JOIN or a correlated subquery instead.",
                notYetAvailable.Points[0]
            );
            Assert.Equal("T-SQL constructs that are not available yet", notYetAvailable.Title);
        }
    }

    [Fact]
    public void Topic_InEnglish_TranslatesPointsAndLocalizedExampleResults()
    {
        using UiCulture culture = UiCulture.Use("en");
        DialectHintTopic topic = DialectDifferences
            .CreateDefault(UnsupportedConstructs.CreateDefault())
            .Single((DialectHintTopic t) => t.Id == "DataTypes");

        Assert.StartsWith("SQLite checks types less strictly", topic.Summary);
        Assert.Equal(7, topic.Points.Count);
        Assert.Equal("Error 245 (Conversion failed)", topic.Examples[0].MsSqlResult);
        Assert.Equal("The text abc is stored without an error", topic.Examples[0].SandboxResult);
        Assert.Equal("2147483648", topic.Examples[1].SandboxResult);
    }

    [Theory]
    [InlineData("SELEC 1")]
    [InlineData("selec 1")]
    public void MistypedKeyword_AsImplicitExecute_SuggestsTheKeywordInEnglish(string sql)
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectDiagnostic diagnostic = Single(sql);

        Assert.Equal("Execute", diagnostic.Code);
        Assert.Equal(
            "Did you mean SELECT? T-SQL reads an unknown word at the start of a statement as a procedure call, so it is reported as EXEC.",
            diagnostic.Hint
        );
    }

    [Fact]
    public void MistypedKeyword_AsParseError_SuggestsTheKeywordInEnglish()
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectTranslationResult result = _translator.Translate("SELECT 1;\nSELEC 2");

        DialectDiagnostic diagnostic = Assert.Single(result.Errors);
        Assert.Equal("Did you mean SELECT?", diagnostic.Hint);
    }

    [Fact]
    public void IgnoredSetStatement_InEnglish_ExplainsThatItIsSkipped()
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectDiagnostic diagnostic = Single("SET NOCOUNT ON;\nSELECT 1");

        Assert.Equal("IgnoredStatement", diagnostic.Code);
        Assert.Equal(DiagnosticSeverity.Information, diagnostic.Severity);
        Assert.Equal(
            "The statement \"SET NOCOUNT ON\" has no effect in this environment and is skipped.",
            diagnostic.Message
        );
    }

    [Fact]
    public void IgnoredTableHint_InEnglish_ExplainsThatItIsRemoved()
    {
        using UiCulture culture = UiCulture.Use("en");

        DialectDiagnostic diagnostic = Single("SELECT * FROM Customer WITH (NOLOCK)");

        Assert.Equal("IgnoredTableHint", diagnostic.Code);
        Assert.Equal(
            "Table hints such as NOLOCK have no effect in this environment and are removed.",
            diagnostic.Message
        );
    }

    [Fact]
    public void InternalTranslationError_InEnglish_AsksToRephraseTheStatement()
    {
        using UiCulture culture = UiCulture.Use("en");
        TSqlDialectTranslator translator = new TSqlDialectTranslator(
            SchemaCatalog.Empty,
            new List<UnsupportedConstruct>(),
            [
                new DelegateTranslationRule(
                    (FragmentNode node, TranslationContext context) =>
                        throw new InvalidOperationException("boom")
                ),
            ]
        );

        DialectTranslationResult result = translator.Translate("SELECT 1");

        DialectDiagnostic diagnostic = Assert.Single(result.Errors);
        Assert.Equal("InternalTranslationError", diagnostic.Code);
        Assert.Equal(
            "The statement could not be translated to SQLite. Rephrase it or report the problem.",
            diagnostic.Message
        );
    }

    [Fact]
    public void MsSqlMessages_StayEnglishInGerman()
    {
        using UiCulture culture = UiCulture.Use("de");

        DialectTranslationResult result = _translator.Translate("SELECT @x");

        DialectDiagnostic diagnostic = Assert.Single(result.Errors);
        Assert.Equal("Must declare the scalar variable \"@x\".", diagnostic.Message);
    }
}
