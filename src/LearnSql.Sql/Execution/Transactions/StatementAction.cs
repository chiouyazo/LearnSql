namespace LearnSql.Sql.Execution;

internal enum StatementActionKind
{
    Run,
    Skip,
    Replace,
}

internal readonly record struct StatementAction(StatementActionKind Kind, string? Sql)
{
    public static StatementAction Run { get; } = new StatementAction(StatementActionKind.Run, null);

    public static StatementAction Skip { get; } =
        new StatementAction(StatementActionKind.Skip, null);

    public static StatementAction Replace(string sql)
    {
        return new StatementAction(StatementActionKind.Replace, sql);
    }
}
