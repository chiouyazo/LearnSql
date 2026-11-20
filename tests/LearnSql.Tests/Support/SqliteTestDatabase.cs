using LearnSql.Data.Scenarios;
using LearnSql.Sql.Dialect;
using LearnSql.Sql.Dialect.Functions;
using LearnSql.Sql.Dialect.Typing;
using LearnSql.Sql.Exceptions;
using LearnSql.Sql.MsSql;
using Microsoft.Data.Sqlite;

namespace LearnSql.Tests.Support;

public sealed class SqliteTestDatabase : IDisposable
{
    private static readonly ScenarioCatalog Catalog = ScenarioCatalog.LoadDefault();

    private readonly SqliteConnection _connection;
    private readonly MsSqlErrorTranslator _errorTranslator = new MsSqlErrorTranslator();

    public SqliteTestDatabase(TimeProvider? timeProvider = null)
    {
        _connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        _connection.Open();
        new SqliteFunctionRegistrar(timeProvider ?? new SingleDeterministicTimeProvider()).Register(
            _connection
        );
    }

    public SqliteConnection Connection => _connection;

    public static ScenarioCatalog ScenarioCatalog => Catalog;

    public static SqliteTestDatabase ForScenario(string scenarioId)
    {
        SqliteTestDatabase database = new SqliteTestDatabase();
        database.Provision(scenarioId);
        return database;
    }

    public void Provision(string scenarioId)
    {
        ScenarioScripts scripts = Catalog.GetScripts(scenarioId);
        foreach (SqlScript script in scripts.InExecutionOrder)
            ExecuteRaw(script.Content);
    }

    public void ExecuteRaw(string sql)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public ISchemaCatalog ReadSchema()
    {
        return new SqliteSchemaCatalogReader().Read(_connection);
    }

    public DialectTranslationResult Translate(string sql)
    {
        return TSqlDialectTranslator.CreateDefault(ReadSchema()).Translate(sql);
    }

    public ScriptOutcome Run(string sql)
    {
        DialectTranslationResult translation = Translate(sql);
        if (!translation.Success)
            return new ScriptOutcome(translation, [], 0, null);

        List<ResultSet> resultSets = [];
        int affected = 0;
        try
        {
            foreach (TranslatedBatch batch in translation.Batches)
            {
                for (int repetition = 0; repetition < batch.RepeatCount; repetition++)
                {
                    foreach (TranslatedStatement statement in batch.Statements)
                        affected += ExecuteStatement(statement, resultSets);
                }
            }
        }
        catch (SqliteException exception)
        {
            TranslatedException translated = _errorTranslator.CreateException(exception);
            return new ScriptOutcome(translation, resultSets, affected, translated);
        }

        return new ScriptOutcome(translation, resultSets, affected, null);
    }

    public ScriptOutcome RunSuccessfully(string sql)
    {
        ScriptOutcome outcome = Run(sql);
        Assert.True(outcome.Succeeded, $"Statement failed: {sql}\n{outcome.Describe()}");
        return outcome;
    }

    public object? Scalar(string sql)
    {
        ScriptOutcome outcome = RunSuccessfully(sql);
        return outcome.LastResultSet.Single();
    }

    public ResultSet Query(string sql)
    {
        return RunSuccessfully(sql).LastResultSet;
    }

    public long TotalChanges()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT total_changes()";
        return (long)command.ExecuteScalar()!;
    }

    public string StateFingerprint()
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText =
            "SELECT total_changes() || ':' || (SELECT schema_version FROM pragma_schema_version)";
        return (string)command.ExecuteScalar()!;
    }

    public IReadOnlyList<string> ForeignKeyViolations()
    {
        List<string> violations = [];
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check";
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
            violations.Add($"{reader.GetString(0)}:{reader.GetValue(1)}->{reader.GetString(2)}");
        return violations;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private int ExecuteStatement(TranslatedStatement statement, List<ResultSet> resultSets)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = statement.Sql;
        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.FieldCount > 0)
        {
            List<string> columns = [];
            for (int index = 0; index < reader.FieldCount; index++)
                columns.Add(reader.GetName(index));

            List<IReadOnlyList<object?>> rows = [];
            while (reader.Read())
            {
                object?[] values = new object?[reader.FieldCount];
                for (int index = 0; index < values.Length; index++)
                    values[index] = reader.IsDBNull(index) ? null : reader.GetValue(index);
                rows.Add(values);
            }

            if (!statement.IsGenerated)
                resultSets.Add(new ResultSet(columns, rows));
            return 0;
        }

        return statement.IsGenerated ? 0 : Math.Max(reader.RecordsAffected, 0);
    }
}
