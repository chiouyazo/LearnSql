namespace LearnSql.Data.Scenarios;

public sealed class NaturalStringComparer : IComparer<string>
{
    public static NaturalStringComparer Instance { get; } = new NaturalStringComparer();

    private NaturalStringComparer() { }

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;

        int i = 0;
        int j = 0;
        while (i < x.Length && j < y.Length)
        {
            if (char.IsAsciiDigit(x[i]) && char.IsAsciiDigit(y[j]))
            {
                int startX = i;
                int startY = j;
                while (i < x.Length && char.IsAsciiDigit(x[i]))
                    i++;
                while (j < y.Length && char.IsAsciiDigit(y[j]))
                    j++;
                ReadOnlySpan<char> numberX = x.AsSpan(startX, i - startX).TrimStart('0');
                ReadOnlySpan<char> numberY = y.AsSpan(startY, j - startY).TrimStart('0');
                if (numberX.Length != numberY.Length)
                    return numberX.Length.CompareTo(numberY.Length);
                int numberComparison = numberX.CompareTo(numberY, StringComparison.Ordinal);
                if (numberComparison != 0)
                    return numberComparison;
            }
            else
            {
                int characterComparison = char.ToUpperInvariant(x[i])
                    .CompareTo(char.ToUpperInvariant(y[j]));
                if (characterComparison != 0)
                    return characterComparison;
                i++;
                j++;
            }
        }

        return (x.Length - i).CompareTo(y.Length - j);
    }
}
