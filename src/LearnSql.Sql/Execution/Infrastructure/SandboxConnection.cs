using LearnSql.Sql.Dialect.Functions;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace LearnSql.Sql.Execution;

internal sealed class SandboxConnection : IAsyncDisposable
{
    private const string ConnectionString = "Data Source=:memory:;Foreign Keys=True";
    private const int PageSizeBytes = 4096;

    private SandboxConnection(SqliteConnection connection)
    {
        Connection = connection;
    }

    public SqliteConnection Connection { get; }

    public sqlite3 Handle =>
        Connection.Handle ?? throw new ObjectDisposedException(nameof(SandboxConnection));

    public bool IsAutocommit => raw.sqlite3_get_autocommit(Handle) != 0;

    public int LastChanges => raw.sqlite3_changes(Handle);

    public static async Task<SandboxConnection> OpenAsync(
        SandboxOptions options,
        SqliteFunctionRegistrar registrar,
        CancellationToken cancellationToken
    )
    {
        SqliteConnection connection = new SqliteConnection(ConnectionString);
        SandboxConnection sandbox = new SandboxConnection(connection);
        try
        {
            await connection.OpenAsync(cancellationToken);
            registrar.Register(connection);
            raw.sqlite3_limit(sandbox.Handle, raw.SQLITE_LIMIT_LENGTH, options.MaxValueBytes);
            long maxPages = Math.Max(1, options.MaxDatabaseBytes / PageSizeBytes);
            await sandbox.ExecuteNonQueryAsync(
                $"PRAGMA max_page_count = {maxPages}",
                cancellationToken
            );
            return sandbox;
        }
        catch
        {
            await sandbox.DisposeAsync();
            throw;
        }
    }

    public async Task ExecuteNonQueryAsync(string sql, CancellationToken cancellationToken)
    {
        await using (SqliteCommand command = Connection.CreateCommand())
        {
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}
