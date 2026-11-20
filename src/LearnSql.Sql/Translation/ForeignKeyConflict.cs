namespace LearnSql.Sql.Translation;

public sealed record ForeignKeyConflict(
    string? ConstraintName,
    string ChildTable,
    string ChildColumn,
    string ParentTable,
    string ParentColumn
);
