namespace LearnSql.Sql.Dialect.Syntax;

public sealed record StatementProfile(
    StatementKind Kind,
    string? TargetTable,
    IReadOnlyList<string> ModifiedObjects
)
{
    public static StatementProfile Unknown { get; } =
        new StatementProfile(StatementKind.Other, null, []);

    public bool ChangesSchema => Kind.ChangesSchema();
}
