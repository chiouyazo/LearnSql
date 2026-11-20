namespace LearnSql.Sql.Execution;

public sealed record RelationSchema(
    string Name,
    RelationKind Kind,
    bool IsSystem,
    bool IsTemporary,
    IReadOnlyList<ColumnSchema> Columns,
    IReadOnlyList<ForeignKeySchema> ForeignKeys
);
