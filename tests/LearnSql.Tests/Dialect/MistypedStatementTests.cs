using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Unsupported;

namespace LearnSql.Tests.Dialect;

public class MistypedStatementTests
{
    private readonly TSqlDialectTranslator _translator = TSqlDialectTranslator.CreateDefault();

    private DialectDiagnostic Diagnose(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);

        Assert.False(result.Success);
        return Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData("SELEC 1", "SELECT")]
    [InlineData("SELCT 1", "SELECT")]
    [InlineData("SELET 1", "SELECT")]
    [InlineData("SLECT 1", "SELECT")]
    [InlineData("SELECTT 1", "SELECT")]
    [InlineData("selec 1", "SELECT")]
    public void ImplicitExecuteOfAMistypedKeyword_SuggestsTheKeyword(string sql, string keyword)
    {
        DialectDiagnostic diagnostic = Diagnose(sql);

        Assert.Equal("Execute", diagnostic.Code);
        Assert.Equal(DiagnosticKind.UnsupportedConstruct, diagnostic.Kind);
        Assert.StartsWith($"Meintest du {keyword}?", diagnostic.Hint);
    }

    [Theory]
    [InlineData("EXEC SELEC 1")]
    [InlineData("EXECUTE SELCT")]
    [InlineData("EXEC sp_help")]
    [InlineData("sp_help")]
    [InlineData("FOO 1")]
    [InlineData("SEL 1")]
    [InlineData("SELCTT 1")]
    [InlineData("usp_GetCustomers")]
    [InlineData("INSERT INTO T EXEC sp_help")]
    public void OtherExecuteForms_KeepTheGenericAlternative(string sql)
    {
        DialectDiagnostic diagnostic = Diagnose(sql);

        Assert.Equal("Execute", diagnostic.Code);
        Assert.Equal("Schreibe die Anweisung direkt in das Skript.", diagnostic.Hint);
    }

    [Fact]
    public void ImplicitExecuteOfAMistypedKeyword_KeepsTheMessageAndPosition()
    {
        DialectDiagnostic diagnostic = Diagnose("SELEC 2");

        Assert.Equal(
            "Das Konstrukt \"EXEC und EXECUTE\" ist in der Umgebung noch nicht verfügbar.",
            diagnostic.Message
        );
        Assert.Equal(1, diagnostic.Span.Start.Line);
    }

    [Theory]
    [InlineData("SELECT 1;\nSELEC 2", "SELECT", 2)]
    [InlineData("SELECT 1; SELCT 2", "SELECT", 1)]
    [InlineData("SELECT 1;\n\nUPDAT T SET a = 1", "UPDATE", 3)]
    public void MistypedKeywordAtTheStartOfALaterStatement_GetsAHintOnTheSyntaxError(
        string sql,
        string keyword,
        int line
    )
    {
        DialectDiagnostic diagnostic = Diagnose(sql);

        Assert.Equal(DiagnosticKind.ParseError, diagnostic.Kind);
        Assert.Equal(102, diagnostic.MsSqlErrorNumber);
        Assert.Equal($"Meintest du {keyword}?", diagnostic.Hint);
        Assert.Equal(line, diagnostic.Span.Start.Line);
    }

    [Theory]
    [InlineData("SELECT 1 FROM")]
    [InlineData("SELECT 1; SELECT FROM")]
    [InlineData("SELECT 1; FOO BAR")]
    [InlineData("SELECT 1 SELEC 2")]
    [InlineData("SELECT * FROM T WHERE SELEC")]
    public void SyntaxErrorsAwayFromAStatementStart_HaveNoHint(string sql)
    {
        DialectTranslationResult result = _translator.Translate(sql);

        Assert.False(result.Success);
        Assert.All(result.Errors, (DialectDiagnostic diagnostic) => Assert.Null(diagnostic.Hint));
    }

    [Theory]
    [InlineData("SELECT", "SELECT", false)]
    [InlineData("SELEC", "SELECT", true)]
    [InlineData("SELECTT", "SELECT", true)]
    [InlineData("SELECTTT", "SELECT", false)]
    [InlineData("SLEECT", "SELECT", true)]
    [InlineData("SLEEECT", "SELECT", false)]
    [InlineData("SELECZ", "SELECT", true)]
    [InlineData("SEL", "SELECT", false)]
    [InlineData("DROPP", "DROP", true)]
    [InlineData("DRP", "DROP", false)]
    public void FindKeyword_AcceptsExactlyOneEdit(string word, string keyword, bool expected)
    {
        string? found = MistypedStatementSuggestion.FindKeyword(word);

        Assert.Equal(expected ? keyword : null, found);
    }

    [Fact]
    public void FindKeyword_DoesNotSuggestAKeywordForItself()
    {
        foreach (string keyword in new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DECLARE" })
            Assert.Null(MistypedStatementSuggestion.FindKeyword(keyword));
    }

    [Fact]
    public void Catalog_OnlyTheExecuteEntryCarriesASuggestion()
    {
        IEnumerable<string> ids = UnsupportedConstructs
            .CreateDefault()
            .Where((UnsupportedConstruct construct) => construct.Suggest is not null)
            .Select((UnsupportedConstruct construct) => construct.Id);

        Assert.Equal(["Execute"], ids);
    }
}
