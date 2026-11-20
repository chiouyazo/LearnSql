namespace LearnSql.Sql.Execution;

public static class ExecutionMessages
{
    public const string CommandsCompleted = "Commands completed successfully.";

    public static string RowsAffected(int count)
    {
        return count == 1 ? "(1 row affected)" : $"({count} rows affected)";
    }

    public static string RowsShown(int count)
    {
        return $"({count} rows shown, more rows exist)";
    }
}
