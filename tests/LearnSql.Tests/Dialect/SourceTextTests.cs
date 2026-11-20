using LearnSql.Sql.Dialect.Diagnostics;

namespace LearnSql.Tests.Dialect;

public class SourceTextTests
{
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(2, 1, 3)]
    [InlineData(3, 1, 4)]
    [InlineData(4, 2, 1)]
    [InlineData(6, 2, 3)]
    [InlineData(7, 3, 1)]
    [InlineData(8, 3, 2)]
    public void GetPosition_LfText_IsOneBased(int offset, int line, int column)
    {
        SourceText text = new SourceText("abc\nde\nf");

        SourcePosition position = text.GetPosition(offset);

        Assert.Equal(new SourcePosition(offset, line, column), position);
    }

    [Fact]
    public void GetPosition_CrLf_CountsTheSequenceAsOneLineBreak()
    {
        SourceText text = new SourceText("ab\r\ncd");

        Assert.Equal(new SourcePosition(4, 2, 1), text.GetPosition(4));
        Assert.Equal(new SourcePosition(5, 2, 2), text.GetPosition(5));
    }

    [Fact]
    public void GetPosition_LoneCarriageReturn_StartsANewLine()
    {
        SourceText text = new SourceText("ab\rcd");

        Assert.Equal(new SourcePosition(3, 2, 1), text.GetPosition(3));
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(100, 3)]
    public void GetPosition_OutOfRangeOffset_IsClamped(int offset, int expectedOffset)
    {
        SourceText text = new SourceText("abc");

        Assert.Equal(expectedOffset, text.GetPosition(offset).Offset);
    }

    [Fact]
    public void GetSpan_CoversTheRequestedRangeAndReportsItsLength()
    {
        SourceText text = new SourceText("abc\ndef");

        SourceSpan span = text.GetSpan(2, 4);

        Assert.Equal(new SourcePosition(2, 1, 3), span.Start);
        Assert.Equal(new SourcePosition(6, 2, 3), span.End);
        Assert.Equal(4, span.Length);
    }

    [Fact]
    public void GetSpan_NegativeLength_IsTreatedAsEmpty()
    {
        SourceText text = new SourceText("abc");

        Assert.Equal(0, text.GetSpan(1, -3).Length);
    }

    [Fact]
    public void Length_IsTheTextLength()
    {
        Assert.Equal(7, new SourceText("abc\ndef").Length);
    }
}
