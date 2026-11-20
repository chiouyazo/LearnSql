namespace LearnSql.Sql.Execution;

public sealed record ForeignKeySchema(
    string? Name,
    IReadOnlyList<string> Columns,
    string ReferencedTable,
    IReadOnlyList<string> ReferencedColumns,
    string OnUpdate,
    string OnDelete
);
