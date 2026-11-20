namespace LearnSql.Sql.Dialect;

public static class SandboxSchemas
{
    public const string DatabaseName = "LearnSql";

    private static readonly string[] LocalNames = ["dbo", "INFORMATION_SCHEMA"];

    public static bool IsLocal(string schemaName)
    {
        foreach (string name in LocalNames)
        {
            if (string.Equals(name, schemaName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
