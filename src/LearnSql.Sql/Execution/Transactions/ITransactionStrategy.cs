using LearnSql.Sql.Dialect;

namespace LearnSql.Sql.Execution;

internal interface ITransactionStrategy
{
    bool RollsBack { get; }

    Task BeginAsync(CancellationToken cancellationToken);

    StatementAction Resolve(TranslatedStatement statement);

    Task EndAsync();
}
