using System.Text.RegularExpressions;

namespace LearnSql.Sql.Execution;

public static partial class ForeignKeyNameParser
{
    [GeneratedRegex(
        @"CONSTRAINT\s+(?<Name>""[^""]+""|\[[^\]]+\]|`[^`]+`|\w+)\s+FOREIGN\s+KEY\s*\((?<Columns>[^)]*)\)\s*REFERENCES\s+(?<Table>""[^""]+""|\[[^\]]+\]|`[^`]+`|\w+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex ConstraintPattern();

    public static IReadOnlyDictionary<string, string> Parse(string? createSql)
    {
        Dictionary<string, string> names = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase
        );
        if (string.IsNullOrWhiteSpace(createSql))
        {
            return names;
        }

        foreach (Match match in ConstraintPattern().Matches(createSql))
        {
            string key = BuildKey(
                match.Groups["Columns"].Value.Split(',').Select(Unquote),
                Unquote(match.Groups["Table"].Value)
            );
            names.TryAdd(key, Unquote(match.Groups["Name"].Value));
        }

        return names;
    }

    public static string BuildKey(IEnumerable<string> columns, string referencedTable)
    {
        string joined = string.Join(
            ',',
            columns.Select((string column) => column.ToLowerInvariant())
        );
        return $"{joined}->{referencedTable.ToLowerInvariant()}";
    }

    private static string Unquote(string identifier)
    {
        string trimmed = identifier.Trim();
        if (trimmed.Length >= 2)
        {
            char first = trimmed[0];
            char last = trimmed[^1];
            if (
                (first == '"' && last == '"')
                || (first == '`' && last == '`')
                || (first == '[' && last == ']')
            )
            {
                return trimmed[1..^1];
            }
        }

        return trimmed;
    }
}
