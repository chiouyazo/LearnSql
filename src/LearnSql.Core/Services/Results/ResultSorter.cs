namespace LearnSql.Core.Services.Results;

public enum ResultSortDirection
{
    Ascending,
    Descending,
}

public static class ResultSorter
{
    public static int[] Sort(ResultSet resultSet, int columnIndex, ResultSortDirection direction)
    {
        int[] order = [.. Enumerable.Range(0, resultSet.Rows.Count)];
        Comparison<int> comparison = (int left, int right) =>
        {
            int result = CompareValues(
                resultSet.Rows[left][columnIndex],
                resultSet.Rows[right][columnIndex]
            );
            if (direction == ResultSortDirection.Descending)
            {
                result = -result;
            }

            return result != 0 ? result : left.CompareTo(right);
        };
        Array.Sort(order, comparison);
        return order;
    }

    public static int CompareValues(object? left, object? right)
    {
        if (left is null || right is null)
        {
            return (left is null ? 0 : 1) - (right is null ? 0 : 1);
        }

        if (CellFormatter.IsNumeric(left) && CellFormatter.IsNumeric(right))
        {
            return Convert
                .ToDecimal(left, CultureInfo.InvariantCulture)
                .CompareTo(Convert.ToDecimal(right, CultureInfo.InvariantCulture));
        }

        return string.Compare(
            CellFormatter.Format(left),
            CellFormatter.Format(right),
            StringComparison.OrdinalIgnoreCase
        );
    }
}
