using LearnSql.Sql.Dialect.Batching;

namespace LearnSql.Tests.Dialect;

public class BatchSplitterTests
{
    private readonly BatchSplitter _splitter = new BatchSplitter();

    private IReadOnlyList<SqlBatchSource> Split(string sql) => _splitter.Split(sql);

    [Fact]
    public void Split_EmptyText_ReturnsNoBatch()
    {
        Assert.Empty(Split(string.Empty));
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\n\n")]
    [InlineData("\r\n \t \r\n")]
    public void Split_OnlyWhitespace_ReturnsNoBatch(string sql)
    {
        Assert.Empty(Split(sql));
    }

    [Fact]
    public void Split_TextWithoutGo_ReturnsOneBatchWithRepeatCountOne()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1");

        SqlBatchSource batch = Assert.Single(batches);
        Assert.Equal("SELECT 1", batch.Text);
        Assert.Equal(0, batch.StartOffset);
        Assert.Equal(1, batch.RepeatCount);
    }

    [Fact]
    public void Split_GoAloneOnALine_SeparatesTwoBatches()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\nGO\nSELECT 2");

        Assert.Equal(2, batches.Count);
        Assert.Equal("SELECT 1\n", batches[0].Text);
        Assert.Equal("SELECT 2", batches[1].Text);
    }

    [Fact]
    public void Split_SecondBatch_StartsAtTheOffsetBehindTheGoLine()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\nGO\nSELECT 2");

        Assert.Equal(0, batches[0].StartOffset);
        Assert.Equal("SELECT 1\nGO\n".Length, batches[1].StartOffset);
    }

    [Theory]
    [InlineData("go")]
    [InlineData("Go")]
    [InlineData("gO")]
    [InlineData("  GO")]
    [InlineData("\tGO\t")]
    [InlineData("GO   ")]
    [InlineData("GO -- trailing comment")]
    [InlineData("GO--trailing comment")]
    public void Split_GoVariants_AreRecognizedAsSeparator(string goLine)
    {
        IReadOnlyList<SqlBatchSource> batches = Split($"SELECT 1\n{goLine}\nSELECT 2");

        Assert.Equal(2, batches.Count);
    }

    [Theory]
    [InlineData("GOTO label")]
    [InlineData("GOAL")]
    [InlineData("GO0")]
    [InlineData("GO_1")]
    [InlineData("GO;")]
    [InlineData("GO x")]
    [InlineData("GO 1.5")]
    [InlineData("GO /* c */")]
    [InlineData("SELECT 1 GO")]
    [InlineData("GO 0")]
    [InlineData("GO 99999999999999999999")]
    public void Split_LineThatOnlyStartsLikeGo_IsNotASeparator(string line)
    {
        IReadOnlyList<SqlBatchSource> batches = Split($"SELECT 1\n{line}\nSELECT 2");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoWithCount_SetsTheRepeatCountOfThePrecedingBatch()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\nGO 5\nSELECT 2\nGO\nSELECT 3");

        Assert.Equal([5, 1, 1], batches.Select((SqlBatchSource batch) => batch.RepeatCount));
    }

    [Theory]
    [InlineData("GO 3")]
    [InlineData("GO   3")]
    [InlineData("go\t3  ")]
    [InlineData("GO 3 -- three times")]
    public void Split_GoCountVariants_ParseTheCount(string goLine)
    {
        IReadOnlyList<SqlBatchSource> batches = Split($"SELECT 1\n{goLine}\n");

        SqlBatchSource batch = Assert.Single(batches);
        Assert.Equal(3, batch.RepeatCount);
    }

    [Fact]
    public void Split_ConsecutiveGoLines_ProduceNoEmptyBatches()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("GO\nSELECT 1\nGO\nGO\n\nGO\nSELECT 2\nGO");

        Assert.Equal(["SELECT 1\n", "SELECT 2\n"], batches.Select((SqlBatchSource b) => b.Text));
    }

    [Fact]
    public void Split_OnlyGoLines_ReturnsNoBatch()
    {
        Assert.Empty(Split("GO\nGO 3\nGO"));
    }

    [Fact]
    public void Split_GoOnLastLineWithoutLineBreak_IsRecognized()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\nGO");

        SqlBatchSource batch = Assert.Single(batches);
        Assert.Equal("SELECT 1\n", batch.Text);
    }

    [Fact]
    public void Split_CrLfLineBreaks_AreHandledAndOffsetsCountBothCharacters()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\r\nGO\r\nSELECT 2\r\nGO 2\r\n");

        Assert.Equal(2, batches.Count);
        Assert.Equal("SELECT 1\r\n", batches[0].Text);
        Assert.Equal(14, batches[1].StartOffset);
        Assert.Equal("SELECT 2\r\n", batches[1].Text);
        Assert.Equal(2, batches[1].RepeatCount);
    }

    [Fact]
    public void Split_LoneCarriageReturnLineBreaks_AreHandled()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\rGO\rSELECT 2");

        Assert.Equal(2, batches.Count);
        Assert.Equal("SELECT 2", batches[1].Text);
    }

    [Fact]
    public void Split_GoInsideBlockComment_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("/*\nGO\n*/ SELECT 1");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoInsideNestedBlockComment_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split(
            "/* a /* b */\nGO\n c */ SELECT 1\nGO\nSELECT 2"
        );

        Assert.Equal(2, batches.Count);
        Assert.StartsWith("/* a /* b */", batches[0].Text);
    }

    [Fact]
    public void Split_GoAfterClosedBlockComment_IsASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("/* c */\nGO\nSELECT 1");

        Assert.Equal(["/* c */\n", "SELECT 1"], batches.Select((SqlBatchSource b) => b.Text));
    }

    [Fact]
    public void Split_GoInsideLineComment_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1 -- GO\nSELECT 2");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_ApostropheInsideLineComment_DoesNotOpenAString()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1 -- it's\nGO\nSELECT 2");

        Assert.Equal(2, batches.Count);
    }

    [Fact]
    public void Split_GoInsideMultilineString_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 'a\nGO\nb'");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoInsideStringWithEscapedQuote_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 'it''s\nGO\nstill'");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoAfterClosedString_IsASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 'a'\nGO\nSELECT 'b'");

        Assert.Equal(2, batches.Count);
    }

    [Fact]
    public void Split_GoInsideBracketIdentifier_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1 AS [a\nGO\nb]");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoInsideBracketIdentifierWithEscapedBracket_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1 AS [a]]\nGO\nb]");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_GoInsideDoubleQuotedIdentifier_IsNotASeparator()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1 AS \"a\nGO\nb\"");

        Assert.Single(batches);
    }

    [Fact]
    public void Split_BracketInsideString_DoesNotOpenAnIdentifier()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT '[' AS a\nGO\nSELECT 2");

        Assert.Equal(2, batches.Count);
    }

    [Fact]
    public void Split_GoWithSurroundingWhitespaceLines_KeepsTheBlankLinesInTheBatches()
    {
        IReadOnlyList<SqlBatchSource> batches = Split("SELECT 1\n\nGO\n\nSELECT 2\n");

        Assert.Equal("SELECT 1\n\n", batches[0].Text);
        Assert.Equal("\nSELECT 2\n", batches[1].Text);
    }
}
