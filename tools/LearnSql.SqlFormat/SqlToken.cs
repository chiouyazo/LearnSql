using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.SqlFormat;

internal sealed record SqlToken(string Text, TSqlTokenType Type, bool SpaceBeforeParen = false)
{
    private static readonly HashSet<TSqlTokenType> NonKeywordTypes =
    [
        TSqlTokenType.Identifier,
        TSqlTokenType.QuotedIdentifier,
        TSqlTokenType.AsciiStringLiteral,
        TSqlTokenType.UnicodeStringLiteral,
        TSqlTokenType.AsciiStringOrQuotedIdentifier,
        TSqlTokenType.Variable,
        TSqlTokenType.Integer,
        TSqlTokenType.Real,
        TSqlTokenType.Numeric,
        TSqlTokenType.Money,
        TSqlTokenType.HexLiteral,
        TSqlTokenType.Label,
        TSqlTokenType.PseudoColumn,
        TSqlTokenType.SqlCommandIdentifier,
        TSqlTokenType.Go,
    ];

    private static readonly HashSet<TSqlTokenType> OperatorTypes =
    [
        TSqlTokenType.Bang,
        TSqlTokenType.PercentSign,
        TSqlTokenType.Ampersand,
        TSqlTokenType.Star,
        TSqlTokenType.MultiplyEquals,
        TSqlTokenType.Plus,
        TSqlTokenType.Minus,
        TSqlTokenType.Divide,
        TSqlTokenType.LessThan,
        TSqlTokenType.EqualsSign,
        TSqlTokenType.GreaterThan,
        TSqlTokenType.Circumflex,
        TSqlTokenType.VerticalLine,
        TSqlTokenType.Tilde,
        TSqlTokenType.AddEquals,
        TSqlTokenType.SubtractEquals,
        TSqlTokenType.DivideEquals,
        TSqlTokenType.ModEquals,
        TSqlTokenType.BitwiseAndEquals,
        TSqlTokenType.BitwiseOrEquals,
        TSqlTokenType.BitwiseXorEquals,
        TSqlTokenType.LeftShift,
        TSqlTokenType.RightShift,
        TSqlTokenType.Concat,
        TSqlTokenType.ConcatEquals,
        TSqlTokenType.TSEqual,
    ];

    private static readonly HashSet<TSqlTokenType> FunctionKeywordTypes =
    [
        TSqlTokenType.Left,
        TSqlTokenType.Right,
        TSqlTokenType.Coalesce,
        TSqlTokenType.NullIf,
        TSqlTokenType.Convert,
        TSqlTokenType.TryConvert,
        TSqlTokenType.Identity,
    ];

    public bool IsOpen => Type == TSqlTokenType.LeftParenthesis;

    public bool IsClose => Type == TSqlTokenType.RightParenthesis;

    public bool IsComma => Type == TSqlTokenType.Comma;

    public bool IsSemicolon => Type == TSqlTokenType.Semicolon;

    public bool IsDot => Type == TSqlTokenType.Dot;

    public bool IsOperator => OperatorTypes.Contains(Type);

    public bool IsKeyword =>
        !NonKeywordTypes.Contains(Type)
        && Text.Length > 0
        && Text.All((char c) => char.IsAsciiLetter(c) || c == '_');

    public bool IsFunctionLike =>
        Type is TSqlTokenType.Identifier or TSqlTokenType.QuotedIdentifier
        || FunctionKeywordTypes.Contains(Type);

    public bool Is(TSqlTokenType type)
    {
        return Type == type;
    }

    public bool IsWord(string word)
    {
        return (Type == TSqlTokenType.Identifier || IsKeyword)
            && string.Equals(Text, word, StringComparison.OrdinalIgnoreCase);
    }
}
