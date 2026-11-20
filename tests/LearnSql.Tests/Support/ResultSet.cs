namespace LearnSql.Tests.Support;

public sealed record ResultSet(
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows
)
{
    public int RowCount => Rows.Count;

    public IReadOnlyList<object?> Column(int index = 0)
    {
        return [.. Rows.Select((IReadOnlyList<object?> row) => row[index])];
    }

    public object? Single()
    {
        Assert.Single(Columns);
        Assert.Single(Rows);
        return Rows[0][0];
    }
}
