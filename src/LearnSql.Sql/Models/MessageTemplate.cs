using System.Text.RegularExpressions;

namespace LearnSql.Sql.Models;

public static partial class MessageTemplate
{
    [GeneratedRegex(@"\{(?<Name>[A-Za-z][A-Za-z0-9]*)\}")]
    private static partial Regex PlaceholderPattern();

    public static IReadOnlyList<string> GetPlaceholders(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        return
        [
            .. PlaceholderPattern()
                .Matches(template)
                .Select((Match match) => match.Groups["Name"].Value)
                .Distinct(StringComparer.Ordinal),
        ];
    }

    public static bool TryRender(
        string template,
        IReadOnlyDictionary<string, string> arguments,
        out string text
    )
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(arguments);

        bool complete = true;
        string rendered = PlaceholderPattern()
            .Replace(
                template,
                (Match match) =>
                {
                    string name = match.Groups["Name"].Value;
                    if (
                        arguments.TryGetValue(name, out string? value)
                        && !string.IsNullOrWhiteSpace(value)
                    )
                    {
                        return value;
                    }

                    complete = false;
                    return match.Value;
                }
            );

        text = complete ? rendered : string.Empty;
        return complete;
    }
}
