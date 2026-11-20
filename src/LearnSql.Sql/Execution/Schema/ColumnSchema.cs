namespace LearnSql.Sql.Execution;

public sealed record ColumnSchema(
    string Name,
    string DeclaredType,
    bool IsNullable,
    bool IsPrimaryKey,
    int PrimaryKeyOrdinal,
    bool IsIdentity,
    string? DefaultValue
);
