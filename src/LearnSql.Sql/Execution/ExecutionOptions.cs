namespace LearnSql.Sql.Execution;

public sealed record ExecutionOptions
{
    public const int DefaultMaxRowsPerResultSet = 1000;

    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public static ExecutionOptions Default { get; } = new ExecutionOptions();

    public int MaxRowsPerResultSet { get; init; } = DefaultMaxRowsPerResultSet;

    public TimeSpan Timeout { get; init; } = DefaultTimeout;

    public bool RollbackAfterExecution { get; init; }

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxRowsPerResultSet);
        if (Timeout != System.Threading.Timeout.InfiniteTimeSpan)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Timeout, TimeSpan.Zero);
        }
    }
}
