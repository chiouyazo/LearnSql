using LearnSql.Core.Components.Editor;

namespace LearnSql.Core.Services.Workspace;

public sealed class WorkspaceEditorBridge : IWorkspaceEditor, IWorkspaceEditorText
{
    private SqlEditor? _editor;

    public bool IsAvailable => _editor is not null;

    public void Attach(SqlEditor editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        _editor = editor;
    }

    public void Detach(SqlEditor editor)
    {
        if (ReferenceEquals(_editor, editor))
        {
            _editor = null;
        }
    }

    public Task InsertAtCursorAsync(string text)
    {
        return _editor is null ? Task.CompletedTask : _editor.InsertAtCursorAsync(text);
    }

    public async Task ReplaceTextAsync(string text)
    {
        if (_editor is null)
        {
            return;
        }

        await _editor.ReplaceTextAsync(text);
        await _editor.FocusAsync();
    }

    public async Task<string?> GetTextAsync()
    {
        return _editor is null ? null : await _editor.GetTextAsync();
    }

    public Task FocusAsync()
    {
        return _editor is null ? Task.CompletedTask : _editor.FocusAsync();
    }
}
