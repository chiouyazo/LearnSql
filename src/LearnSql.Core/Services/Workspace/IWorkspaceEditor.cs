namespace LearnSql.Core.Services.Workspace;

public interface IWorkspaceEditor
{
    bool IsAvailable { get; }

    Task InsertAtCursorAsync(string text);

    Task ReplaceTextAsync(string text);

    Task FocusAsync();
}
