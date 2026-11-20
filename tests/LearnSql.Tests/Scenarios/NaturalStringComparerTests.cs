using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Scenarios;

public class NaturalStringComparerTests
{
    private static readonly NaturalStringComparer Comparer = NaturalStringComparer.Instance;

    [Theory]
    [InlineData("2.x", "10.x")]
    [InlineData("file2", "file10")]
    [InlineData("a9", "a10")]
    [InlineData("a1b", "a1c")]
    [InlineData("a", "b")]
    [InlineData("a", "ab")]
    [InlineData("step-02", "step-10")]
    [InlineData("1", "01x")]
    [InlineData("a001", "a2")]
    public void Compare_FirstIsSmaller_ReturnsNegative(string smaller, string larger)
    {
        Assert.True(Comparer.Compare(smaller, larger) < 0);
        Assert.True(Comparer.Compare(larger, smaller) > 0);
    }

    [Theory]
    [InlineData("abc", "ABC")]
    [InlineData("a01", "a1")]
    [InlineData("007", "7")]
    [InlineData("", "")]
    public void Compare_EquivalentStrings_ReturnsZero(string left, string right)
    {
        Assert.Equal(0, Comparer.Compare(left, right));
    }

    [Fact]
    public void Compare_SameReference_ReturnsZero()
    {
        string text = "a1";

        Assert.Equal(0, Comparer.Compare(text, text));
    }

    [Fact]
    public void Compare_NullHandling_PutsNullFirst()
    {
        Assert.Equal(0, Comparer.Compare(null, null));
        Assert.True(Comparer.Compare(null, "a") < 0);
        Assert.True(Comparer.Compare("a", null) > 0);
    }

    [Fact]
    public void Sort_OfNumberedIds_UsesNumericOrderInsideTheNumbers()
    {
        string[] ids = ["select-10", "select-2", "Select-1", "select-11", "filter-3"];

        string[] sorted = [.. ids.Order(Comparer)];

        Assert.Equal(["filter-3", "Select-1", "select-2", "select-10", "select-11"], sorted);
    }

    [Fact]
    public void Compare_VeryLongDigitRuns_DoesNotOverflow()
    {
        string small = "a" + new string('9', 40);
        string large = "a1" + new string('0', 40);

        Assert.True(Comparer.Compare(small, large) < 0);
    }

    [Fact]
    public void Instance_IsASingleton()
    {
        Assert.Same(NaturalStringComparer.Instance, NaturalStringComparer.Instance);
    }
}
