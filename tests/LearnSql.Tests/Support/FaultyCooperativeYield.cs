using LearnSql.Sql.Execution;

namespace LearnSql.Tests.Support;

public sealed class FaultyCooperativeYield : ICooperativeYield
{
    public bool ThrowOnYieldIfDue { get; set; }

    public bool ThrowOnYield { get; set; }

    public void StartSlice() { }

    public ValueTask YieldIfDueAsync(CancellationToken cancellationToken)
    {
        if (ThrowOnYieldIfDue)
        {
            throw new InvalidOperationException("The yield failed on purpose.");
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask YieldAsync(CancellationToken cancellationToken)
    {
        if (ThrowOnYield)
        {
            throw new InvalidOperationException("The yield failed on purpose.");
        }

        return ValueTask.CompletedTask;
    }
}
