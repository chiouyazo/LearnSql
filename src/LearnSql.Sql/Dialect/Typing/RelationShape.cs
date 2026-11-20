namespace LearnSql.Sql.Dialect.Typing;

public sealed class RelationShape
{
    public RelationShape(IReadOnlyList<ColumnInfo> columns)
    {
        Columns = columns;
    }

    public IReadOnlyList<ColumnInfo> Columns { get; }

    public bool TryGetColumn(string name, out ColumnInfo? column)
    {
        foreach (ColumnInfo candidate in Columns)
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                column = candidate;
                return true;
            }
        }
        column = null;
        return false;
    }
}
