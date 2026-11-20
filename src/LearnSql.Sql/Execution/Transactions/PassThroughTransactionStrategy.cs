using LearnSql.Sql.Dialect;

namespace LearnSql.Sql.Execution;

internal sealed class PassThroughTransactionStrategy : ITransactionStrategy
{
    public bool RollsBack => false;

    public Task BeginAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public StatementAction Resolve(TranslatedStatement statement)
    {
        return StatementAction.Run;
    }

    public Task EndAsync()
    {
        return Task.CompletedTask;
    }
}
