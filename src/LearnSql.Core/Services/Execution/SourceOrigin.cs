namespace LearnSql.Core.Services.Execution;

public readonly record struct SourceOrigin(int Line, int Column)
{
    public static SourceOrigin Start { get; } = new SourceOrigin(1, 1);

    public EditorRange ToEditorRange(SourceSpan span)
    {
        int startLine = Line + span.Start.Line - 1;
        int startColumn = span.Start.Line == 1 ? Column + span.Start.Column - 1 : span.Start.Column;
        int endLine = Line + span.End.Line - 1;
        int endColumn = span.End.Line == 1 ? Column + span.End.Column - 1 : span.End.Column;
        if (endLine == startLine && endColumn <= startColumn)
        {
            endColumn = startColumn + 1;
        }

        return new EditorRange(startLine, startColumn, endLine, endColumn);
    }
}
