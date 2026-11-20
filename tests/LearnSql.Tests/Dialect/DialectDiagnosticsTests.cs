using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Extensions;

namespace LearnSql.Tests.Dialect;

public class DialectDiagnosticsTests
{
    private readonly TSqlDialectTranslator _translator = TSqlDialectTranslator.CreateDefault();

    [Fact]
    public void Translate_ParseErrorInSecondBatch_ReportsLineAndColumnRelativeToTheWholeText()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO\nSELECT 1 FROM FROM");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(15, diagnostic.Span.Start.Column);
        Assert.Equal(3, diagnostic.Span.End.Line);
        Assert.Equal(19, diagnostic.Span.End.Column);
    }

    [Fact]
    public void Translate_ParseErrorWithCrLf_ReportsTheCorrectLine()
    {
        DialectTranslationResult result = _translator.Translate(
            "SELECT 1\r\nGO\r\nSELECT 1 FROM FROM"
        );

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(15, diagnostic.Span.Start.Column);
    }

    [Fact]
    public void Translate_ParseError_UsesMsSqlErrorNumber102AndParseErrorKind()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1 FROM FROM");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(DiagnosticKind.ParseError, diagnostic.Kind);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(102, diagnostic.MsSqlErrorNumber);
        Assert.StartsWith("SQL", diagnostic.Code);
        Assert.Contains("Incorrect syntax near 'FROM'", diagnostic.Message);
    }

    [Fact]
    public void Translate_ParseErrorAtEndOfInput_HasAnEmptySpanAtTheEnd()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1 +");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(0, diagnostic.Span.Length);
        Assert.Equal("SELECT 1 +".Length, diagnostic.Span.Start.Offset);
        Assert.Equal(102, diagnostic.MsSqlErrorNumber);
    }

    [Fact]
    public void Translate_ParseErrorsInSeveralBatches_ReportsEveryBatch()
    {
        DialectTranslationResult result = _translator.Translate(
            "SELECT 1 FROM WHERE\nGO\nSELECT 2 2"
        );

        Assert.Equal(2, result.Diagnostics.Count);
        Assert.Equal([1, 3], result.Diagnostics.Select((DialectDiagnostic d) => d.Span.Start.Line));
    }

    [Fact]
    public void Translate_ParseErrorInLaterBatch_PreventsTheEarlierBatchFromRunning()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO\nSELECT 1 FROM FROM");

        Assert.False(result.Success);
        Assert.Empty(result.Batches);
    }

    [Fact]
    public void Translate_StatementOrigins_PointToTheirPositionInTheOriginalText()
    {
        DialectTranslationResult result = _translator.Translate(
            "SELECT 1\nGO\n  SELECT 2;  SELECT 3"
        );

        TranslatedBatch second = result.Batches[1];
        Assert.Equal(2, second.Statements.Count);
        Assert.Equal(new SourcePosition(14, 3, 3), second.Statements[0].Origin.Start);
        Assert.Equal(new SourcePosition(25, 3, 14), second.Statements[1].Origin.Start);
    }

    [Fact]
    public void Translate_StatementOrigin_CoversTheOriginalStatementText()
    {
        const string sql = "SELECT TOP 1 a\nFROM T";

        DialectTranslationResult result = _translator.Translate(sql);

        TranslatedStatement statement = Assert.Single(Assert.Single(result.Batches).Statements);
        Assert.Equal(0, statement.Origin.Start.Offset);
        Assert.Equal(sql.Length, statement.Origin.End.Offset);
        Assert.False(statement.IsGenerated);
    }

    [Fact]
    public void Translate_BatchSpan_CoversTheBatchText()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO 2\nSELECT 2");

        Assert.Equal(0, result.Batches[0].Span.Start.Offset);
        Assert.Equal("SELECT 1\n".Length, result.Batches[0].Span.End.Offset);
        Assert.Equal(2, result.Batches[0].RepeatCount);
        Assert.Equal("SELECT 1\nGO 2\n".Length, result.Batches[1].Span.Start.Offset);
    }

    [Fact]
    public void Translate_IgnoredSetStatement_AddsAnInformationDiagnosticAndKeepsSuccess()
    {
        DialectTranslationResult result = _translator.Translate("SET NOCOUNT ON; SELECT 1");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.True(result.Success);
        Assert.Empty(result.Errors);
        Assert.Equal(DiagnosticSeverity.Information, diagnostic.Severity);
        Assert.Equal(DiagnosticKind.IgnoredConstruct, diagnostic.Kind);
        Assert.Equal("IgnoredStatement", diagnostic.Code);
        Assert.Contains("SET NOCOUNT ON", diagnostic.Message);
        TranslatedStatement statement = Assert.Single(Assert.Single(result.Batches).Statements);
        Assert.Equal("SELECT 1", statement.Sql);
    }

    [Fact]
    public void Translate_OnlyIgnoredStatements_ProducesABatchWithoutStatements()
    {
        DialectTranslationResult result = _translator.Translate("SET NOCOUNT ON");

        Assert.True(result.Success);
        Assert.Empty(Assert.Single(result.Batches).Statements);
    }

    [Fact]
    public void Translate_InformationAlongsideAnError_StillFails()
    {
        DialectTranslationResult result = _translator.Translate(
            "SET NOCOUNT ON\nGO\nSET ANSI_NULLS OFF"
        );

        Assert.False(result.Success);
        Assert.Equal(
            [DiagnosticSeverity.Information, DiagnosticSeverity.Error],
            result.Diagnostics.Select((DialectDiagnostic d) => d.Severity)
        );
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Translate_EmptyText_SucceedsWithoutBatches()
    {
        DialectTranslationResult result = _translator.Translate(string.Empty);

        Assert.True(result.Success);
        Assert.Empty(result.Batches);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Translate_CommentOnlyText_SucceedsWithAnEmptyBatch()
    {
        DialectTranslationResult result = _translator.Translate("-- nothing here");

        Assert.True(result.Success);
        Assert.Empty(Assert.Single(result.Batches).Statements);
    }

    [Fact]
    public void Translate_UndeclaredVariable_ReportsMsSqlErrorNumber137AtTheReference()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO\nSELECT @missing");

        DialectDiagnostic diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(137, diagnostic.MsSqlErrorNumber);
        Assert.Equal(DiagnosticKind.TranslationError, diagnostic.Kind);
        Assert.Equal(3, diagnostic.Span.Start.Line);
        Assert.Equal(8, diagnostic.Span.Start.Column);
    }

    [Fact]
    public void ToSummary_FormatsPositionCodeAndMessage()
    {
        DialectTranslationResult result = _translator.Translate("SELECT 1\nGO\nSELECT 1 FROM FROM");

        string summary = Assert.Single(result.Diagnostics).ToSummary();

        Assert.Equal("L: 3:15 [SQL46010] Incorrect syntax near 'FROM'.", summary);
    }
}
