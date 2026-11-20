namespace LearnSql.Sql.Execution;

internal sealed class ExecutionTimeoutException : Exception
{
    public ExecutionTimeoutException(TimeSpan limit)
        : base($"The execution exceeded the time limit of {limit.TotalSeconds:0.##} seconds.")
    {
        Limit = limit;
    }

    public TimeSpan Limit { get; }
}
