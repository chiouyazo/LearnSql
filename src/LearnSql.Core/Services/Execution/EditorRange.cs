namespace LearnSql.Core.Services.Execution;

public readonly record struct EditorRange(
    int StartLine,
    int StartColumn,
    int EndLine,
    int EndColumn
);
