namespace LearnSql.Core.Services.Workspace;

public interface IWorkspaceEditorText
{
    Task<string?> GetTextAsync();
}
