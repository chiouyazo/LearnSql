using LearnSql.Sql.MsSql;

namespace LearnSql.Tests.MsSql;

public class MsSqlErrorTests
{
    [Fact]
    public void ToString_WithoutLine_ContainsHeaderWithoutLineAndMessage()
    {
        MsSqlError error = new MsSqlError(208, 16, 1, "Invalid object name 'T'.");

        Assert.Equal("Msg 208, Level 16, State 1\nInvalid object name 'T'.", error.ToString());
    }

    [Fact]
    public void ToString_WithLine_ContainsHeaderWithLineAndMessage()
    {
        MsSqlError error = new MsSqlError(208, 16, 1, "Invalid object name 'T'.", 3);

        Assert.Equal(
            "Msg 208, Level 16, State 1, Line 3\nInvalid object name 'T'.",
            error.ToString()
        );
    }

    [Fact]
    public void Header_WithLineZero_StillShowsTheLine()
    {
        MsSqlError error = new MsSqlError(1, 2, 3, "m", 0);

        Assert.Equal("Msg 1, Level 2, State 3, Line 0", error.Header);
    }

    [Fact]
    public void Definition_Create_CopiesNumberSeverityStateAndLine()
    {
        MsSqlErrorDefinition definition = new MsSqlErrorDefinition(515, 16, 2, "template");

        MsSqlError error = definition.Create("rendered", 9);

        Assert.Equal(new MsSqlError(515, 16, 2, "rendered", 9), error);
    }

    [Fact]
    public void Definition_CreateWithoutLine_HasNoLineNumber()
    {
        MsSqlErrorDefinition definition = new MsSqlErrorDefinition(515, 16, 2, "template");

        Assert.Null(definition.Create("rendered").LineNumber);
    }
}
