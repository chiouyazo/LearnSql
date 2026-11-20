using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace LearnSql.Core.Services.Templates;

public static partial class SqlIdentifier
{
    private static readonly FrozenSet<string> ReservedWords = new string[]
    {
        "ADD",
        "ALL",
        "ALTER",
        "AND",
        "AS",
        "BY",
        "CASE",
        "CHECK",
        "COLUMN",
        "CONSTRAINT",
        "CREATE",
        "DEFAULT",
        "DELETE",
        "DESC",
        "DISTINCT",
        "DROP",
        "EXISTS",
        "FOREIGN",
        "FROM",
        "FULL",
        "GROUP",
        "HAVING",
        "IN",
        "INDEX",
        "INNER",
        "INSERT",
        "INTO",
        "IS",
        "JOIN",
        "KEY",
        "LEFT",
        "LIKE",
        "NOT",
        "NULL",
        "ON",
        "OR",
        "ORDER",
        "OUTER",
        "PRIMARY",
        "REFERENCES",
        "RIGHT",
        "SELECT",
        "SET",
        "TABLE",
        "TOP",
        "UNION",
        "UNIQUE",
        "UPDATE",
        "USER",
        "VALUES",
        "VIEW",
        "WHEN",
        "WHERE",
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string Quote(string name)
    {
        bool plain = PlainName().IsMatch(name) && !ReservedWords.Contains(name);
        return plain ? name : "[" + name.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex PlainName();
}
