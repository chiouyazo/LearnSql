using LearnSql.Sql.Generated;
using Microsoft.Data.Sqlite;

namespace LearnSql.Sql;

public class DatabaseProvider : IDisposable, IAsyncDisposable
{
	private SqliteConnection _connection;
	
	/// <summary>
	/// Called automatically on startup.
	/// </summary>
	public async Task Initialize()
	{
		SqliteConnection connection = new SqliteConnection("Data Source=data.db");
		await connection.OpenAsync();
		
		_connection = connection;
	}

	public async Task<string> RunCommand(string text)
	{
		using (SqliteCommand command = _connection.CreateCommand())
		{
			command.CommandText = text;

			try
			{
				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						return "";
					}
				}
			}
			catch (SqliteException liteEx)
			{
				// https://sqlite.org/rescode.html
				if (liteEx.SqliteErrorCode == 1)
				{
					// generic "error code"
					Console.WriteLine($"SQLite error {liteEx.SqliteErrorCode}: {liteEx.Message}");
					SqliteError sqliteError = SqliteErrorMapper.Match(liteEx.Message);
					Console.WriteLine($"Mapped error to {sqliteError.Type} with {sqliteError.Message} and args {string.Join(", ", sqliteError.Arguments)}");
				}
					
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				// throw;
			}
		}

		return "";
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