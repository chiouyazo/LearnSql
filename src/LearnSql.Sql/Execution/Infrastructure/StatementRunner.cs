using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Rewriting;
using LearnSql.Sql.Dialect.Syntax;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql.Execution;

internal sealed class StatementRunner
{
    private readonly ResultSetReader _reader;
    private readonly TimeProvider _timeProvider;

    public StatementRunner(ResultSetReader reader, TimeProvider timeProvider)
    {
        _reader = reader;
        _timeProvider = timeProvider;
    }

    public async Task<StatementResult> RunAsync(
        SandboxConnection connection,
        TranslatedStatement statement,
        StatementAction action,
        int batchIndex,
        int iteration,
        ExecutionOptions options,
        ExecutionGuard guard,
        CancellationToken cancellationToken
    )
    {
        long start = _timeProvider.GetTimestamp();
        StatementProfile profile = statement.EffectiveProfile;
        List<ResultSet> resultSets = [];
        List<string> messages = [];
        int? rowsAffected = null;

        if (action.Kind != StatementActionKind.Skip)
        {
            string sql = action.Kind == StatementActionKind.Replace ? action.Sql! : statement.Sql;
            await using (SqliteCommand command = connection.Connection.CreateCommand())
            {
                command.CommandText = sql;
                await using (
                    SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken)
                )
                {
                    do
                    {
                        if (reader.FieldCount > 0)
                        {
                            ResultSet resultSet = await _reader.ReadAsync(
                                reader,
                                options.MaxRowsPerResultSet,
                                guard,
                                cancellationToken
                            );
                            resultSets.Add(resultSet);
                            messages.Add(DescribeResultSet(resultSet));
                        }
                    } while (await reader.NextResultAsync(cancellationToken));
                }
            }

            rowsAffected = await CountAffectedAsync(
                connection,
                profile,
                resultSets.Count > 0,
                cancellationToken
            );
            if (rowsAffected is int count)
            {
                messages.Add(ExecutionMessages.RowsAffected(count));
            }
        }

        return new StatementResult(
            batchIndex,
            iteration,
            profile.Kind,
            statement.Origin,
            resultSets,
            rowsAffected,
            _timeProvider.GetElapsedTime(start),
            messages
        );
    }

    private static string DescribeResultSet(ResultSet resultSet)
    {
        return resultSet.Truncated
            ? ExecutionMessages.RowsShown(resultSet.Rows.Count)
            : ExecutionMessages.RowsAffected(resultSet.Rows.Count);
    }

    private static async Task<int?> CountAffectedAsync(
        SandboxConnection connection,
        StatementProfile profile,
        bool producedResultSet,
        CancellationToken cancellationToken
    )
    {
        if (producedResultSet)
        {
            return null;
        }

        if (profile.Kind.IsDataModification())
        {
            return connection.LastChanges;
        }

        if (profile.Kind == StatementKind.SelectInto && profile.TargetTable is string table)
        {
            await using (SqliteCommand command = connection.Connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM " + SqliteSyntax.QuoteIdentifier(table);
                object? value = await command.ExecuteScalarAsync(cancellationToken);
                return Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        return null;
    }
}
