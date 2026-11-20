using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using LearnSql.Sql.Generated;

namespace LearnSql.Tests.Support;

public sealed record ErrorMessageExample(
    string Message,
    IReadOnlyDictionary<string, string> Arguments,
    bool HasAdjacentGroups
)
{
    private static readonly Type MessagesType =
        typeof(SqliteErrorMapper).Assembly.GetType("LearnSql.Sql.Generated.SqliteErrorMessages")
        ?? throw new InvalidOperationException("The generated message catalog was not found.");

    public static ErrorMessageExample Create(SqliteErrorType type)
    {
        MethodInfo method =
            MessagesType.GetMethod(type.ToString(), BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"No pattern exists for '{type}'.");
        Regex regex = (Regex)method.Invoke(null, null)!;
        return FromPattern(regex.ToString());
    }

    public static ErrorMessageExample FromPattern(string pattern)
    {
        string body = pattern;
        if (body.StartsWith('^'))
            body = body[1..];
        if (body.EndsWith('$'))
            body = body[..^1];

        Dictionary<string, string> arguments = new Dictionary<string, string>();
        string message = Synthesize(body, arguments);
        return new ErrorMessageExample(message, arguments, pattern.Contains(")(?<"));
    }

    private static string Synthesize(string pattern, Dictionary<string, string> arguments)
    {
        StringBuilder builder = new StringBuilder();
        int index = 0;
        while (index < pattern.Length)
        {
            if (TryConsumeToken(pattern, ref index, builder))
                continue;

            char current = pattern[index];
            if (current == '\\')
            {
                builder.Append(pattern[index + 1]);
                index += 2;
            }
            else if (pattern.AsSpan(index).StartsWith("(?<"))
            {
                int nameEnd = pattern.IndexOf('>', index);
                string name = pattern.Substring(index + 3, nameEnd - index - 3);
                int close = FindClosingParenthesis(pattern, index);
                string inner = FirstAlternative(
                    pattern.Substring(nameEnd + 1, close - nameEnd - 1)
                );
                string value = Synthesize(inner, arguments);
                if (inner == ".+?" || inner == ".+")
                    value = "sample" + name;
                arguments[name] = value;
                builder.Append(value);
                index = close + 1;
            }
            else if (pattern.AsSpan(index).StartsWith("(?:"))
            {
                int close = FindClosingParenthesis(pattern, index);
                string inner = pattern.Substring(index + 3, close - index - 3);
                builder.Append(Synthesize(FirstAlternative(inner), arguments));
                index = close + 1;
            }
            else
            {
                builder.Append(current);
                index++;
            }
        }

        return builder.ToString();
    }

    private static bool TryConsumeToken(string pattern, ref int index, StringBuilder builder)
    {
        (string Token, string Text)[] tokens =
        [
            (@"\d+(?:st|nd|rd|th)", "2nd"),
            ("-?\\d+", "7"),
            ("[^.,]+", "sampleTable"),
            ("[ ]?", string.Empty),
            (".+?", "sample"),
            (".*?", string.Empty),
            (".+", "sample"),
        ];

        foreach ((string token, string text) in tokens)
        {
            if (pattern.AsSpan(index).StartsWith(token, StringComparison.Ordinal))
            {
                builder.Append(text);
                index += token.Length;
                return true;
            }
        }

        return false;
    }

    private static int FindClosingParenthesis(string pattern, int open)
    {
        int depth = 0;
        for (int index = open; index < pattern.Length; index++)
        {
            if (pattern[index] == '\\')
            {
                index++;
            }
            else if (pattern[index] == '(')
            {
                depth++;
            }
            else if (pattern[index] == ')')
            {
                depth--;
                if (depth == 0)
                    return index;
            }
        }

        throw new InvalidOperationException($"Unbalanced parentheses in '{pattern}'.");
    }

    private static string FirstAlternative(string inner)
    {
        int depth = 0;
        for (int index = 0; index < inner.Length; index++)
        {
            if (inner[index] == '\\')
                index++;
            else if (inner[index] == '(')
                depth++;
            else if (inner[index] == ')')
                depth--;
            else if (inner[index] == '|' && depth == 0)
                return inner[..index];
        }

        return inner;
    }
}
