namespace LearnSql.Sql.Dialect.Typing;

public sealed class SourceScope
{
    public SourceScope(IReadOnlyList<RelationSource> sources)
    {
        Sources = sources;
    }

    public IReadOnlyList<RelationSource> Sources { get; }

    public bool TryResolveColumn(string? qualifier, string column, out ColumnInfo? info)
    {
        foreach (RelationSource source in Sources)
        {
            if (
                qualifier is not null
                && !string.Equals(source.Name, qualifier, StringComparison.OrdinalIgnoreCase)
            )
                continue;

            if (
                source.Shape is not null
                && source.Shape.TryGetColumn(column, out ColumnInfo? found)
            )
            {
                info = found;
                return true;
            }
        }
        info = null;
        return false;
    }
}
