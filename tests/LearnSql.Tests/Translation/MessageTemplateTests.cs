using LearnSql.Sql.Models;

namespace LearnSql.Tests.Translation;

public class MessageTemplateTests
{
    private static IReadOnlyDictionary<string, string> Args(
        params (string Key, string Value)[] pairs
    )
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach ((string key, string value) in pairs)
            result[key] = value;
        return result;
    }

    [Fact]
    public void TryRender_AllArgumentsPresent_ReplacesEveryPlaceholder()
    {
        bool success = MessageTemplate.TryRender(
            "Invalid {Kind} '{Name}' in {Kind}.",
            Args(("Kind", "table"), ("Name", "T")),
            out string text
        );

        Assert.True(success);
        Assert.Equal("Invalid table 'T' in table.", text);
    }

    [Fact]
    public void TryRender_MissingArgument_FailsAndReturnsEmptyText()
    {
        bool success = MessageTemplate.TryRender(
            "Invalid '{Name}'.",
            Args(("Other", "x")),
            out string text
        );

        Assert.False(success);
        Assert.Equal(string.Empty, text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void TryRender_EmptyOrWhitespaceArgument_Fails(string value)
    {
        bool success = MessageTemplate.TryRender(
            "Invalid '{Name}'.",
            Args(("Name", value)),
            out string text
        );

        Assert.False(success);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryRender_OnePlaceholderMissingAmongMany_Fails()
    {
        bool success = MessageTemplate.TryRender("{A} and {B}", Args(("A", "1")), out string text);

        Assert.False(success);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryRender_ArgumentValueContainingPlaceholder_IsNotSubstitutedAgain()
    {
        bool success = MessageTemplate.TryRender(
            "A={A}, B={B}",
            Args(("A", "{B}"), ("B", "final")),
            out string text
        );

        Assert.True(success);
        Assert.Equal("A={B}, B=final", text);
    }

    [Fact]
    public void TryRender_ValueWithBracesAndDollarSigns_IsInsertedLiterally()
    {
        bool success = MessageTemplate.TryRender(
            "Value {V}.",
            Args(("V", "$1 {x} $&")),
            out string text
        );

        Assert.True(success);
        Assert.Equal("Value $1 {x} $&.", text);
    }

    [Fact]
    public void TryRender_TemplateWithoutPlaceholders_ReturnsTheTemplate()
    {
        bool success = MessageTemplate.TryRender("Plain text.", Args(), out string text);

        Assert.True(success);
        Assert.Equal("Plain text.", text);
    }

    [Fact]
    public void TryRender_EmptyTemplate_Succeeds()
    {
        bool success = MessageTemplate.TryRender(string.Empty, Args(), out string text);

        Assert.True(success);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryRender_ExtraArguments_AreIgnored()
    {
        bool success = MessageTemplate.TryRender(
            "{A}",
            Args(("A", "1"), ("Unused", "2")),
            out string text
        );

        Assert.True(success);
        Assert.Equal("1", text);
    }

    [Fact]
    public void TryRender_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MessageTemplate.TryRender("x", null!, out string _)
        );
        Assert.Throws<ArgumentNullException>(() =>
            MessageTemplate.TryRender(null!, Args(), out string _)
        );
    }

    [Fact]
    public void GetPlaceholders_ReturnsDistinctNamesInOrderOfAppearance()
    {
        IReadOnlyList<string> placeholders = MessageTemplate.GetPlaceholders("{B} {A} {B} {C}");

        Assert.Equal(["B", "A", "C"], placeholders);
    }

    [Theory]
    [InlineData("no placeholders")]
    [InlineData("{}")]
    [InlineData("{1abc}")]
    [InlineData("{ spaced }")]
    [InlineData("{unclosed")]
    public void GetPlaceholders_NoValidPlaceholder_ReturnsEmpty(string template)
    {
        Assert.Empty(MessageTemplate.GetPlaceholders(template));
    }

    [Fact]
    public void GetPlaceholders_NullTemplate_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MessageTemplate.GetPlaceholders(null!));
    }
}
