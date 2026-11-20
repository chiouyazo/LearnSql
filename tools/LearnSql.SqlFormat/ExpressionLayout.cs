using System.Text;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.SqlFormat;

internal static class ExpressionLayout
{
    public static string Flat(IReadOnlyList<SqlToken> tokens)
    {
        return Render(tokens, 0, false);
    }

    public static string Render(IReadOnlyList<SqlToken> tokens, int indent)
    {
        return Render(tokens, indent, true);
    }

    private static string Render(IReadOnlyList<SqlToken> tokens, int indent, bool expand)
    {
        StringBuilder builder = new StringBuilder();
        SqlToken? previous = null;
        bool previousUnary = false;
        int index = 0;
        while (index < tokens.Count)
        {
            SqlToken token = tokens[index];
            int last = index;
            string? block = null;
            if (expand && token.Is(TSqlTokenType.Case))
            {
                last = TokenScan.MatchingEnd(tokens, index);
                block = RenderCase(TokenScan.Slice(tokens, index, last + 1), indent);
            }
            else if (expand && token.IsOpen && IsSubqueryStart(tokens, index))
            {
                last = TokenScan.MatchingClose(tokens, index);
                block = RenderSubquery(TokenScan.Slice(tokens, index + 1, last), indent);
            }
            else if (
                expand
                && token.IsOpen
                && previous is not null
                && previous.Is(TSqlTokenType.Over)
            )
            {
                last = TokenScan.MatchingClose(tokens, index);
                block = RenderWindow(TokenScan.Slice(tokens, index + 1, last), indent);
            }

            bool unary = IsUnary(previous, token);
            if (NeedsSpace(previous, previousUnary, token))
                builder.Append(' ');
            builder.Append(block ?? token.Text);
            previous = tokens[last];
            previousUnary = block is null && unary;
            index = last + 1;
        }

        return builder.ToString();
    }

    private static bool IsSubqueryStart(IReadOnlyList<SqlToken> tokens, int openIndex)
    {
        return openIndex + 1 < tokens.Count
            && tokens[openIndex + 1].Type is TSqlTokenType.Select or TSqlTokenType.With;
    }

    private static string RenderSubquery(IReadOnlyList<SqlToken> inner, int indent)
    {
        if (!MustExpand(inner))
        {
            string flat = Render(inner, indent);
            if (!flat.Contains('\n'))
                return "(" + flat + ")";
        }

        List<string> lines = StatementLayout.FormatBody(inner, indent + StatementLayout.IndentSize);
        return "(\n" + string.Join("\n", lines) + "\n" + StatementLayout.Indent(indent) + ")";
    }

    private static bool MustExpand(IReadOnlyList<SqlToken> inner)
    {
        if (inner[0].Is(TSqlTokenType.With))
            return true;
        foreach (int index in TokenScan.TopLevel(inner))
        {
            if (
                inner[index].Type
                is TSqlTokenType.Union
                    or TSqlTokenType.Intersect
                    or TSqlTokenType.Except
            )
                return true;
        }

        return StatementLayout.CountClauses(inner) >= 2;
    }

    private static string RenderCase(IReadOnlyList<SqlToken> tokens, int indent)
    {
        List<SqlToken> inner = TokenScan.Slice(tokens, 1, tokens.Count - 1);
        List<List<SqlToken>> parts = TokenScan.SplitBefore(
            inner,
            (int index) => inner[index].Type is TSqlTokenType.When or TSqlTokenType.Else
        );
        StringBuilder builder = new StringBuilder("CASE");
        int branchStart = 0;
        if (parts.Count > 0 && !parts[0][0].Is(TSqlTokenType.When))
        {
            if (!parts[0][0].Is(TSqlTokenType.Else))
            {
                builder.Append(' ').Append(Render(parts[0], indent));
                branchStart = 1;
            }
        }

        string branchIndent = StatementLayout.Indent(indent + StatementLayout.IndentSize);
        for (int part = branchStart; part < parts.Count; part++)
        {
            builder.Append('\n').Append(branchIndent);
            builder.Append(Render(parts[part], indent + StatementLayout.IndentSize));
        }

        builder.Append('\n').Append(StatementLayout.Indent(indent)).Append("END");
        return builder.ToString();
    }

    private static string RenderWindow(IReadOnlyList<SqlToken> inner, int indent)
    {
        List<List<SqlToken>> parts = TokenScan.SplitBefore(
            inner,
            (int index) => StartsWindowPart(inner, index)
        );
        if (parts.Count < 2)
            return "(" + Render(inner, indent) + ")";

        StringBuilder builder = new StringBuilder("(");
        string partIndent = StatementLayout.Indent(indent + StatementLayout.IndentSize);
        foreach (List<SqlToken> part in parts)
        {
            builder.Append('\n').Append(partIndent);
            builder.Append(Render(part, indent + StatementLayout.IndentSize));
        }

        builder.Append('\n').Append(StatementLayout.Indent(indent)).Append(')');
        return builder.ToString();
    }

    private static bool StartsWindowPart(IReadOnlyList<SqlToken> tokens, int index)
    {
        SqlToken token = tokens[index];
        SqlToken? next = index + 1 < tokens.Count ? tokens[index + 1] : null;
        if (next is null)
            return false;
        if (token.IsWord("PARTITION") || token.Is(TSqlTokenType.Order))
            return next.Is(TSqlTokenType.By);
        if (token.IsWord("ROWS") || token.IsWord("RANGE") || token.IsWord("GROUPS"))
        {
            return next.Type
                    is TSqlTokenType.Between
                        or TSqlTokenType.Integer
                        or TSqlTokenType.Current
                || next.IsWord("UNBOUNDED");
        }

        return false;
    }

    private static bool IsUnary(SqlToken? previous, SqlToken token)
    {
        if (token.Is(TSqlTokenType.Tilde))
            return true;
        if (token.Type is not (TSqlTokenType.Plus or TSqlTokenType.Minus))
            return false;
        if (previous is null || previous.IsOpen || previous.IsComma || previous.IsOperator)
            return true;
        return previous.IsKeyword
            && !previous.Is(TSqlTokenType.End)
            && !previous.Is(TSqlTokenType.Null);
    }

    private static bool NeedsSpace(SqlToken? previous, bool previousUnary, SqlToken current)
    {
        if (previous is null)
            return false;
        if (current.IsClose || current.IsComma || current.IsSemicolon || current.IsDot)
            return false;
        if (previous.IsOpen || previous.IsDot || previousUnary)
            return false;
        if (current.Is(TSqlTokenType.DoubleColon) || previous.Is(TSqlTokenType.DoubleColon))
            return false;
        if (current.IsOpen)
            return !previous.IsFunctionLike || current.SpaceBeforeParen;
        return true;
    }
}
