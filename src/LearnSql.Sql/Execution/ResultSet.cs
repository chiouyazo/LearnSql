namespace LearnSql.Sql.Execution;

public sealed record ResultSet(
    IReadOnlyList<ResultColumn> Columns,
    IReadOnlyList<object?[]> Rows,
    bool Truncated
);
