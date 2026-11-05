using System.Data;
using System.Reflection;
using LearnSql.Data;
using LearnSql.Sql.Exceptions;
using LearnSql.Sql.Generated;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql;

public class DatabaseProvider : IDisposable, IAsyncDisposable
{
    private SqliteConnection? _connection;

    /// <summary>
    /// Called automatically on startup.
    /// </summary>
    public async Task Initialize()
    {
        await ResetDatabase();
    }

    private async Task ResetDatabase()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        _connection = new SqliteConnection($"Data Source=:memory:;Foreign Keys=True");
        await _connection.OpenAsync();

        await ProvisionDefaultSchema();
    }

    private async Task ProvisionDefaultSchema()
    {
        try
        {
            Assembly asm = typeof(DataHolder).Assembly;

            // TODO: Error handling for UI
            // TODO: Load resources dynamically:
            string[] manifestResourceNames = asm.GetManifestResourceNames();

            List<string> tables = [];
            List<string> views = [];
            List<string> constraints = [];
            foreach (string manifestResourceName in manifestResourceNames)
            {
                if (manifestResourceName.Contains("Base.Views."))
                    views.Add(manifestResourceName);
                else if (manifestResourceName.Contains("Base.Tables."))
                    tables.Add(manifestResourceName);
                else if (manifestResourceName.Contains("Base.Constraints."))
                    constraints.Add(manifestResourceName);
            }

            // Tables first, then views (they reference tables), then constraints.
            // Within a group the numeric file prefixes (1.*, 2.*, 3.*) define the order.
            foreach (string name in tables.Order(StringComparer.Ordinal))
                await ApplyResource(asm, name);
            foreach (string name in views.Order(StringComparer.Ordinal))
                await ApplyResource(asm, name);
            foreach (string name in constraints.Order(StringComparer.Ordinal))
                await ApplyResource(asm, name);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to provision default schema:");
            Console.WriteLine(e);
        }
    }

    private async Task ApplyResource(Assembly asm, string name)
    {
        try
        {
            string content = string.Empty;
            using (Stream? manifestResourceStream = asm.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(manifestResourceStream))
            {
                content = await reader.ReadToEndAsync();
            }

            using (SqliteCommand command = _connection!.CreateCommand())
            {
                command.CommandText = content;
                command.ExecuteNonQuery();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to apply resource {name}.\n" + e);
        }
    }

    public async Task ProvisionScenario(string id)
    {
        await ResetDatabase();
        Assembly asm = typeof(DataHolder).Assembly;
        string[] manifestResourceNames = asm.GetManifestResourceNames();
        List<string> baseData =
        [
            .. manifestResourceNames
                .Where(x => x.StartsWith($"LearnSql.Data.Data.{id}.SQL.Base."))
                .Order(),
        ];

        if (!baseData.Any())
        {
            // TODO: Error scenario not found
            return;
        }
        foreach (string path in baseData)
        {
            await ApplyResource(asm, path);
        }
    }

    public async Task<DataTable?> RunCommand(string text)
    {
        using (SqliteCommand command = _connection.CreateCommand())
        {
            command.CommandText = text;

            try
            {
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    List<List<object>> rows = new List<List<object>>();
                    int recordsAffected = reader.RecordsAffected;
                    bool hasRows = reader.HasRows;
                    // DataTable.Load infers column types from the first row; for view/expression columns
                    // SQLite has no declared type, so a NULL in row 1 yields Byte[] and later values fail.
                    DataTable data = new DataTable();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string name = reader.GetName(i);
                        string uniqueName = name;
                        for (int suffix = 1; data.Columns.Contains(uniqueName); suffix++)
                            uniqueName = $"{name}{suffix}";
                        data.Columns.Add(uniqueName, typeof(object));
                    }

                    object[] values = new object[reader.FieldCount];
                    while (await reader.ReadAsync())
                    {
                        reader.GetValues(values);
                        data.Rows.Add(values);
                    }

                    return data;
                }
            }
            catch (SqliteException liteEx)
            {
                // https://sqlite.org/rescode.html
                if (liteEx.SqliteErrorCode == 1)
                {
                    // generic "error code"
                    Console.WriteLine($"SQLite error {liteEx.SqliteErrorCode}: {liteEx.Message}");
                    SqliteError genericSqlError = SqliteErrorMapper.Match(liteEx.Message);
                    Console.WriteLine(
                        $"Mapped error to {genericSqlError.Type} with {genericSqlError.Message} and args {string.Join(", ", genericSqlError.Arguments)}"
                    );
                    throw new TranslatedException(genericSqlError);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // throw;
            }
        }

        return null;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
