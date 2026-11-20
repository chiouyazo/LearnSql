using LearnSql.Sql.Dialect.Diagnostics;
using LearnSql.Sql.Dialect.Syntax;
using LearnSql.Sql.Dialect.Typing;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace LearnSql.Sql.Dialect.Rewriting;

public sealed class TranslationContext
{
    private readonly FragmentLocator _locator;
    private readonly EditCollector _edits;
    private readonly List<DialectDiagnostic> _diagnostics = [];

    public TranslationContext(
        FragmentTree tree,
        string batchText,
        FragmentLocator locator,
        ISchemaCatalog schema
    )
    {
        Tree = tree;
        _locator = locator;
        _edits = new EditCollector(batchText);
        Variables = new VariableScope();
        Types = new ExpressionTypeInferrer(tree, schema, Variables);
    }

    public FragmentTree Tree { get; }

    public VariableScope Variables { get; }

    public ExpressionTypeInferrer Types { get; }

    public bool UsesVariables { get; set; }

    public IReadOnlyList<DialectDiagnostic> Diagnostics => _diagnostics;

    public string Original(TSqlFragment fragment)
    {
        return _edits.Source.Substring(fragment.StartOffset, fragment.FragmentLength);
    }

    public string Render(TSqlFragment fragment)
    {
        return _edits.Render(fragment.StartOffset, fragment.FragmentLength);
    }

    public FragmentNode NodeOf(TSqlFragment fragment)
    {
        return Tree.GetNode(fragment);
    }

    public void Replace(TSqlFragment fragment, string text)
    {
        _edits.Replace(fragment.StartOffset, fragment.FragmentLength, text);
    }

    public void Replace(TSqlFragment fragment, Func<string> text)
    {
        _edits.Replace(fragment.StartOffset, fragment.FragmentLength, text);
    }

    public void ReplaceRange(int start, int end, string text)
    {
        _edits.Replace(start, end - start, text);
    }

    public void ReplaceToken(TSqlParserToken token, string text)
    {
        _edits.Replace(token.Offset, token.Text.Length, text);
    }

    public void DeleteRange(int start, int end)
    {
        _edits.Delete(start, end - start);
    }

    public void Delete(TSqlFragment fragment)
    {
        _edits.Delete(fragment.StartOffset, fragment.FragmentLength);
    }

    public void InsertBefore(TSqlFragment fragment, string text)
    {
        _edits.InsertBefore(fragment.StartOffset, text);
    }

    public void InsertBefore(TSqlFragment fragment, Func<string> text)
    {
        _edits.InsertBefore(fragment.StartOffset, text);
    }

    public void InsertAfter(TSqlFragment fragment, string text)
    {
        _edits.InsertAfter(fragment.StartOffset + fragment.FragmentLength, text);
    }

    public void InsertAfter(TSqlFragment fragment, Func<string> text)
    {
        _edits.InsertAfter(fragment.StartOffset + fragment.FragmentLength, text);
    }

    public void Report(
        TSqlFragment at,
        DiagnosticKind kind,
        DiagnosticSeverity severity,
        string code,
        string message,
        string? hint = null,
        int? msSqlErrorNumber = null
    )
    {
        _diagnostics.Add(
            new DialectDiagnostic(
                kind,
                severity,
                code,
                message,
                hint,
                _locator.GetSpan(at),
                msSqlErrorNumber
            )
        );
    }

    public TSqlParserToken TokenAt(int index)
    {
        return Tree.Tokens[index];
    }

    public int NextSignificantTokenIndex(int afterIndex)
    {
        int index = afterIndex + 1;
        while (index < Tree.Tokens.Count && IsTrivia(Tree.Tokens[index]))
            index++;
        return index;
    }

    public int PreviousSignificantTokenIndex(int beforeIndex)
    {
        int index = beforeIndex - 1;
        while (index >= 0 && IsTrivia(Tree.Tokens[index]))
            index--;
        return index;
    }

    public int EndOfToken(int index)
    {
        TSqlParserToken token = Tree.Tokens[index];
        return token.Offset + token.Text.Length;
    }

    private static bool IsTrivia(TSqlParserToken token)
    {
        return token.TokenType
            is TSqlTokenType.WhiteSpace
                or TSqlTokenType.SingleLineComment
                or TSqlTokenType.MultilineComment;
    }
}
