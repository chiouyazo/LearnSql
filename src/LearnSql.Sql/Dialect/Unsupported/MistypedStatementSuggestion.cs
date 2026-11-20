using LearnSql.Sql.Resources;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Unsupported;

public static class MistypedStatementSuggestion
{
    private const int MinimumWordLength = 4;

    private static readonly string[] Keywords =
    [
        "SELECT",
        "INSERT",
        "UPDATE",
        "DELETE",
        "DECLARE",
        "CREATE",
        "ALTER",
        "DROP",
        "TRUNCATE",
        "BEGIN",
        "COMMIT",
        "ROLLBACK",
    ];

    public static string? ForImplicitExecute(TSqlFragment fragment)
    {
        if (fragment is not ExecuteStatement statement || HasExecKeyword(statement))
            return null;

        string? name = (
            statement.ExecuteSpecification?.ExecutableEntity as ExecutableProcedureReference
        )
            ?.ProcedureReference
            ?.ProcedureReference
            ?.Name
            ?.BaseIdentifier
            ?.Value;
        string? keyword = name is null ? null : FindKeyword(name);
        return keyword is null ? null : DialectStrings.MeantKeywordForImplicitExecute(keyword);
    }

    public static string? ForParseError(string text, int offset, int length)
    {
        if (length == 0 || !StartsStatement(text, offset))
            return null;

        string? keyword = FindKeyword(text.Substring(offset, length));
        return keyword is null ? null : DialectStrings.MeantKeyword(keyword);
    }

    public static string? FindKeyword(string word)
    {
        if (word.Length < MinimumWordLength)
            return null;

        return Keywords.FirstOrDefault(keyword => IsOneEditAway(word.ToUpperInvariant(), keyword));
    }

    private static bool StartsStatement(string text, int offset)
    {
        int index = offset - 1;
        while (index >= 0 && char.IsWhiteSpace(text[index]))
            index--;
        return index < 0 || text[index] == ';';
    }

    private static bool HasExecKeyword(ExecuteStatement statement)
    {
        TSqlParserToken first = statement.ScriptTokenStream[statement.FirstTokenIndex];
        return first.TokenType is TSqlTokenType.Exec or TSqlTokenType.Execute;
    }

    private static bool IsOneEditAway(string first, string second)
    {
        if (first == second || Math.Abs(first.Length - second.Length) > 1)
            return false;

        int prefix = 0;
        int limit = Math.Min(first.Length, second.Length);
        while (prefix < limit && first[prefix] == second[prefix])
            prefix++;

        if (first.Length == second.Length)
        {
            bool substitution = first.AsSpan(prefix + 1).SequenceEqual(second.AsSpan(prefix + 1));
            bool transposition =
                prefix + 1 < first.Length
                && first[prefix] == second[prefix + 1]
                && first[prefix + 1] == second[prefix]
                && first.AsSpan(prefix + 2).SequenceEqual(second.AsSpan(prefix + 2));
            return substitution || transposition;
        }

        string longer = first.Length > second.Length ? first : second;
        string shorter = first.Length > second.Length ? second : first;
        return longer.AsSpan(prefix + 1).SequenceEqual(shorter.AsSpan(prefix));
    }
}
