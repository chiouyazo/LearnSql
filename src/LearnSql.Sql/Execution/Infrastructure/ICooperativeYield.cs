namespace LearnSql.Sql.Execution;

public interface ICooperativeYield
{
    void StartSlice();

    ValueTask YieldIfDueAsync(CancellationToken cancellationToken);

    ValueTask YieldAsync(CancellationToken cancellationToken);
}
