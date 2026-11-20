using System.Text.RegularExpressions;
using LearnSql.Sql.Models;

namespace LearnSql.Sql.MsSql;

public sealed partial class MsSqlRaisedException : Exception
{
    private const string MarkerPrefix = " [mssql:";
    private const string MarkerSuffix = "]";

    public MsSqlRaisedException(
        MsSqlRaisedErrorKind kind,
        IReadOnlyDictionary<string, string>? arguments = null
    )
        : base(Render(kind, arguments ?? EmptyArguments))
    {
        Kind = kind;
        Arguments = arguments ?? EmptyArguments;
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyArguments =
        new Dictionary<string, string>();

    public MsSqlRaisedErrorKind Kind { get; }

    public IReadOnlyDictionary<string, string> Arguments { get; }

    public static bool TryParse(
        string sqliteMessage,
        out MsSqlRaisedErrorKind kind,
        out string text
    )
    {
        ArgumentNullException.ThrowIfNull(sqliteMessage);

        Match match = MarkedMessagePattern().Match(sqliteMessage);
        if (
            match.Success
            && Enum.TryParse(match.Groups["Kind"].Value, false, out MsSqlRaisedErrorKind parsed)
            && Enum.IsDefined(parsed)
        )
        {
            kind = parsed;
            text = match.Groups["Text"].Value;
            return true;
        }

        kind = default;
        text = string.Empty;
        return false;
    }

    [GeneratedRegex(
        @"^(?:SQLite Error \d+: ')?(?<Text>.*) \[mssql:(?<Kind>[A-Za-z]+)\](?:'\.)?$",
        RegexOptions.Singleline
    )]
    private static partial Regex MarkedMessagePattern();

    private static string Render(
        MsSqlRaisedErrorKind kind,
        IReadOnlyDictionary<string, string> arguments
    )
    {
        MsSqlErrorDefinition definition = MsSqlErrorCatalog.GetRaised(kind);
        if (!MessageTemplate.TryRender(definition.Template, arguments, out string text))
        {
            throw new ArgumentException(
                $"The arguments do not satisfy the template of {kind}.",
                nameof(arguments)
            );
        }

        return text + MarkerPrefix + kind + MarkerSuffix;
    }
}
