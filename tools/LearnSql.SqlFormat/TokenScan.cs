using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.SqlFormat;

internal static class TokenScan
{
    public static List<SqlToken> Slice(IReadOnlyList<SqlToken> tokens, int start, int end)
    {
        List<SqlToken> result = [];
        for (int index = start; index < end; index++)
            result.Add(tokens[index]);
        return result;
    }

    public static List<SqlToken> From(IReadOnlyList<SqlToken> tokens, int start)
    {
        return Slice(tokens, start, tokens.Count);
    }

    public static int MatchingClose(IReadOnlyList<SqlToken> tokens, int openIndex)
    {
        int depth = 0;
        for (int index = openIndex; index < tokens.Count; index++)
        {
            if (tokens[index].IsOpen)
                depth++;
            else if (tokens[index].IsClose && --depth == 0)
                return index;
        }

        throw new FormatException("The parentheses are not balanced.");
    }

    public static int MatchingEnd(IReadOnlyList<SqlToken> tokens, int caseIndex)
    {
        int depth = 0;
        for (int index = caseIndex; index < tokens.Count; index++)
        {
            if (tokens[index].Is(TSqlTokenType.Case))
                depth++;
            else if (tokens[index].Is(TSqlTokenType.End) && --depth == 0)
                return index;
        }

        throw new FormatException("A CASE expression has no matching END.");
    }

    public static List<int> TopLevel(IReadOnlyList<SqlToken> tokens)
    {
        List<int> result = [];
        int parentheses = 0;
        int cases = 0;
        for (int index = 0; index < tokens.Count; index++)
        {
            SqlToken token = tokens[index];
            if (token.IsClose)
            {
                parentheses--;
                continue;
            }

            if (token.Is(TSqlTokenType.End) && cases > 0)
            {
                cases--;
                continue;
            }

            if (parentheses == 0 && cases == 0)
                result.Add(index);
            if (token.IsOpen)
                parentheses++;
            else if (token.Is(TSqlTokenType.Case))
                cases++;
        }

        return result;
    }

    public static List<List<SqlToken>> SplitAtCommas(IReadOnlyList<SqlToken> tokens)
    {
        List<List<SqlToken>> parts = [];
        int start = 0;
        foreach (int index in TopLevel(tokens))
        {
            if (!tokens[index].IsComma)
                continue;
            parts.Add(Slice(tokens, start, index));
            start = index + 1;
        }

        if (start < tokens.Count || parts.Count > 0)
            parts.Add(Slice(tokens, start, tokens.Count));
        return parts;
    }

    public static List<List<SqlToken>> SplitBefore(
        IReadOnlyList<SqlToken> tokens,
        Func<int, bool> startsPart
    )
    {
        List<List<SqlToken>> parts = [];
        int start = 0;
        foreach (int index in TopLevel(tokens))
        {
            if (index == 0 || !startsPart(index))
                continue;
            parts.Add(Slice(tokens, start, index));
            start = index;
        }

        if (tokens.Count > 0)
            parts.Add(Slice(tokens, start, tokens.Count));
        return parts;
    }

    public static List<List<SqlToken>> SplitConditions(IReadOnlyList<SqlToken> tokens)
    {
        List<List<SqlToken>> parts = [];
        int start = 0;
        int betweens = 0;
        foreach (int index in TopLevel(tokens))
        {
            SqlToken token = tokens[index];
            if (token.Is(TSqlTokenType.Between))
            {
                betweens++;
            }
            else if (token.Is(TSqlTokenType.And) && betweens > 0)
            {
                betweens--;
            }
            else if (index > 0 && (token.Is(TSqlTokenType.And) || token.Is(TSqlTokenType.Or)))
            {
                parts.Add(Slice(tokens, start, index));
                start = index;
            }
        }

        if (tokens.Count > 0)
            parts.Add(Slice(tokens, start, tokens.Count));
        return parts;
    }
}
