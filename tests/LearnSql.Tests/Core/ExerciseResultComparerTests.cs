using LearnSql.Core.Services.Exercises;
using SqlResultSet = LearnSql.Sql.Execution.ResultSet;

namespace LearnSql.Tests.Core;

public sealed class ExerciseResultComparerTests
{
    private static SqlResultSet Table(string[] columns, params object?[][] rows)
    {
        return ExerciseTestData.Table(columns, rows);
    }

    private static object?[] Row(params object?[] values)
    {
        return values;
    }

    [Fact]
    public void Compare_IdenticalResults_AreEqual()
    {
        SqlResultSet expected = Table(["A", "B"], Row(1L, "x"), Row(2L, "y"));
        SqlResultSet actual = Table(["A", "B"], Row(1L, "x"), Row(2L, "y"));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, true);

        Assert.True(comparison.IsEqual);
        Assert.False(comparison.ColumnCountDiffers);
        Assert.False(comparison.RowCountDiffers);
        Assert.False(comparison.OrderDiffers);
        Assert.Empty(comparison.MissingRows);
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_TwoEmptyResultsWithSameShape_AreEqual()
    {
        ResultComparison comparison = ResultComparer.Compare(Table(["A"]), Table(["A"]), false);

        Assert.True(comparison.IsEqual);
    }

    [Fact]
    public void Compare_UnorderedExercise_IgnoresRowOrder()
    {
        SqlResultSet expected = Table(["A"], Row(1L), Row(2L), Row(3L));
        SqlResultSet actual = Table(["A"], Row(3L), Row(1L), Row(2L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.True(comparison.IsEqual);
        Assert.False(comparison.OrderDiffers);
    }

    [Fact]
    public void Compare_OrderedExercise_ReportsReorderedRows()
    {
        SqlResultSet expected = Table(["A"], Row(1L), Row(2L), Row(3L));
        SqlResultSet actual = Table(["A"], Row(3L), Row(1L), Row(2L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, true);

        Assert.False(comparison.IsEqual);
        Assert.True(comparison.OrderDiffers);
        Assert.Empty(comparison.MissingRows);
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_OrderedExercise_DoesNotReportOrderWhenRowsDiffer()
    {
        SqlResultSet expected = Table(["A"], Row(1L), Row(2L));
        SqlResultSet actual = Table(["A"], Row(2L), Row(9L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, true);

        Assert.False(comparison.IsEqual);
        Assert.False(comparison.OrderDiffers);
        Assert.Equal(Row(1L), Assert.Single(comparison.MissingRows));
        Assert.Equal(Row(9L), Assert.Single(comparison.ExtraRows));
    }

    [Fact]
    public void Compare_OrderedExercise_DistinguishesOrderOfDuplicateRows()
    {
        SqlResultSet expected = Table(["A"], Row("a"), Row("a"), Row("b"));
        SqlResultSet actual = Table(["A"], Row("a"), Row("b"), Row("a"));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, true);

        Assert.True(comparison.OrderDiffers);
    }

    [Fact]
    public void Compare_MissingRows_AreListedFromTheExpectedResult()
    {
        SqlResultSet expected = Table(["A"], Row(1L), Row(2L), Row(3L), Row(4L));
        SqlResultSet actual = Table(["A"], Row(2L), Row(4L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.True(comparison.RowCountDiffers);
        Assert.Equal(4, comparison.ExpectedRowCount);
        Assert.Equal(2, comparison.ActualRowCount);
        Assert.Equal([Row(1L), Row(3L)], comparison.MissingRows);
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_ExtraRows_AreListedFromTheActualResult()
    {
        SqlResultSet expected = Table(["A"], Row(1L));
        SqlResultSet actual = Table(["A"], Row(1L), Row(7L), Row(8L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.Empty(comparison.MissingRows);
        Assert.Equal([Row(7L), Row(8L)], comparison.ExtraRows);
    }

    [Fact]
    public void Compare_SameRowCountButDifferentRows_ReportsMissingAndExtra()
    {
        SqlResultSet expected = Table(["A"], Row(1L), Row(2L));
        SqlResultSet actual = Table(["A"], Row(1L), Row(3L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.False(comparison.RowCountDiffers);
        Assert.Equal(Row(2L), Assert.Single(comparison.MissingRows));
        Assert.Equal(Row(3L), Assert.Single(comparison.ExtraRows));
    }

    [Fact]
    public void Compare_UsesMultisetSemanticsForDuplicateRows()
    {
        SqlResultSet expected = Table(["A"], Row("a"), Row("a"), Row("b"));
        SqlResultSet actual = Table(["A"], Row("a"), Row("b"), Row("b"));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.Equal(Row("a"), Assert.Single(comparison.MissingRows));
        Assert.Equal(Row("b"), Assert.Single(comparison.ExtraRows));
    }

    [Fact]
    public void Compare_DuplicateTooFewTimes_IsMissingOneRow()
    {
        SqlResultSet expected = Table(["A"], Row("a"), Row("a"));
        SqlResultSet actual = Table(["A"], Row("a"));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.Equal(Row("a"), Assert.Single(comparison.MissingRows));
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_DifferentColumnCount_IsReportedWithoutRowComparison()
    {
        SqlResultSet expected = Table(["A", "B"], Row(1L, 2L));
        SqlResultSet actual = Table(["A"], Row(1L), Row(5L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.True(comparison.ColumnCountDiffers);
        Assert.Equal(2, comparison.ExpectedColumns.Count);
        Assert.Single(comparison.ActualColumns);
        Assert.Equal(1, comparison.ExpectedRowCount);
        Assert.Equal(2, comparison.ActualRowCount);
        Assert.Empty(comparison.MissingRows);
        Assert.Empty(comparison.ExtraRows);
        Assert.False(comparison.OrderDiffers);
    }

    [Fact]
    public void Compare_ColumnNamesAndAliases_MayDiffer()
    {
        SqlResultSet expected = Table(["OrderCount"], Row(5L));
        SqlResultSet actual = Table(["n"], Row(5L));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, true);

        Assert.True(comparison.IsEqual);
        Assert.Equal(["OrderCount"], comparison.ExpectedColumns);
        Assert.Equal(["n"], comparison.ActualColumns);
    }

    [Fact]
    public void Compare_ColumnOrderMatters()
    {
        SqlResultSet expected = Table(["Name", "City"], Row("Anna", "Hamburg"));
        SqlResultSet actual = Table(["City", "Name"], Row("Hamburg", "Anna"));

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.Single(comparison.MissingRows);
        Assert.Single(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_ColumnNameCasing_IsIrrelevant()
    {
        SqlResultSet expected = Table(["id"], Row(1L));
        SqlResultSet actual = Table(["ID"], Row(1L));

        Assert.True(ResultComparer.Compare(expected, actual, false).IsEqual);
    }

    [Fact]
    public void Compare_NullEqualsNull()
    {
        SqlResultSet expected = Table(["A", "B"], Row(null, "x"));
        SqlResultSet actual = Table(["A", "B"], Row(null, "x"));

        Assert.True(ResultComparer.Compare(expected, actual, true).IsEqual);
    }

    [Fact]
    public void Compare_NullDiffersFromZeroAndEmptyText()
    {
        Assert.False(
            ResultComparer
                .Compare(Table(["A"], Row((object?)null)), Table(["A"], Row(0L)), false)
                .IsEqual
        );
        Assert.False(
            ResultComparer
                .Compare(Table(["A"], Row((object?)null)), Table(["A"], Row("")), false)
                .IsEqual
        );
        Assert.False(
            ResultComparer
                .Compare(Table(["A"], Row("NULL")), Table(["A"], Row((object?)null)), false)
                .IsEqual
        );
    }

    [Theory]
    [InlineData(1L, 1.0)]
    [InlineData(0L, -0.0)]
    [InlineData(42L, 42.0000000001)]
    [InlineData(-7L, -7.0)]
    public void ValuesEqual_IntegersAndEquivalentFloats_AreEqual(long left, double right)
    {
        Assert.True(ResultComparer.ValuesEqual(left, right));
        Assert.True(ResultComparer.ValuesEqual(right, left));
    }

    [Fact]
    public void ValuesEqual_FloatingPointNoise_IsTolerated()
    {
        Assert.True(ResultComparer.ValuesEqual(0.1 + 0.2, 0.3));
        Assert.True(ResultComparer.ValuesEqual(1.0 / 3.0, 0.33333333333333));
        Assert.True(ResultComparer.ValuesEqual(100.0 * 1.1, 110.00000000000001));
    }

    [Theory]
    [InlineData(1.0, 1.0001)]
    [InlineData(0.3, 0.31)]
    [InlineData(2.5, 2.4)]
    [InlineData(12.35, 12.3)]
    public void ValuesEqual_RealDifferences_AreNotEqual(double left, double right)
    {
        Assert.False(ResultComparer.ValuesEqual(left, right));
    }

    [Fact]
    public void ValuesEqual_RoundedAndUnroundedAverages_AreDifferent()
    {
        Assert.False(ResultComparer.ValuesEqual(12.345678, 12.35));
    }

    [Fact]
    public void ValuesEqual_LargeIntegers_AreComparedExactly()
    {
        Assert.True(ResultComparer.ValuesEqual(9007199254740993L, 9007199254740993L));
        Assert.False(ResultComparer.ValuesEqual(9007199254740993L, 9007199254740992L));
    }

    [Fact]
    public void ValuesEqual_NonFiniteNumbers_AreComparedByValue()
    {
        Assert.True(ResultComparer.ValuesEqual(double.PositiveInfinity, double.PositiveInfinity));
        Assert.False(ResultComparer.ValuesEqual(double.PositiveInfinity, double.NegativeInfinity));
        Assert.True(ResultComparer.ValuesEqual(double.NaN, double.NaN));
        Assert.False(ResultComparer.ValuesEqual(double.NaN, 0.0));
    }

    [Fact]
    public void ValuesEqual_OtherNumericTypes_AreNormalized()
    {
        Assert.True(ResultComparer.ValuesEqual(5, 5L));
        Assert.True(ResultComparer.ValuesEqual((short)5, 5.0));
        Assert.True(ResultComparer.ValuesEqual(2.5m, 2.5));
        Assert.True(ResultComparer.ValuesEqual(2.5f, 2.5));
        Assert.True(ResultComparer.ValuesEqual((byte)3, 3L));
    }

    [Fact]
    public void ValuesEqual_Text_IsComparedExactly()
    {
        Assert.True(ResultComparer.ValuesEqual("Hamburg", "Hamburg"));
        Assert.False(ResultComparer.ValuesEqual("Hamburg", "hamburg"));
        Assert.False(ResultComparer.ValuesEqual("Hamburg", "Hamburg "));
        Assert.False(ResultComparer.ValuesEqual("Hamburg", " Hamburg"));
        Assert.False(ResultComparer.ValuesEqual("a", "a\n"));
    }

    [Fact]
    public void ValuesEqual_TextAndNumber_AreDifferentTypes()
    {
        Assert.False(ResultComparer.ValuesEqual("1", 1L));
        Assert.False(ResultComparer.ValuesEqual("1.5", 1.5));
        Assert.False(ResultComparer.ValuesEqual(1L, "1"));
    }

    [Fact]
    public void ValuesEqual_Binary_IsComparedByContent()
    {
        Assert.True(ResultComparer.ValuesEqual(new byte[] { 1, 2 }, new byte[] { 1, 2 }));
        Assert.False(ResultComparer.ValuesEqual(new byte[] { 1, 2 }, new byte[] { 1, 3 }));
        Assert.False(ResultComparer.ValuesEqual(new byte[] { 0x31 }, "1"));
    }

    [Fact]
    public void ValuesEqual_NullHandling()
    {
        Assert.True(ResultComparer.ValuesEqual(null, null));
        Assert.False(ResultComparer.ValuesEqual(null, 0L));
        Assert.False(ResultComparer.ValuesEqual("", null));
    }

    [Fact]
    public void Compare_CellBoundaries_DoNotCauseCollisions()
    {
        SqlResultSet expected = Table(["A", "B"], Row("a", "bc"));
        SqlResultSet actual = Table(["A", "B"], Row("ab", "c"));

        Assert.False(ResultComparer.Compare(expected, actual, false).IsEqual);
    }

    [Fact]
    public void Compare_TextResemblingEncodedNumbers_DoesNotCollideWithNumbers()
    {
        SqlResultSet expected = Table(["A", "B"], Row(1L, 2L));
        SqlResultSet actual = Table(["A", "B"], Row("n1;", "n2;"));

        Assert.False(ResultComparer.Compare(expected, actual, false).IsEqual);
    }

    [Fact]
    public void Compare_TruncatedActualResult_IsNeverEqual()
    {
        SqlResultSet expected = Table(["A"], Row(1L));
        SqlResultSet actual = new SqlResultSet(Table(["A"]).Columns, [Row(1L)], true);

        ResultComparison comparison = ResultComparer.Compare(expected, actual, false);

        Assert.False(comparison.IsEqual);
        Assert.True(comparison.ActualTruncated);
    }

    [Fact]
    public void Compare_ExpectedEmptyButRowsReturned_ReportsExtraRows()
    {
        ResultComparison comparison = ResultComparer.Compare(
            Table(["A"]),
            Table(["A"], Row(1L)),
            true
        );

        Assert.False(comparison.IsEqual);
        Assert.Single(comparison.ExtraRows);
        Assert.Empty(comparison.MissingRows);
    }

    [Fact]
    public void Compare_ActualEmptyButRowsExpected_ReportsMissingRows()
    {
        ResultComparison comparison = ResultComparer.Compare(
            Table(["A"], Row(1L), Row(2L)),
            Table(["A"]),
            false
        );

        Assert.False(comparison.IsEqual);
        Assert.Equal(2, comparison.MissingRows.Count);
        Assert.Empty(comparison.ExtraRows);
    }

    [Fact]
    public void Compare_DoesNotModifyItsInputs()
    {
        object?[][] expectedRows = [Row(2L), Row(1L)];
        object?[][] actualRows = [Row(1L), Row(2L)];
        SqlResultSet expected = Table(["A"], expectedRows);
        SqlResultSet actual = Table(["A"], actualRows);

        ResultComparer.Compare(expected, actual, true);

        Assert.Equal(Row(2L), expectedRows[0]);
        Assert.Equal(Row(1L), actualRows[0]);
    }

    [Fact]
    public void Compare_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ResultComparer.Compare(null!, Table(["A"]), false)
        );
        Assert.Throws<ArgumentNullException>(() =>
            ResultComparer.Compare(Table(["A"]), null!, false)
        );
    }

    [Fact]
    public void Compare_RealisticAggregateResult_ToleratesIntegerAndFloatRepresentations()
    {
        SqlResultSet expected = Table(["Category", "Average"], Row("Kaffee", 8.5), Row("Tee", 5.0));
        SqlResultSet actual = Table(["Cat", "Avg"], Row("Tee", 5L), Row("Kaffee", 8.5000000001));

        Assert.True(ResultComparer.Compare(expected, actual, false).IsEqual);
        Assert.True(ResultComparer.Compare(expected, actual, true).OrderDiffers);
    }
}
