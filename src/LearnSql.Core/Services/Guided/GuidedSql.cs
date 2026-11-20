using System.Text.RegularExpressions;

namespace LearnSql.Core.Services.Guided;

public static partial class GuidedSql
{
    public static string Normalize(string? sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return string.Empty;
        }

        string collapsed = WhitespaceRun().Replace(sql, " ").Trim();
        return collapsed.TrimEnd(';', ' ');
    }

    public static bool Matches(string? left, string? right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRun();
}
