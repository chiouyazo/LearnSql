using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Syntax;

namespace LearnSql.Sql.Execution;

internal sealed class RollbackTransactionStrategy : ITransactionStrategy
{
    private const string Savepoint = "learnsql_run";

    private readonly SandboxConnection _connection;
    private bool _started;

    public RollbackTransactionStrategy(SandboxConnection connection)
    {
        _connection = connection;
    }

    public bool RollsBack => true;

    public async Task BeginAsync(CancellationToken cancellationToken)
    {
        await _connection.ExecuteNonQueryAsync($"SAVEPOINT {Savepoint}", cancellationToken);
        _started = true;
    }

    public StatementAction Resolve(TranslatedStatement statement)
    {
        return statement.EffectiveProfile.Kind switch
        {
            StatementKind.BeginTransaction or StatementKind.CommitTransaction =>
                StatementAction.Skip,
            StatementKind.RollbackTransaction when IsUnnamedRollback(statement) =>
                StatementAction.Replace($"ROLLBACK TO {Savepoint}"),
            _ => StatementAction.Run,
        };
    }

    public async Task EndAsync()
    {
        if (!_started)
        {
            return;
        }

        _started = false;
        if (_connection.IsAutocommit)
        {
            return;
        }

        await _connection.ExecuteNonQueryAsync($"ROLLBACK TO {Savepoint}", CancellationToken.None);
        await _connection.ExecuteNonQueryAsync($"RELEASE {Savepoint}", CancellationToken.None);
    }

    private static bool IsUnnamedRollback(TranslatedStatement statement)
    {
        return string.Equals(statement.Sql, "ROLLBACK", StringComparison.OrdinalIgnoreCase);
    }
}
