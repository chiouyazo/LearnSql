using LearnSql.Core.Services.Guided;

namespace LearnSql.Tests.Core;

public sealed class GuidedSqlTests
{
    [Theory]
    [InlineData("SELECT 1;", "SELECT 1")]
    [InlineData("  SELECT   1 ;  ", "SELECT 1")]
    [InlineData("SELECT\r\n  1\n;", "SELECT 1")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Normalize_CollapsesWhitespaceAndTrailingSemicolon(string input, string expected)
    {
        Assert.Equal(expected, GuidedSql.Normalize(input));
    }

    [Fact]
    public void Normalize_Null_IsEmpty()
    {
        Assert.Equal(string.Empty, GuidedSql.Normalize(null));
    }

    [Fact]
    public void Matches_IgnoresLayoutButNotContent()
    {
        Assert.True(GuidedSql.Matches("SELECT *\n FROM Customer;", "SELECT * FROM Customer"));
        Assert.False(GuidedSql.Matches("SELECT * FROM Customer", "select * from Customer"));
        Assert.False(GuidedSql.Matches("SELECT 1", "SELECT 2"));
    }

    [Fact]
    public void Tokenize_ClassifiesKeywordsFunctionsStringsAndNumbers()
    {
        IReadOnlyList<GuidedSqlToken> tokens = GuidedSqlTokenizer.Tokenize(
            "SELECT COUNT(*), 'it''s', 42 FROM [My Table]"
        );

        Assert.Contains(new GuidedSqlToken("SELECT", GuidedSqlTokenKind.Keyword), tokens);
        Assert.Contains(new GuidedSqlToken("COUNT", GuidedSqlTokenKind.Function), tokens);
        Assert.Contains(new GuidedSqlToken("'it''s'", GuidedSqlTokenKind.String), tokens);
        Assert.Contains(new GuidedSqlToken("42", GuidedSqlTokenKind.Number), tokens);
        Assert.Contains(new GuidedSqlToken("FROM", GuidedSqlTokenKind.Keyword), tokens);
        Assert.Contains(new GuidedSqlToken("[My Table]", GuidedSqlTokenKind.Identifier), tokens);
    }

    [Fact]
    public void Tokenize_ConcatenatedTokensReproduceInput()
    {
        const string sql = "SELECT a.Id, 'x y' AS [N]\nFROM T a\nWHERE a.Id >= 3.5;";

        string joined = string.Concat(
            GuidedSqlTokenizer.Tokenize(sql).Select((GuidedSqlToken token) => token.Text)
        );

        Assert.Equal(sql, joined);
    }

    [Fact]
    public void Matches_MultilineIndentedSql_EqualsSingleLineSql()
    {
        const string multiline =
            "SELECT c.FirstName,\n       COUNT(o.Id) AS Orders\nFROM Customer AS c\n    LEFT JOIN [Order] AS o\n        ON o.CustomerId = c.Id\nGROUP BY c.FirstName;";
        const string singleLine =
            "SELECT c.FirstName, COUNT(o.Id) AS Orders FROM Customer AS c LEFT JOIN [Order] AS o ON o.CustomerId = c.Id GROUP BY c.FirstName";

        Assert.True(GuidedSql.Matches(multiline, singleLine));
    }

    [Fact]
    public void Tokenize_MultilineSql_KeepsLineBreaksAndIndentation()
    {
        const string sql = "SELECT COUNT\n    (*)\nFROM Customer\n    WHERE Id = 1;";

        IReadOnlyList<GuidedSqlToken> tokens = GuidedSqlTokenizer.Tokenize(sql);

        Assert.Equal(sql, string.Concat(tokens.Select((GuidedSqlToken token) => token.Text)));
        Assert.Contains(new GuidedSqlToken("COUNT", GuidedSqlTokenKind.Function), tokens);
        Assert.Contains(new GuidedSqlToken("WHERE", GuidedSqlTokenKind.Keyword), tokens);
    }

    [Fact]
    public void Tokenize_UnterminatedString_DoesNotThrow()
    {
        IReadOnlyList<GuidedSqlToken> tokens = GuidedSqlTokenizer.Tokenize("SELECT 'open");

        Assert.Equal(GuidedSqlTokenKind.String, tokens[^1].Kind);
    }

    [Fact]
    public void Tokenize_Empty_ReturnsNoTokens()
    {
        Assert.Empty(GuidedSqlTokenizer.Tokenize(null));
        Assert.Empty(GuidedSqlTokenizer.Tokenize(string.Empty));
    }
}
