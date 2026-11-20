using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.SqlFormat;

internal static class SqlTokenizer
{
    private static readonly HashSet<string> FunctionNames = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "COUNT",
        "SUM",
        "AVG",
        "MIN",
        "MAX",
        "ROUND",
        "COALESCE",
        "ISNULL",
        "NULLIF",
        "LEN",
        "SUBSTRING",
        "UPPER",
        "LOWER",
        "TRIM",
        "LTRIM",
        "RTRIM",
        "REPLACE",
        "CONCAT",
        "CONCAT_WS",
        "GETDATE",
        "SYSDATETIME",
        "DATEADD",
        "DATEDIFF",
        "DATEPART",
        "DATENAME",
        "YEAR",
        "MONTH",
        "DAY",
        "EOMONTH",
        "ABS",
        "CEILING",
        "FLOOR",
        "POWER",
        "SQRT",
        "SIGN",
        "ROW_NUMBER",
        "RANK",
        "DENSE_RANK",
        "NTILE",
        "LAG",
        "LEAD",
        "FIRST_VALUE",
        "LAST_VALUE",
        "CAST",
        "CONVERT",
        "TRY_CAST",
        "TRY_CONVERT",
        "IIF",
        "CHARINDEX",
        "STRING_AGG",
        "STUFF",
        "REVERSE",
        "FORMAT",
        "PATINDEX",
        "CHOOSE",
    };

    private static readonly HashSet<string> TypeNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "INT",
        "INTEGER",
        "BIGINT",
        "SMALLINT",
        "TINYINT",
        "BIT",
        "DECIMAL",
        "NUMERIC",
        "FLOAT",
        "REAL",
        "MONEY",
        "SMALLMONEY",
        "DATE",
        "DATETIME",
        "DATETIME2",
        "SMALLDATETIME",
        "DATETIMEOFFSET",
        "TIME",
        "CHAR",
        "VARCHAR",
        "NCHAR",
        "NVARCHAR",
        "TEXT",
        "NTEXT",
        "BINARY",
        "VARBINARY",
        "UNIQUEIDENTIFIER",
    };

    private static readonly HashSet<string> OffsetWords = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "NEXT",
        "FIRST",
        "ROWS",
        "ROW",
        "ONLY",
    };

    private static readonly HashSet<string> FrameModes = new HashSet<string>(StringComparer.Ordinal)
    {
        "ROWS",
        "RANGE",
        "GROUPS",
    };

    private static readonly HashSet<string> SpacedCallWords = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "APPLY",
        "CLUSTERED",
        "NONCLUSTERED",
    };

    private static readonly Dictionary<(TSqlTokenType, TSqlTokenType), string> GluedOperators =
        new Dictionary<(TSqlTokenType, TSqlTokenType), string>
        {
            [(TSqlTokenType.LessThan, TSqlTokenType.GreaterThan)] = "<>",
            [(TSqlTokenType.LessThan, TSqlTokenType.EqualsSign)] = "<=",
            [(TSqlTokenType.GreaterThan, TSqlTokenType.EqualsSign)] = ">=",
            [(TSqlTokenType.Bang, TSqlTokenType.EqualsSign)] = "!=",
            [(TSqlTokenType.Bang, TSqlTokenType.LessThan)] = "!<",
            [(TSqlTokenType.Bang, TSqlTokenType.GreaterThan)] = "!>",
        };

    public static List<SqlStatement> Tokenize(string sql)
    {
        List<SqlToken> tokens = [];
        List<bool> breaksBefore = [];
        List<bool> breaksAfter = [];
        ReadTokens(sql, tokens, breaksBefore, breaksAfter);

        List<SqlStatement> statements = [];
        List<SqlToken> current = [];
        int index = 0;
        while (index < tokens.Count)
        {
            SqlToken token = tokens[index];
            if (IsBatchSeparator(tokens, breaksBefore, breaksAfter, index))
            {
                Flush(statements, current);
                List<SqlToken> separator = [token];
                if (index + 1 < tokens.Count && tokens[index + 1].Is(TSqlTokenType.Integer))
                {
                    separator.Add(tokens[index + 1]);
                    index++;
                }

                statements.Add(new SqlStatement(separator, true));
            }
            else
            {
                current.Add(token);
                if (token.IsSemicolon)
                    Flush(statements, current);
            }

            index++;
        }

        Flush(statements, current);
        return statements;
    }

    public static List<string> Fingerprint(IEnumerable<SqlStatement> statements)
    {
        List<string> result = [];
        foreach (SqlStatement statement in statements)
        {
            foreach (SqlToken token in statement.Tokens)
            {
                bool caseInsensitive = token.Type == TSqlTokenType.Identifier || token.IsKeyword;
                result.Add(caseInsensitive ? token.Text.ToUpperInvariant() : token.Text);
            }
        }

        return result;
    }

    private static void Flush(List<SqlStatement> statements, List<SqlToken> current)
    {
        if (current.Count == 0)
            return;
        statements.Add(new SqlStatement(Annotate(current), false));
        current.Clear();
    }

    private static bool IsBatchSeparator(
        List<SqlToken> tokens,
        List<bool> breaksBefore,
        List<bool> breaksAfter,
        int index
    )
    {
        SqlToken token = tokens[index];
        if (token.Type != TSqlTokenType.Identifier && token.Type != TSqlTokenType.Go)
            return false;
        if (!string.Equals(token.Text, "GO", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!breaksBefore[index])
            return false;
        if (breaksAfter[index])
            return true;
        return index + 1 < tokens.Count
            && tokens[index + 1].Is(TSqlTokenType.Integer)
            && breaksAfter[index + 1];
    }

    private static void ReadTokens(
        string sql,
        List<SqlToken> tokens,
        List<bool> breaksBefore,
        List<bool> breaksAfter
    )
    {
        TSqlParser parser = TSqlParser.CreateParser(SqlVersion.Sql180, true);
        using StringReader reader = new StringReader(sql);
        IList<TSqlParserToken> raw = parser.GetTokenStream(reader, out IList<ParseError> errors);
        if (errors.Count > 0)
        {
            throw new FormatException(
                $"The SQL could not be tokenized (line {errors[0].Line}, column {errors[0].Column}): {errors[0].Message}"
            );
        }

        bool lineBreakPending = true;
        TSqlParserToken? previous = null;
        for (int index = 0; index < raw.Count; index++)
        {
            TSqlParserToken token = raw[index];
            switch (token.TokenType)
            {
                case TSqlTokenType.EndOfFile:
                    MarkBreakAfter(breaksAfter, tokens, true);
                    break;
                case TSqlTokenType.WhiteSpace:
                    if (token.Text.Contains('\n'))
                    {
                        MarkBreakAfter(breaksAfter, tokens, true);
                        lineBreakPending = true;
                    }

                    break;
                case TSqlTokenType.SingleLineComment:
                case TSqlTokenType.MultilineComment:
                    throw new FormatException(
                        $"Comments are not supported (line {token.Line}, column {token.Column})."
                    );
                default:
                    AddToken(tokens, breaksBefore, breaksAfter, token, previous, lineBreakPending);
                    lineBreakPending = false;
                    break;
            }

            if (token.TokenType != TSqlTokenType.WhiteSpace)
                previous = token;
        }
    }

    private static void MarkBreakAfter(List<bool> breaksAfter, List<SqlToken> tokens, bool value)
    {
        if (breaksAfter.Count > 0 && tokens.Count == breaksAfter.Count)
            breaksAfter[^1] = value;
    }

    private static void AddToken(
        List<SqlToken> tokens,
        List<bool> breaksBefore,
        List<bool> breaksAfter,
        TSqlParserToken token,
        TSqlParserToken? previous,
        bool lineBreakPending
    )
    {
        bool adjacent =
            previous is not null
            && previous.Offset + previous.Text.Length == token.Offset
            && tokens.Count > 0;
        if (
            adjacent
            && GluedOperators.TryGetValue((previous!.TokenType, token.TokenType), out string? glued)
        )
        {
            tokens[^1] = new SqlToken(glued, tokens[^1].Type);
            return;
        }

        tokens.Add(new SqlToken(token.Text, token.TokenType));
        breaksBefore.Add(lineBreakPending);
        breaksAfter.Add(false);
    }

    private static List<SqlToken> Annotate(List<SqlToken> tokens)
    {
        TokenAnnotator annotator = new TokenAnnotator(tokens);
        return annotator.Annotate();
    }

    private sealed class TokenAnnotator
    {
        private readonly List<SqlToken> _tokens;
        private readonly Stack<Frame> _frames = new Stack<Frame>();
        private readonly bool _startsWithWith;
        private readonly bool _createsTable;
        private readonly bool _createsObject;
        private bool _tableOpened;
        private bool _offsetMode;
        private bool _mainStatementSeen;

        public TokenAnnotator(List<SqlToken> tokens)
        {
            _tokens = tokens;
            _startsWithWith = tokens[0].Is(TSqlTokenType.With);
            _createsObject = tokens[0].Is(TSqlTokenType.Create);
            _createsTable = _createsObject && tokens.Count > 1 && tokens[1].Is(TSqlTokenType.Table);
        }

        public List<SqlToken> Annotate()
        {
            List<SqlToken> result = [];
            for (int index = 0; index < _tokens.Count; index++)
                result.Add(AnnotateToken(index));
            return result;
        }

        private SqlToken AnnotateToken(int index)
        {
            SqlToken token = _tokens[index];
            SqlToken? previous = index > 0 ? _tokens[index - 1] : null;
            SqlToken? next = index + 1 < _tokens.Count ? _tokens[index + 1] : null;
            Frame? top = _frames.Count > 0 ? _frames.Peek() : null;
            string text = token.Text;
            bool spaceBeforeParen = false;

            if (token.IsKeyword)
                text = text.ToUpperInvariant();
            else if (token.Type == TSqlTokenType.Identifier && previous?.IsDot != true)
                text = CaseIdentifier(index, previous, next, top);

            if (_frames.Count == 0 && IsMainStatementStart(token))
                _mainStatementSeen = true;

            if (token.IsOpen)
            {
                spaceBeforeParen = ComputeSpaceBeforeParen(index, previous);
                _frames.Push(new Frame(FrameKindFor(previous)));
            }
            else if (token.IsClose && _frames.Count > 0)
            {
                _frames.Pop();
            }
            else if (top is not null && top.Kind == FrameKind.TableDefinitions)
            {
                top.Position = token.IsComma ? 0 : top.Position + 1;
            }

            if (token.IsOpen && top is not null && top.Kind == FrameKind.TableDefinitions)
                top.Position++;

            return new SqlToken(text, token.Type, spaceBeforeParen);
        }

        private static bool IsMainStatementStart(SqlToken token)
        {
            return token.Type
                is TSqlTokenType.Select
                    or TSqlTokenType.Insert
                    or TSqlTokenType.Update
                    or TSqlTokenType.Delete;
        }

        private FrameKind FrameKindFor(SqlToken? previous)
        {
            if (previous is null)
                return FrameKind.Plain;
            if (previous.Is(TSqlTokenType.Over))
                return FrameKind.Over;
            if (previous.IsWord("CAST") || previous.IsWord("TRY_CAST"))
                return FrameKind.Cast;
            if (previous.IsWord("CONVERT") || previous.IsWord("TRY_CONVERT"))
                return FrameKind.Convert;
            if (_createsTable && !_tableOpened && _frames.Count == 0)
            {
                _tableOpened = true;
                return FrameKind.TableDefinitions;
            }

            return FrameKind.Plain;
        }

        private bool ComputeSpaceBeforeParen(int index, SqlToken? previous)
        {
            if (
                previous is null
                || previous.Type is not (TSqlTokenType.Identifier or TSqlTokenType.QuotedIdentifier)
            )
                return false;
            if (
                previous.Type == TSqlTokenType.Identifier
                && SpacedCallWords.Contains(previous.Text.ToUpperInvariant())
            )
                return true;

            int start = index - 1;
            while (
                start >= 2
                && _tokens[start - 1].IsDot
                && _tokens[start - 2].Type
                    is TSqlTokenType.Identifier
                        or TSqlTokenType.QuotedIdentifier
            )
                start -= 2;
            SqlToken? context = start > 0 ? _tokens[start - 1] : null;
            if (context is null)
                return false;
            if (
                context.Type
                is TSqlTokenType.Into
                    or TSqlTokenType.Table
                    or TSqlTokenType.References
                    or TSqlTokenType.View
            )
                return true;
            if (_createsObject && context.Type is TSqlTokenType.On or TSqlTokenType.Index)
                return true;
            return _startsWithWith
                && !_mainStatementSeen
                && _frames.Count == 0
                && context.Type is TSqlTokenType.With or TSqlTokenType.Comma;
        }

        private string CaseIdentifier(int index, SqlToken? previous, SqlToken? next, Frame? top)
        {
            string text = _tokens[index].Text;
            string upper = text.ToUpperInvariant();
            if (next is not null && next.IsOpen && FunctionNames.Contains(upper))
                return upper;
            if (TypeNames.Contains(upper) && IsTypePosition(index, previous, top))
                return upper;
            if (upper == "MAX" && IsMaxLength(index, previous, next))
                return upper;
            if (IsContextWord(upper, previous, next, top))
                return upper;
            return text;
        }

        private bool IsTypePosition(int index, SqlToken? previous, Frame? top)
        {
            if (previous is null)
                return false;
            if (previous.Is(TSqlTokenType.Variable))
                return true;
            if (previous.Is(TSqlTokenType.As))
            {
                return top?.Kind == FrameKind.Cast
                    || (index >= 2 && _tokens[index - 2].Is(TSqlTokenType.Variable));
            }

            if (top?.Kind == FrameKind.Convert && previous.IsOpen)
                return true;
            return top?.Kind == FrameKind.TableDefinitions && top.Position == 1;
        }

        private bool IsMaxLength(int index, SqlToken? previous, SqlToken? next)
        {
            return previous is not null
                && previous.IsOpen
                && next is not null
                && next.IsClose
                && index >= 2
                && _tokens[index - 2].Type == TSqlTokenType.Identifier
                && TypeNames.Contains(_tokens[index - 2].Text.ToUpperInvariant());
        }

        private bool IsContextWord(string upper, SqlToken? previous, SqlToken? next, Frame? top)
        {
            if (
                upper == "OFFSET"
                && _frames.Count == 0
                && previous is not null
                && !previous.IsComma
                && !previous.Is(TSqlTokenType.Select)
                && next is not null
                && (next.Type is TSqlTokenType.Integer or TSqlTokenType.Variable || next.IsOpen)
            )
            {
                _offsetMode = true;
                return true;
            }

            if (_offsetMode && _frames.Count == 0 && OffsetWords.Contains(upper))
            {
                if (upper == "ONLY")
                    _offsetMode = false;
                return true;
            }

            if (upper == "TIES" && previous is not null && previous.Is(TSqlTokenType.With))
                return true;
            if (
                upper == "APPLY"
                && previous is not null
                && previous.Type is TSqlTokenType.Cross or TSqlTokenType.Outer
            )
                return true;
            return top?.Kind == FrameKind.Over && IsWindowWord(upper, previous, next);
        }

        private static bool IsWindowWord(string upper, SqlToken? previous, SqlToken? next)
        {
            if (upper == "PARTITION")
                return next is not null && next.Is(TSqlTokenType.By);
            if (FrameModes.Contains(upper))
            {
                return next is not null
                    && (
                        next.Type
                            is TSqlTokenType.Between
                                or TSqlTokenType.Integer
                                or TSqlTokenType.Current
                        || next.IsWord("UNBOUNDED")
                    );
            }

            if (upper == "UNBOUNDED")
                return next is not null && (next.IsWord("PRECEDING") || next.IsWord("FOLLOWING"));
            if (upper is "PRECEDING" or "FOLLOWING")
            {
                return previous is not null
                    && (previous.Is(TSqlTokenType.Integer) || previous.IsWord("UNBOUNDED"));
            }

            return upper == "ROW" && previous is not null && previous.Is(TSqlTokenType.Current);
        }
    }

    private sealed class Frame
    {
        public Frame(FrameKind kind)
        {
            Kind = kind;
        }

        public FrameKind Kind { get; }

        public int Position { get; set; }
    }

    private enum FrameKind
    {
        Plain,
        Over,
        Cast,
        Convert,
        TableDefinitions,
    }
}
