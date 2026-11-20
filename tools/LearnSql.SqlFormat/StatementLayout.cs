using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.SqlFormat;

internal static class StatementLayout
{
    public const int IndentSize = 4;
    public const int MaxInlineLength = 60;

    private enum ClauseKind
    {
        Head,
        Select,
        Into,
        From,
        Where,
        Having,
        GroupBy,
        OrderBy,
        Offset,
        Fetch,
        Option,
        For,
        Set,
    }

    private sealed record Clause(ClauseKind Kind, List<SqlToken> Tokens);

    public static string Indent(int indent)
    {
        return new string(' ', indent);
    }

    public static string Format(IReadOnlyList<SqlToken> tokens)
    {
        bool terminated = tokens.Count > 0 && tokens[^1].IsSemicolon;
        List<SqlToken> body = TokenScan.Slice(tokens, 0, tokens.Count - (terminated ? 1 : 0));
        List<string> lines = IsShort(body) ? [ExpressionLayout.Flat(body)] : FormatBody(body, 0);
        if (terminated)
        {
            if (lines.Count == 0)
                lines.Add(";");
            else
                lines[^1] += ";";
        }

        return string.Join("\n", lines);
    }

    public static List<string> FormatBody(IReadOnlyList<SqlToken> body, int indent)
    {
        if (body.Count == 0)
            return [];

        SqlToken first = body[0];
        if (first.Is(TSqlTokenType.With))
            return FormatWith(body, indent);
        if (first.Is(TSqlTokenType.Select) || first.IsOpen)
            return FormatQuery(body, indent);
        if (first.Is(TSqlTokenType.Insert))
            return FormatInsert(body, indent);
        if (first.Is(TSqlTokenType.Update))
            return FormatUpdate(body, indent);
        if (first.Is(TSqlTokenType.Delete))
            return FormatDelete(body, indent);
        if (
            body.Count > 2
            && first.Type is TSqlTokenType.Create or TSqlTokenType.Alter
            && body[1].Is(TSqlTokenType.View)
        )
            return FormatView(body, indent);
        if (body.Count > 2 && first.Is(TSqlTokenType.Create) && body[1].Is(TSqlTokenType.Table))
            return FormatCreateTable(body, indent);

        return [Indent(indent) + ExpressionLayout.Render(body, indent)];
    }

    public static int CountClauses(IReadOnlyList<SqlToken> query)
    {
        return SplitQueryClauses(query).Count;
    }

    private static bool IsShort(IReadOnlyList<SqlToken> body)
    {
        if (body.Count == 0)
            return false;
        if (body[0].Is(TSqlTokenType.With))
            return false;
        if (body.Count > 1 && body[0].Is(TSqlTokenType.Create) && body[1].Is(TSqlTokenType.Table))
            return false;
        for (int index = 0; index < body.Count; index++)
        {
            if (
                body[index].Type
                is TSqlTokenType.Case
                    or TSqlTokenType.Union
                    or TSqlTokenType.Intersect
                    or TSqlTokenType.Except
            )
                return false;
            if (
                body[index].IsOpen
                && index + 1 < body.Count
                && body[index + 1].Type is TSqlTokenType.Select or TSqlTokenType.With
            )
                return false;
        }

        return ExpressionLayout.Flat(body).Length <= MaxInlineLength;
    }

    private static List<string> FormatQuery(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<string> lines = [];
        int start = 0;
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (
                tokens[index].Type
                is not (TSqlTokenType.Union or TSqlTokenType.Intersect or TSqlTokenType.Except)
            )
                continue;
            lines.AddRange(FormatQuerySpecification(TokenScan.Slice(tokens, start, index), indent));
            int operatorEnd = index + 1;
            if (operatorEnd < tokens.Count && tokens[operatorEnd].Is(TSqlTokenType.All))
                operatorEnd++;
            lines.Add(
                Indent(indent) + ExpressionLayout.Flat(TokenScan.Slice(tokens, index, operatorEnd))
            );
            start = operatorEnd;
        }

        lines.AddRange(
            FormatQuerySpecification(TokenScan.Slice(tokens, start, tokens.Count), indent)
        );
        return lines;
    }

    private static List<string> FormatQuerySpecification(IReadOnlyList<SqlToken> tokens, int indent)
    {
        if (tokens.Count == 0)
            return [];
        if (tokens[0].IsOpen && TokenScan.MatchingClose(tokens, 0) == tokens.Count - 1)
        {
            List<string> wrapped = [Indent(indent) + "("];
            wrapped.AddRange(
                FormatQuery(TokenScan.Slice(tokens, 1, tokens.Count - 1), indent + IndentSize)
            );
            wrapped.Add(Indent(indent) + ")");
            return wrapped;
        }

        List<string> lines = [];
        foreach (Clause clause in SplitQueryClauses(tokens))
            lines.AddRange(FormatClause(clause, indent));
        return lines;
    }

    private static List<Clause> SplitQueryClauses(IReadOnlyList<SqlToken> tokens)
    {
        return SplitClauses(
            tokens,
            (int index, ClauseKind current) => ClassifyQuery(tokens, index, current)
        );
    }

    private static ClauseKind? ClassifyQuery(
        IReadOnlyList<SqlToken> tokens,
        int index,
        ClauseKind current
    )
    {
        SqlToken token = tokens[index];
        SqlToken? next = index + 1 < tokens.Count ? tokens[index + 1] : null;
        if (index == 0 && token.Is(TSqlTokenType.Select))
            return ClauseKind.Select;
        if (token.Is(TSqlTokenType.Into) && current == ClauseKind.Select)
            return ClauseKind.Into;
        if (token.Is(TSqlTokenType.From))
            return ClauseKind.From;
        if (token.Is(TSqlTokenType.Where))
            return ClauseKind.Where;
        if (token.Is(TSqlTokenType.Having))
            return ClauseKind.Having;
        if (token.Is(TSqlTokenType.Group) && next is not null && next.Is(TSqlTokenType.By))
            return ClauseKind.GroupBy;
        if (token.Is(TSqlTokenType.Order) && next is not null && next.Is(TSqlTokenType.By))
            return ClauseKind.OrderBy;
        if (token.Is(TSqlTokenType.Fetch))
            return ClauseKind.Fetch;
        if (token.Is(TSqlTokenType.Option))
            return ClauseKind.Option;
        if (token.Is(TSqlTokenType.For))
            return ClauseKind.For;
        if (IsOffsetStart(tokens, index))
            return ClauseKind.Offset;
        return null;
    }

    private static bool IsOffsetStart(IReadOnlyList<SqlToken> tokens, int index)
    {
        if (!tokens[index].IsWord("OFFSET") || index + 1 >= tokens.Count)
            return false;
        SqlToken previous = tokens[index - 1];
        SqlToken next = tokens[index + 1];
        return !previous.IsComma
            && !previous.IsDot
            && !previous.Is(TSqlTokenType.Select)
            && (next.Type is TSqlTokenType.Integer or TSqlTokenType.Variable || next.IsOpen);
    }

    private static List<Clause> SplitClauses(
        IReadOnlyList<SqlToken> tokens,
        Func<int, ClauseKind, ClauseKind?> classify
    )
    {
        List<Clause> clauses = [];
        ClauseKind current = classify(0, ClauseKind.Head) ?? ClauseKind.Head;
        int start = 0;
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (index == 0)
                continue;
            ClauseKind? kind = classify(index, current);
            if (kind is null)
                continue;
            clauses.Add(new Clause(current, TokenScan.Slice(tokens, start, index)));
            current = kind.Value;
            start = index;
        }

        if (tokens.Count > 0)
            clauses.Add(new Clause(current, TokenScan.Slice(tokens, start, tokens.Count)));
        return clauses;
    }

    private static List<string> FormatClause(Clause clause, int indent)
    {
        switch (clause.Kind)
        {
            case ClauseKind.Select:
                return FormatSelect(clause.Tokens, indent);
            case ClauseKind.From:
                return FormatFrom(clause.Tokens, indent);
            case ClauseKind.Where:
            case ClauseKind.Having:
                return FormatCondition(clause.Tokens, indent);
            case ClauseKind.Set:
                return FormatSet(clause.Tokens, indent);
            default:
                return [Indent(indent) + ExpressionLayout.Render(clause.Tokens, indent)];
        }
    }

    private static List<string> FormatSelect(IReadOnlyList<SqlToken> tokens, int indent)
    {
        int modifiersEnd = SelectModifiersEnd(tokens);
        string header = ExpressionLayout.Flat(TokenScan.Slice(tokens, 0, modifiersEnd));
        List<string> elements = RenderElements(tokens, modifiersEnd, indent + IndentSize);
        return FormatList(header, elements, indent);
    }

    private static int SelectModifiersEnd(IReadOnlyList<SqlToken> tokens)
    {
        int index = 1;
        while (index < tokens.Count)
        {
            SqlToken token = tokens[index];
            if (token.Type is TSqlTokenType.All or TSqlTokenType.Distinct)
            {
                index++;
            }
            else if (token.Is(TSqlTokenType.Top))
            {
                index++;
                if (index < tokens.Count && tokens[index].IsOpen)
                    index = TokenScan.MatchingClose(tokens, index) + 1;
                else
                    index++;
                if (index < tokens.Count && tokens[index].Is(TSqlTokenType.Percent))
                    index++;
                if (
                    index + 1 < tokens.Count
                    && tokens[index].Is(TSqlTokenType.With)
                    && tokens[index + 1].IsWord("TIES")
                )
                    index += 2;
            }
            else
            {
                break;
            }
        }

        return Math.Min(index, tokens.Count);
    }

    private static List<string> RenderElements(
        IReadOnlyList<SqlToken> tokens,
        int start,
        int indent
    )
    {
        List<string> elements = [];
        foreach (List<SqlToken> element in TokenScan.SplitAtCommas(TokenScan.From(tokens, start)))
            elements.Add(ExpressionLayout.Render(element, indent));
        return elements;
    }

    private static List<string> FormatList(string header, List<string> elements, int indent)
    {
        if (elements.Count == 0)
            return [Indent(indent) + header];
        if (elements.Count == 1 && !elements[0].Contains('\n'))
            return [Indent(indent) + header + " " + elements[0]];

        List<string> lines = [Indent(indent) + header];
        for (int index = 0; index < elements.Count; index++)
        {
            string separator = index < elements.Count - 1 ? "," : string.Empty;
            lines.Add(Indent(indent + IndentSize) + elements[index] + separator);
        }

        return lines;
    }

    private static List<string> FormatSet(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<string> assignments = RenderElements(tokens, 1, indent + IndentSize);
        return FormatList("SET", assignments, indent);
    }

    private static List<string> FormatCondition(IReadOnlyList<SqlToken> tokens, int indent)
    {
        string keyword = ExpressionLayout.Flat([tokens[0]]);
        List<List<SqlToken>> conditions = TokenScan.SplitConditions(TokenScan.From(tokens, 1));
        if (conditions.Count == 0)
            return [Indent(indent) + keyword];

        List<string> lines =
        [
            Indent(indent) + keyword + " " + ExpressionLayout.Render(conditions[0], indent),
        ];
        for (int index = 1; index < conditions.Count; index++)
        {
            lines.Add(
                Indent(indent + IndentSize)
                    + ExpressionLayout.Render(conditions[index], indent + IndentSize)
            );
        }

        return lines;
    }

    private static List<string> FormatFrom(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<SqlToken> sources = TokenScan.From(tokens, 1);
        List<string> lines = [];
        int start = 0;
        bool firstItem = true;
        bool afterComma = false;
        List<int> boundaries = TokenScan.TopLevel(sources);
        foreach (int index in boundaries)
        {
            bool isComma = sources[index].IsComma;
            if (!isComma && !IsJoinStart(sources, index))
                continue;
            if (index > start || !firstItem)
                AddFromItem(
                    lines,
                    TokenScan.Slice(sources, start, index),
                    indent,
                    firstItem,
                    afterComma
                );
            firstItem = false;
            afterComma = isComma;
            start = isComma ? index + 1 : index;
        }

        AddFromItem(
            lines,
            TokenScan.Slice(sources, start, sources.Count),
            indent,
            firstItem,
            afterComma
        );
        return lines;
    }

    private static void AddFromItem(
        List<string> lines,
        List<SqlToken> item,
        int indent,
        bool first,
        bool afterComma
    )
    {
        if (item.Count == 0)
        {
            if (first)
                lines.Add(Indent(indent) + "FROM");
            return;
        }

        if (first)
        {
            lines.Add(Indent(indent) + "FROM " + ExpressionLayout.Render(item, indent));
            return;
        }

        if (afterComma)
        {
            lines[^1] += ",";
            lines.Add(
                Indent(indent + IndentSize) + ExpressionLayout.Render(item, indent + IndentSize)
            );
            return;
        }

        lines.Add(Indent(indent) + RenderJoin(item, indent));
    }

    private static string RenderJoin(IReadOnlyList<SqlToken> tokens, int indent)
    {
        int on = -1;
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (tokens[index].Is(TSqlTokenType.On))
            {
                on = index;
                break;
            }
        }

        if (on < 0)
            return ExpressionLayout.Render(tokens, indent);

        string target = ExpressionLayout.Render(TokenScan.Slice(tokens, 0, on), indent);
        List<List<SqlToken>> conditions = TokenScan.SplitConditions(TokenScan.From(tokens, on + 1));
        if (conditions.Count == 0)
            return target + " ON";

        System.Text.StringBuilder builder = new System.Text.StringBuilder(target);
        builder.Append(" ON ").Append(ExpressionLayout.Render(conditions[0], indent));
        for (int index = 1; index < conditions.Count; index++)
        {
            builder.Append('\n').Append(Indent(indent + IndentSize));
            builder.Append(ExpressionLayout.Render(conditions[index], indent + IndentSize));
        }

        return builder.ToString();
    }

    private static bool IsJoinStart(IReadOnlyList<SqlToken> tokens, int index)
    {
        SqlToken token = tokens[index];
        SqlToken? previous = index > 0 ? tokens[index - 1] : null;
        SqlToken? next = index + 1 < tokens.Count ? tokens[index + 1] : null;
        if (token.Is(TSqlTokenType.Join))
            return previous is null || !IsJoinModifier(previous);
        if (next is null)
            return false;
        if (token.Type is TSqlTokenType.Inner)
            return next.Is(TSqlTokenType.Join);
        if (token.Type is TSqlTokenType.Left or TSqlTokenType.Right or TSqlTokenType.Full)
            return next.Is(TSqlTokenType.Join) || next.Is(TSqlTokenType.Outer);
        if (token.Is(TSqlTokenType.Cross))
            return next.Is(TSqlTokenType.Join) || next.IsWord("APPLY");
        if (token.Is(TSqlTokenType.Outer))
        {
            bool continuesModifier =
                previous is not null
                && previous.Type is TSqlTokenType.Left or TSqlTokenType.Right or TSqlTokenType.Full;
            return !continuesModifier && next.IsWord("APPLY");
        }

        return false;
    }

    private static bool IsJoinModifier(SqlToken token)
    {
        return token.Type
            is TSqlTokenType.Inner
                or TSqlTokenType.Left
                or TSqlTokenType.Right
                or TSqlTokenType.Full
                or TSqlTokenType.Cross
                or TSqlTokenType.Outer;
    }

    private static List<string> FormatWith(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<string> lines = [];
        int index = 1;
        bool first = true;
        while (index < tokens.Count)
        {
            int asIndex = FindTopLevel(tokens, index, TSqlTokenType.As);
            if (asIndex < 0 || asIndex + 1 >= tokens.Count || !tokens[asIndex + 1].IsOpen)
                break;
            int close = TokenScan.MatchingClose(tokens, asIndex + 1);
            string header = ExpressionLayout.Flat(TokenScan.Slice(tokens, index, asIndex));
            lines.Add(Indent(indent) + (first ? "WITH " : string.Empty) + header + " AS (");
            lines.AddRange(
                FormatBody(TokenScan.Slice(tokens, asIndex + 2, close), indent + IndentSize)
            );
            bool another = close + 1 < tokens.Count && tokens[close + 1].IsComma;
            lines.Add(Indent(indent) + ")" + (another ? "," : string.Empty));
            first = false;
            index = another ? close + 2 : close + 1;
            if (!another)
                break;
        }

        lines.AddRange(FormatBody(TokenScan.From(tokens, index), indent));
        return lines;
    }

    private static int FindTopLevel(IReadOnlyList<SqlToken> tokens, int start, TSqlTokenType type)
    {
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (index >= start && tokens[index].Is(type))
                return index;
        }

        return -1;
    }

    private static List<string> FormatInsert(IReadOnlyList<SqlToken> tokens, int indent)
    {
        int tail = -1;
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (
                index > 0
                && tokens[index].Type
                    is TSqlTokenType.Values
                        or TSqlTokenType.Select
                        or TSqlTokenType.With
                        or TSqlTokenType.Default
                        or TSqlTokenType.Exec
                        or TSqlTokenType.Execute
            )
            {
                tail = index;
                break;
            }
        }

        if (tail < 0)
            return [Indent(indent) + ExpressionLayout.Render(tokens, indent)];

        List<string> lines =
        [
            Indent(indent) + ExpressionLayout.Render(TokenScan.Slice(tokens, 0, tail), indent),
        ];
        List<SqlToken> rest = TokenScan.From(tokens, tail);
        if (rest[0].Is(TSqlTokenType.Values))
            lines.AddRange(FormatValues(rest, indent));
        else
            lines.AddRange(FormatBody(rest, indent));
        return lines;
    }

    private static List<string> FormatValues(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<List<SqlToken>> rows = TokenScan.SplitAtCommas(TokenScan.From(tokens, 1));
        if (rows.Count == 1)
        {
            return [Indent(indent) + "VALUES " + ExpressionLayout.Render(rows[0], indent)];
        }

        List<string> lines = [Indent(indent) + "VALUES"];
        for (int index = 0; index < rows.Count; index++)
        {
            string separator = index < rows.Count - 1 ? "," : string.Empty;
            lines.Add(
                Indent(indent + IndentSize)
                    + ExpressionLayout.Render(rows[index], indent + IndentSize)
                    + separator
            );
        }

        return lines;
    }

    private static List<string> FormatUpdate(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<Clause> clauses = SplitClauses(
            tokens,
            (int index, ClauseKind current) =>
                tokens[index].Type switch
                {
                    TSqlTokenType.Set when current == ClauseKind.Head => ClauseKind.Set,
                    TSqlTokenType.From => ClauseKind.From,
                    TSqlTokenType.Where => ClauseKind.Where,
                    TSqlTokenType.Option => ClauseKind.Option,
                    _ => null,
                }
        );
        return FormatClauses(clauses, indent);
    }

    private static List<string> FormatDelete(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<Clause> clauses = SplitClauses(
            tokens,
            (int index, ClauseKind current) =>
                tokens[index].Type switch
                {
                    TSqlTokenType.From when !tokens[index - 1].Is(TSqlTokenType.Delete) =>
                        ClauseKind.From,
                    TSqlTokenType.Where => ClauseKind.Where,
                    TSqlTokenType.Option => ClauseKind.Option,
                    _ => null,
                }
        );
        return FormatClauses(clauses, indent);
    }

    private static List<string> FormatClauses(List<Clause> clauses, int indent)
    {
        List<string> lines = [];
        foreach (Clause clause in clauses)
            lines.AddRange(FormatClause(clause, indent));
        return lines;
    }

    private static List<string> FormatView(IReadOnlyList<SqlToken> tokens, int indent)
    {
        int asIndex = FindTopLevel(tokens, 0, TSqlTokenType.As);
        if (asIndex < 0 || asIndex + 1 >= tokens.Count)
            return [Indent(indent) + ExpressionLayout.Render(tokens, indent)];

        List<string> lines =
        [
            Indent(indent) + ExpressionLayout.Flat(TokenScan.Slice(tokens, 0, asIndex + 1)),
        ];
        lines.AddRange(FormatBody(TokenScan.From(tokens, asIndex + 1), indent));
        return lines;
    }

    private static List<string> FormatCreateTable(IReadOnlyList<SqlToken> tokens, int indent)
    {
        int open = -1;
        foreach (int index in TokenScan.TopLevel(tokens))
        {
            if (tokens[index].IsOpen)
            {
                open = index;
                break;
            }
        }

        if (open < 0)
            return [Indent(indent) + ExpressionLayout.Render(tokens, indent)];

        int close = TokenScan.MatchingClose(tokens, open);
        List<string> lines =
        [
            Indent(indent) + ExpressionLayout.Flat(TokenScan.Slice(tokens, 0, open)) + " (",
        ];
        List<List<SqlToken>> definitions = TokenScan.SplitAtCommas(
            TokenScan.Slice(tokens, open + 1, close)
        );
        for (int index = 0; index < definitions.Count; index++)
        {
            string separator = index < definitions.Count - 1 ? "," : string.Empty;
            lines.Add(
                Indent(indent + IndentSize)
                    + ExpressionLayout.Render(definitions[index], indent + IndentSize)
                    + separator
            );
        }

        string tail = ExpressionLayout.Render(TokenScan.From(tokens, close + 1), indent);
        lines.Add(Indent(indent) + ")" + (tail.Length > 0 ? " " + tail : string.Empty));
        return lines;
    }
}
