using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Parsing;

namespace LearnSql.Tests.Dialect;

public class MsSqlValidatorTests
{
    [Theory]
    [InlineData("SELECT TOP 5 * FROM Customer")]
    [InlineData("SELECT [Name] FROM [dbo].[Customer]")]
    [InlineData("SELECT 1\nGO\nSELECT 2")]
    [InlineData("")]
    public void Validate_ValidTSql_ReturnsNoDiagnostics(string sql)
    {
        Assert.Empty(MsSqlValidator.Validate(sql));
    }

    [Fact]
    public void Validate_SelectWithoutColumns_ReturnsASyntaxError()
    {
        IReadOnlyList<DialectDiagnostic> diagnostics = MsSqlValidator.Validate("SELECT FROM");

        DialectDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticKind.ParseError, diagnostic.Kind);
        Assert.Equal(102, diagnostic.MsSqlErrorNumber);
        Assert.Contains("FROM", diagnostic.Message);
    }

    [Fact]
    public void Validate_ReservedWordInDoubleQuotes_IsAnIdentifierBecauseQuotedIdentifiersAreOn()
    {
        IReadOnlyList<DialectDiagnostic> diagnostics = MsSqlValidator.Validate(
            "CREATE TABLE \"select\" (\"from\" INT)"
        );

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Validate_ErrorInSecondBatch_ReportsThePositionInTheWholeText()
    {
        IReadOnlyList<DialectDiagnostic> diagnostics = MsSqlValidator.Validate(
            "SELECT 1\nGO\nSELECT FROM"
        );

        DialectDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(8, diagnostic.Span.Start.Column);
    }

    [Fact]
    public void Validate_ErrorsInEveryBatch_AreCollected()
    {
        IReadOnlyList<DialectDiagnostic> diagnostics = MsSqlValidator.Validate(
            "SELECT FROM\nGO\nSELECT FROM"
        );

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void Validate_DoesNotApplyTranslatorRestrictions()
    {
        Assert.Empty(MsSqlValidator.Validate("USE master"));
    }
}
