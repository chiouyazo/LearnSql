namespace LearnSql.Sql.Dialect.Rewriting;

public static class SqliteSyntax
{
    public static string QuoteIdentifier(string name)
    {
        return "\"" + name.Replace("\"", "\"\"") + "\"";
    }

    public static string QuoteString(string value)
    {
        return "'" + value.Replace("'", "''") + "'";
    }
}
