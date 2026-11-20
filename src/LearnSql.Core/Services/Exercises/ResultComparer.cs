using System.Text;

namespace LearnSql.Core.Services.Exercises;

public static class ResultComparer
{
    private const int NumericDecimals = 9;
    private const double MaxExactLong = 9.2e18;

    public static ResultComparison Compare(ResultSet expected, ResultSet actual, bool orderMatters)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        string[] expectedColumns = [.. expected.Columns.Select(NameOf)];
        string[] actualColumns = [.. actual.Columns.Select(NameOf)];
        if (expectedColumns.Length != actualColumns.Length)
        {
            return new ResultComparison(
                expectedColumns,
                actualColumns,
                expected.Rows.Count,
                actual.Rows.Count,
                actual.Truncated,
                false,
                [],
                []
            );
        }

        string[] expectedKeys = [.. expected.Rows.Select(RowKey)];
        string[] actualKeys = [.. actual.Rows.Select(RowKey)];

        Dictionary<string, int> unmatched = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (string key in expectedKeys)
        {
            unmatched[key] = unmatched.GetValueOrDefault(key) + 1;
        }

        List<object?[]> extra = [];
        for (int index = 0; index < actualKeys.Length; index++)
        {
            if (unmatched.TryGetValue(actualKeys[index], out int open) && open > 0)
            {
                unmatched[actualKeys[index]] = open - 1;
            }
            else
            {
                extra.Add(actual.Rows[index]);
            }
        }

        List<object?[]> missing = [];
        for (int index = 0; index < expectedKeys.Length; index++)
        {
            if (unmatched[expectedKeys[index]] > 0)
            {
                unmatched[expectedKeys[index]]--;
                missing.Add(expected.Rows[index]);
            }
        }

        bool orderDiffers =
            orderMatters
            && missing.Count == 0
            && extra.Count == 0
            && !expectedKeys.AsSpan().SequenceEqual(actualKeys);

        return new ResultComparison(
            expectedColumns,
            actualColumns,
            expected.Rows.Count,
            actual.Rows.Count,
            actual.Truncated,
            orderDiffers,
            missing,
            extra
        );
    }

    public static bool ValuesEqual(object? left, object? right)
    {
        return string.Equals(CellKey(left), CellKey(right), StringComparison.Ordinal);
    }

    private static string NameOf(ResultColumn column)
    {
        return column.Name;
    }

    private static string RowKey(object?[] row)
    {
        StringBuilder builder = new StringBuilder();
        foreach (object? value in row)
        {
            builder.Append(CellKey(value));
        }

        return builder.ToString();
    }

    private static string CellKey(object? value)
    {
        return value switch
        {
            null => "N;",
            long number => NumberKey(number),
            int number => NumberKey(number),
            short number => NumberKey(number),
            byte number => NumberKey(number),
            double number => NumberKey(number),
            float number => NumberKey(number),
            decimal number => NumberKey((double)number),
            string text => string.Create(CultureInfo.InvariantCulture, $"s{text.Length}:{text}"),
            byte[] bytes => "b" + Convert.ToHexString(bytes) + ";",
            _ => string.Create(
                CultureInfo.InvariantCulture,
                $"o{value.GetType().FullName}:{value}"
            ),
        };
    }

    private static string NumberKey(long number)
    {
        return string.Create(CultureInfo.InvariantCulture, $"n{number};");
    }

    private static string NumberKey(double number)
    {
        if (!double.IsFinite(number))
        {
            return string.Create(CultureInfo.InvariantCulture, $"d{number:R};");
        }

        double rounded = Math.Round(number, NumericDecimals, MidpointRounding.ToEven);
        if (Math.Abs(rounded) < MaxExactLong && rounded == Math.Floor(rounded))
        {
            return NumberKey((long)rounded);
        }

        return string.Create(CultureInfo.InvariantCulture, $"d{rounded:R};");
    }
}
