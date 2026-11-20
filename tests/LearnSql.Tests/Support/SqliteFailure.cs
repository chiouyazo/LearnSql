using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Support;

public static class SqliteFailure
{
    public const string ShopSetup =
        "CREATE TABLE Parent (Id INTEGER PRIMARY KEY, Code TEXT UNIQUE);"
        + "CREATE TABLE Child ("
        + "Id INTEGER PRIMARY KEY, "
        + "ParentId INTEGER NOT NULL REFERENCES Parent(Id), "
        + "Qty INTEGER CHECK (Qty > 0));"
        + "INSERT INTO Parent VALUES (1, 'A');";

    public static SqliteException Capture(string setupSql, string failingSql)
    {
        using SqliteConnection connection = new SqliteConnection(
            "Data Source=:memory:;Foreign Keys=True"
        );
        connection.Open();

        if (setupSql.Length > 0)
        {
            using SqliteCommand setup = connection.CreateCommand();
            setup.CommandText = setupSql;
            setup.ExecuteNonQuery();
        }

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = failingSql;
        return Assert.Throws<SqliteException>(() => command.ExecuteNonQuery());
    }
}
