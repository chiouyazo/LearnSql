namespace LearnSql.Sql.Dialect.Batching;

public sealed class BatchSplitter
{
    private enum ScanState
    {
        Code,
        StringLiteral,
        QuotedIdentifier,
        BracketIdentifier,
        BlockComment,
    }

    public IReadOnlyList<SqlBatchSource> Split(string sql)
    {
        List<SqlBatchSource> batches = [];
        ScanState state = ScanState.Code;
        int commentDepth = 0;
        int batchStart = 0;
        int lineStart = 0;

        while (lineStart < sql.Length)
        {
            int lineEnd = FindLineEnd(sql, lineStart);
            int nextLineStart = SkipLineBreak(sql, lineEnd);

            if (
                state == ScanState.Code
                && TryParseGoLine(sql, lineStart, lineEnd, out int repeatCount)
            )
            {
                AddBatch(batches, sql, batchStart, lineStart, repeatCount);
                batchStart = nextLineStart;
            }
            else
            {
                state = ScanLine(sql, lineStart, lineEnd, state, ref commentDepth);
            }

            lineStart = nextLineStart;
        }

        AddBatch(batches, sql, batchStart, sql.Length, 1);
        return batches;
    }

    private static void AddBatch(
        List<SqlBatchSource> batches,
        string sql,
        int start,
        int end,
        int repeatCount
    )
    {
        if (end <= start)
            return;

        string text = sql.Substring(start, end - start);
        if (string.IsNullOrWhiteSpace(text))
            return;

        batches.Add(new SqlBatchSource(text, start, repeatCount));
    }

    private static int FindLineEnd(string sql, int from)
    {
        int index = from;
        while (index < sql.Length && sql[index] != '\r' && sql[index] != '\n')
            index++;
        return index;
    }

    private static int SkipLineBreak(string sql, int lineEnd)
    {
        if (lineEnd >= sql.Length)
            return sql.Length;
        if (sql[lineEnd] == '\r' && lineEnd + 1 < sql.Length && sql[lineEnd + 1] == '\n')
            return lineEnd + 2;
        return lineEnd + 1;
    }

    private static bool TryParseGoLine(string sql, int lineStart, int lineEnd, out int repeatCount)
    {
        repeatCount = 1;
        int index = SkipBlanks(sql, lineStart, lineEnd);

        if (
            index + 2 > lineEnd
            || !string.Equals(sql.Substring(index, 2), "GO", StringComparison.OrdinalIgnoreCase)
        )
            return false;
        index += 2;

        if (index < lineEnd && IsIdentifierPart(sql[index]))
            return false;

        index = SkipBlanks(sql, index, lineEnd);

        int digitsStart = index;
        while (index < lineEnd && char.IsAsciiDigit(sql[index]))
            index++;

        if (index > digitsStart)
        {
            if (
                !int.TryParse(sql.AsSpan(digitsStart, index - digitsStart), out int parsed)
                || parsed < 1
            )
                return false;
            repeatCount = parsed;
            index = SkipBlanks(sql, index, lineEnd);
        }

        if (index >= lineEnd)
            return true;

        return index + 1 < lineEnd && sql[index] == '-' && sql[index + 1] == '-';
    }

    private static int SkipBlanks(string sql, int from, int end)
    {
        int index = from;
        while (index < end && (sql[index] == ' ' || sql[index] == '\t'))
            index++;
        return index;
    }

    private static bool IsIdentifierPart(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '@' || c == '#' || c == '$';
    }

    private static ScanState ScanLine(
        string sql,
        int lineStart,
        int lineEnd,
        ScanState state,
        ref int commentDepth
    )
    {
        int index = lineStart;
        while (index < lineEnd)
        {
            char current = sql[index];
            bool hasNext = index + 1 < lineEnd;
            char next = hasNext ? sql[index + 1] : '\0';

            switch (state)
            {
                case ScanState.Code:
                    if (current == '\'')
                        state = ScanState.StringLiteral;
                    else if (current == '"')
                        state = ScanState.QuotedIdentifier;
                    else if (current == '[')
                        state = ScanState.BracketIdentifier;
                    else if (current == '-' && next == '-')
                        return ScanState.Code;
                    else if (current == '/' && next == '*')
                    {
                        state = ScanState.BlockComment;
                        commentDepth = 1;
                        index++;
                    }
                    break;
                case ScanState.StringLiteral:
                    state = ContinueQuoted(current, '\'', next, hasNext, ref index, state);
                    break;
                case ScanState.QuotedIdentifier:
                    state = ContinueQuoted(current, '"', next, hasNext, ref index, state);
                    break;
                case ScanState.BracketIdentifier:
                    state = ContinueQuoted(current, ']', next, hasNext, ref index, state);
                    break;
                case ScanState.BlockComment:
                    if (current == '/' && next == '*')
                    {
                        commentDepth++;
                        index++;
                    }
                    else if (current == '*' && next == '/')
                    {
                        commentDepth--;
                        index++;
                        if (commentDepth == 0)
                            state = ScanState.Code;
                    }
                    break;
            }

            index++;
        }

        return state;
    }

    private static ScanState ContinueQuoted(
        char current,
        char closing,
        char next,
        bool hasNext,
        ref int index,
        ScanState state
    )
    {
        if (current != closing)
            return state;

        if (hasNext && next == closing)
        {
            index++;
            return state;
        }

        return ScanState.Code;
    }
}
