using System.Text;

namespace LearnSql.Core.Services.Guided;

public static class GuidedSqlTokenizer
{
    private static readonly HashSet<string> Keywords = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "ADD",
        "ALL",
        "ALTER",
        "AND",
        "AS",
        "ASC",
        "BETWEEN",
        "BY",
        "CASE",
        "CHECK",
        "CONSTRAINT",
        "CREATE",
        "CROSS",
        "CURRENT",
        "DECLARE",
        "DEFAULT",
        "DELETE",
        "DESC",
        "DISTINCT",
        "DROP",
        "ELSE",
        "END",
        "EXCEPT",
        "EXISTS",
        "FETCH",
        "FIRST",
        "FOLLOWING",
        "FOREIGN",
        "FROM",
        "FULL",
        "GO",
        "GROUP",
        "HAVING",
        "IDENTITY",
        "IN",
        "INDEX",
        "INNER",
        "INSERT",
        "INTERSECT",
        "INTO",
        "IS",
        "JOIN",
        "KEY",
        "LEFT",
        "LIKE",
        "NEXT",
        "NOT",
        "NULL",
        "OFFSET",
        "ON",
        "ONLY",
        "OR",
        "ORDER",
        "OUTER",
        "OVER",
        "PARTITION",
        "PRECEDING",
        "PRIMARY",
        "RANGE",
        "REFERENCES",
        "RIGHT",
        "ROW",
        "ROWS",
        "SELECT",
        "SET",
        "TABLE",
        "THEN",
        "TOP",
        "UNBOUNDED",
        "UNION",
        "UNIQUE",
        "UPDATE",
        "VALUES",
        "VIEW",
        "WHEN",
        "WHERE",
        "WITH",
    };

    private static readonly HashSet<string> Types = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "BIT",
        "DATE",
        "DATETIME",
        "DECIMAL",
        "FLOAT",
        "INT",
        "MONEY",
        "NVARCHAR",
        "VARCHAR",
    };

    public static IReadOnlyList<GuidedSqlToken> Tokenize(string? sql)
    {
        if (string.IsNullOrEmpty(sql))
        {
            return [];
        }

        List<GuidedSqlToken> tokens = [];
        int index = 0;
        while (index < sql.Length)
        {
            char current = sql[index];
            int start = index;
            if (char.IsWhiteSpace(current))
            {
                while (index < sql.Length && char.IsWhiteSpace(sql[index]))
                {
                    index++;
                }

                tokens.Add(new GuidedSqlToken(sql[start..index], GuidedSqlTokenKind.Text));
            }
            else if (current == '\'')
            {
                index = ScanQuoted(sql, index, '\'');
                tokens.Add(new GuidedSqlToken(sql[start..index], GuidedSqlTokenKind.String));
            }
            else if (current == '[')
            {
                index = ScanBracketed(sql, index);
                tokens.Add(new GuidedSqlToken(sql[start..index], GuidedSqlTokenKind.Identifier));
            }
            else if (char.IsDigit(current))
            {
                while (index < sql.Length && (char.IsDigit(sql[index]) || sql[index] == '.'))
                {
                    index++;
                }

                tokens.Add(new GuidedSqlToken(sql[start..index], GuidedSqlTokenKind.Number));
            }
            else if (char.IsLetter(current) || current == '_' || current == '@' || current == '#')
            {
                while (index < sql.Length && IsWordChar(sql[index]))
                {
                    index++;
                }

                string word = sql[start..index];
                tokens.Add(new GuidedSqlToken(word, Classify(word, sql, index)));
            }
            else
            {
                index++;
                tokens.Add(new GuidedSqlToken(sql[start..index], GuidedSqlTokenKind.Text));
            }
        }

        return Merge(tokens);
    }

    private static GuidedSqlTokenKind Classify(string word, string sql, int end)
    {
        if (Keywords.Contains(word) || Types.Contains(word))
        {
            return GuidedSqlTokenKind.Keyword;
        }

        int next = end;
        while (next < sql.Length && char.IsWhiteSpace(sql[next]))
        {
            next++;
        }

        return next < sql.Length && sql[next] == '('
            ? GuidedSqlTokenKind.Function
            : GuidedSqlTokenKind.Text;
    }

    private static bool IsWordChar(char value)
    {
        return char.IsLetterOrDigit(value) || value == '_' || value == '@' || value == '#';
    }

    private static int ScanQuoted(string sql, int start, char quote)
    {
        int index = start + 1;
        while (index < sql.Length)
        {
            if (sql[index] == quote)
            {
                if (index + 1 < sql.Length && sql[index + 1] == quote)
                {
                    index += 2;
                    continue;
                }

                return index + 1;
            }

            index++;
        }

        return sql.Length;
    }

    private static int ScanBracketed(string sql, int start)
    {
        int close = sql.IndexOf(']', start + 1);
        return close < 0 ? sql.Length : close + 1;
    }

    private static List<GuidedSqlToken> Merge(List<GuidedSqlToken> tokens)
    {
        List<GuidedSqlToken> merged = [];
        StringBuilder pending = new StringBuilder();
        foreach (GuidedSqlToken token in tokens)
        {
            if (token.Kind == GuidedSqlTokenKind.Text)
            {
                pending.Append(token.Text);
                continue;
            }

            Flush(merged, pending);
            merged.Add(token);
        }

        Flush(merged, pending);
        return merged;
    }

    private static void Flush(List<GuidedSqlToken> merged, StringBuilder pending)
    {
        if (pending.Length == 0)
        {
            return;
        }

        merged.Add(new GuidedSqlToken(pending.ToString(), GuidedSqlTokenKind.Text));
        pending.Clear();
    }
}
