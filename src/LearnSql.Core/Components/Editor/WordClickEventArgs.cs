namespace LearnSql.Core.Components.Editor;

public sealed record WordClickEventArgs(string Word, int Line, int StartColumn, int EndColumn);
