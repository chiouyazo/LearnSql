namespace LearnSql.SqlFormat;

public static class SqlFormatter
{
    public static string Format(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);

        List<SqlStatement> statements = SqlTokenizer.Tokenize(sql);
        List<string> blocks = [];
        foreach (SqlStatement statement in statements)
        {
            blocks.Add(
                statement.IsBatchSeparator
                    ? ExpressionLayout.Flat(statement.Tokens)
                    : StatementLayout.Format(statement.Tokens)
            );
        }

        string formatted = Join(statements, blocks);
        EnsureSameTokens(statements, formatted);
        return formatted;
    }

    private static string Join(List<SqlStatement> statements, List<string> blocks)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int index = 0; index < blocks.Count; index++)
        {
            if (index > 0)
                builder.Append(statements[index].IsBatchSeparator ? "\n" : "\n\n");
            builder.Append(blocks[index]);
        }

        return builder.ToString();
    }

    private static void EnsureSameTokens(List<SqlStatement> statements, string formatted)
    {
        List<string> expected = SqlTokenizer.Fingerprint(statements);
        List<string> actual = SqlTokenizer.Fingerprint(SqlTokenizer.Tokenize(formatted));
        if (!expected.SequenceEqual(actual, StringComparer.Ordinal))
            throw new InvalidOperationException("Formatting changed the tokens of the SQL.");
    }
}
