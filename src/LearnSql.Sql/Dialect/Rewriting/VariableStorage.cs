namespace LearnSql.Sql.Dialect.Rewriting;

public static class VariableStorage
{
    public const string TableName = "__learnsql_variables";

    private const string QualifiedTable = "temp.\"" + TableName + "\"";

    public static IReadOnlyList<string> SetupStatements { get; } =
    [
        "CREATE TEMP TABLE IF NOT EXISTS \""
            + TableName
            + "\" (\"name\" TEXT PRIMARY KEY, \"value\")",
        "DELETE FROM " + QualifiedTable,
    ];

    public static string Read(string variableName)
    {
        return "(SELECT \"value\" FROM "
            + QualifiedTable
            + " WHERE \"name\" = "
            + KeyLiteral(variableName)
            + ")";
    }

    public static string Declare(
        IEnumerable<KeyValuePair<string, string>> variablesWithInitialValueSql
    )
    {
        IEnumerable<string> rows = variablesWithInitialValueSql.Select(entry =>
            "(" + KeyLiteral(entry.Key) + ", " + entry.Value + ")"
        );
        return "INSERT OR REPLACE INTO "
            + QualifiedTable
            + " (\"name\", \"value\") VALUES "
            + string.Join(", ", rows);
    }

    public static string Assign(string variableName, string valueSql)
    {
        return "UPDATE "
            + QualifiedTable
            + " SET \"value\" = "
            + valueSql
            + " WHERE \"name\" = "
            + KeyLiteral(variableName);
    }

    private static string KeyLiteral(string variableName)
    {
        return SqliteSyntax.QuoteString(variableName.ToLowerInvariant());
    }
}
