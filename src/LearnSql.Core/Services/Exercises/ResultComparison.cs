namespace LearnSql.Core.Services.Exercises;

public sealed record ResultComparison(
    IReadOnlyList<string> ExpectedColumns,
    IReadOnlyList<string> ActualColumns,
    int ExpectedRowCount,
    int ActualRowCount,
    bool ActualTruncated,
    bool OrderDiffers,
    IReadOnlyList<object?[]> MissingRows,
    IReadOnlyList<object?[]> ExtraRows
)
{
    public bool ColumnCountDiffers => ExpectedColumns.Count != ActualColumns.Count;

    public bool RowCountDiffers => ExpectedRowCount != ActualRowCount;

    public bool IsEqual =>
        !ColumnCountDiffers
        && !ActualTruncated
        && !OrderDiffers
        && MissingRows.Count == 0
        && ExtraRows.Count == 0;
}
