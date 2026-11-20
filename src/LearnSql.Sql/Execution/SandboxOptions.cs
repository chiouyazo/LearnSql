namespace LearnSql.Sql.Execution;

public sealed record SandboxOptions
{
    public TimeSpan YieldInterval { get; init; } = TimeSpan.FromMilliseconds(15);

    public int RowsPerYieldCheck { get; init; } = 64;

    public int ProgressHandlerInstructions { get; init; } = 4096;

    public long MaxDatabaseBytes { get; init; } = 64L * 1024 * 1024;

    public int MaxValueBytes { get; init; } = 16 * 1024 * 1024;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(YieldInterval, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(RowsPerYieldCheck);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ProgressHandlerInstructions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxDatabaseBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxValueBytes);
    }
}
