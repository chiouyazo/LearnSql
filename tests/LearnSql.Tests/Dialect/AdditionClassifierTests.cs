using LearnSql.Sql.Dialect.Typing;

namespace LearnSql.Tests.Dialect;

public class AdditionClassifierTests
{
    [Theory]
    [InlineData(ValueKind.Text, ValueKind.Text, AdditionMeaning.Concatenation)]
    [InlineData(ValueKind.Text, ValueKind.Null, AdditionMeaning.Concatenation)]
    [InlineData(ValueKind.Null, ValueKind.Text, AdditionMeaning.Concatenation)]
    [InlineData(ValueKind.Text, ValueKind.Unknown, AdditionMeaning.Concatenation)]
    [InlineData(ValueKind.Unknown, ValueKind.Text, AdditionMeaning.Concatenation)]
    [InlineData(ValueKind.Text, ValueKind.Numeric, AdditionMeaning.Numeric)]
    [InlineData(ValueKind.Numeric, ValueKind.Text, AdditionMeaning.Numeric)]
    [InlineData(ValueKind.Numeric, ValueKind.Numeric, AdditionMeaning.Numeric)]
    [InlineData(ValueKind.Numeric, ValueKind.Null, AdditionMeaning.Numeric)]
    [InlineData(ValueKind.Numeric, ValueKind.Unknown, AdditionMeaning.Numeric)]
    [InlineData(ValueKind.DateTime, ValueKind.Numeric, AdditionMeaning.DateArithmetic)]
    [InlineData(ValueKind.Numeric, ValueKind.DateTime, AdditionMeaning.DateArithmetic)]
    [InlineData(ValueKind.DateTime, ValueKind.Text, AdditionMeaning.Unknown)]
    [InlineData(ValueKind.DateTime, ValueKind.DateTime, AdditionMeaning.Unknown)]
    [InlineData(ValueKind.Unknown, ValueKind.Unknown, AdditionMeaning.Unknown)]
    [InlineData(ValueKind.Null, ValueKind.Null, AdditionMeaning.Unknown)]
    [InlineData(ValueKind.Text, ValueKind.Binary, AdditionMeaning.Unknown)]
    [InlineData(ValueKind.Binary, ValueKind.Binary, AdditionMeaning.Unknown)]
    public void Classify_KindPair_YieldsTheMeaningOfPlus(
        ValueKind left,
        ValueKind right,
        AdditionMeaning expected
    )
    {
        Assert.Equal(expected, AdditionClassifier.Classify(left, right));
    }

    [Theory]
    [InlineData(ValueKind.Text, ValueKind.Numeric)]
    [InlineData(ValueKind.Null, ValueKind.Text)]
    [InlineData(ValueKind.DateTime, ValueKind.Numeric)]
    public void Classify_IsSymmetric(ValueKind left, ValueKind right)
    {
        Assert.Equal(
            AdditionClassifier.Classify(left, right),
            AdditionClassifier.Classify(right, left)
        );
    }
}
