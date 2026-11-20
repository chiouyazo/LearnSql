namespace LearnSql.Sql.Execution;

public sealed record DatabaseSchema(IReadOnlyList<RelationSchema> Relations)
{
    public static DatabaseSchema Empty { get; } = new DatabaseSchema([]);

    public IEnumerable<RelationSchema> Tables =>
        Relations.Where((RelationSchema relation) => relation.Kind == RelationKind.Table);

    public IEnumerable<RelationSchema> Views =>
        Relations.Where((RelationSchema relation) => relation.Kind == RelationKind.View);

    public RelationSchema? Find(string name)
    {
        return Relations.FirstOrDefault(
            (RelationSchema relation) =>
                string.Equals(relation.Name, name, StringComparison.OrdinalIgnoreCase)
        );
    }
}
