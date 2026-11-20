namespace LearnSql.Sql.Dialect.Typing;

public sealed class SchemaCatalog : ISchemaCatalog
{
    private readonly Dictionary<string, RelationShape> _relations;

    public SchemaCatalog(IEnumerable<KeyValuePair<string, RelationShape>> relations)
    {
        _relations = new Dictionary<string, RelationShape>(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, RelationShape> relation in relations)
            _relations[relation.Key] = relation.Value;
    }

    public static SchemaCatalog Empty { get; } = new SchemaCatalog([]);

    public bool TryGetRelation(string name, out RelationShape? shape)
    {
        bool found = _relations.TryGetValue(name, out RelationShape? value);
        shape = value;
        return found;
    }
}
