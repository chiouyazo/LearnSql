using BlazorMonaco;
using BlazorMonaco.Editor;
using LearnSql.Core.Services.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MonacoGlobal = BlazorMonaco.Editor.Global;
using MonacoRange = BlazorMonaco.Range;

namespace LearnSql.Core.Components.Editor;

public partial class SqlEditor : IAsyncDisposable
{
    private const string MarkerOwner = "learnsql";
    private const string EditSource = "learnsql";

    private readonly string _id = "sql-editor-" + Guid.NewGuid().ToString("N");

    private StandaloneCodeEditor? _editor;
    private bool _scriptsReady;
    private bool _loadFailed;
    private bool _initialized;
    private bool _hasMarkers;
    private bool _appliedReadOnly;
    private string _current = string.Empty;
    private IReadOnlyList<EditorMarker>? _pendingMarkers;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public EventCallback OnRunRequested { get; set; }

    [Parameter]
    public EventCallback<WordClickEventArgs> OnWordCtrlClick { get; set; }

    [Parameter]
    public EventCallback OnReady { get; set; }

    public bool IsReady => _initialized && _editor is not null;

    [Inject]
    private IMonacoLoader Monaco { get; set; } = default!;

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    [Inject]
    private ThemeState Theme { get; set; } = default!;

    [Inject]
    private IStringLocalizer<EditorStrings> L { get; set; } = default!;

    [Inject]
    private ILogger<SqlEditor> Logger { get; set; } = default!;

    protected override void OnInitialized()
    {
        _current = Value;
        _appliedReadOnly = ReadOnly;
        Theme.Changed += OnThemeChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMonacoAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!IsReady)
        {
            _current = Value;
            return;
        }

        if (Value != _current)
        {
            await SetTextAsync(Value);
        }

        if (ReadOnly != _appliedReadOnly)
        {
            _appliedReadOnly = ReadOnly;
            await _editor!.UpdateOptions(new EditorUpdateOptions { ReadOnly = ReadOnly });
        }
    }

    public async ValueTask DisposeAsync()
    {
        Theme.Changed -= OnThemeChanged;
        if (_editor is null || !_initialized)
        {
            return;
        }

        _initialized = false;
        try
        {
            await _editor.DisposeEditor();
        }
        catch (Exception exception)
            when (exception is JSException or JSDisconnectedException or InvalidOperationException)
        {
            Logger.LogDebug(exception, "Disposing the code editor failed.");
        }
    }

    public async Task SetTextAsync(string text)
    {
        _current = text;
        if (!IsReady)
        {
            return;
        }

        await ClearMarkersAsync();
        await _editor!.SetValue(text);
    }

    public async Task ReplaceTextAsync(string text)
    {
        await SetTextAsync(text);
        await ValueChanged.InvokeAsync(text);
    }

    public async Task InsertAtCursorAsync(string text)
    {
        if (!IsReady)
        {
            return;
        }

        Selection selection = await _editor!.GetSelection();
        IdentifiedSingleEditOperation operation = new IdentifiedSingleEditOperation
        {
            Range = new MonacoRange
            {
                StartLineNumber = selection.StartLineNumber,
                StartColumn = selection.StartColumn,
                EndLineNumber = selection.EndLineNumber,
                EndColumn = selection.EndColumn,
            },
            Text = text,
            ForceMoveMarkers = true,
        };
        await _editor.ExecuteEdits(EditSource, [operation], (List<Selection>?)null);
        await _editor.Focus();
    }

    public async Task<ExecutableText> GetExecutableTextAsync()
    {
        if (!IsReady)
        {
            return new ExecutableText(_current, SourceOrigin.Start, false);
        }

        Selection selection = await _editor!.GetSelection();
        TextModel model = await _editor.GetModel();
        bool hasSelection =
            selection.StartLineNumber != selection.EndLineNumber
            || selection.StartColumn != selection.EndColumn;
        if (!hasSelection)
        {
            string all = await model.GetValue(EndOfLinePreference.TextDefined, true);
            return new ExecutableText(all, SourceOrigin.Start, false);
        }

        string selected = await model.GetValueInRange(selection, EndOfLinePreference.TextDefined);
        return new ExecutableText(
            selected,
            new SourceOrigin(selection.StartLineNumber, selection.StartColumn),
            true
        );
    }

    public async Task<string> GetTextAsync()
    {
        if (!IsReady)
        {
            return _current;
        }

        TextModel model = await _editor!.GetModel();
        return await model.GetValue(EndOfLinePreference.TextDefined, true);
    }

    public async Task FocusAsync()
    {
        if (IsReady)
        {
            await _editor!.Focus();
        }
    }

    public async Task SetMarkersAsync(IReadOnlyList<EditorMarker> markers)
    {
        if (!IsReady)
        {
            _pendingMarkers = markers;
            return;
        }

        TextModel model = await _editor!.GetModel();
        List<MarkerData> data =
        [
            .. markers.Select(
                (EditorMarker marker) =>
                    new MarkerData
                    {
                        Severity = ToMonacoSeverity(marker.Severity),
                        Message = marker.Message,
                        Source = MarkerOwner,
                        CodeAsString = marker.Code,
                        StartLineNumber = marker.Range.StartLine,
                        StartColumn = marker.Range.StartColumn,
                        EndLineNumber = marker.Range.EndLine,
                        EndColumn = marker.Range.EndColumn,
                    }
            ),
        ];
        await MonacoGlobal.SetModelMarkers(Js, model, MarkerOwner, data);
        _hasMarkers = data.Count > 0;
    }

    public async Task ClearMarkersAsync()
    {
        _pendingMarkers = null;
        if (!IsReady || !_hasMarkers)
        {
            return;
        }

        _hasMarkers = false;
        TextModel model = await _editor!.GetModel();
        await MonacoGlobal.SetModelMarkers(Js, model, MarkerOwner, []);
    }

    public async Task RevealRangeAsync(EditorRange range)
    {
        if (!IsReady)
        {
            return;
        }

        await _editor!.RevealRangeInCenterIfOutsideViewport(
            new MonacoRange
            {
                StartLineNumber = range.StartLine,
                StartColumn = range.StartColumn,
                EndLineNumber = range.EndLine,
                EndColumn = range.EndColumn,
            },
            ScrollType.Smooth
        );
        await _editor.SetPosition(
            new BlazorMonaco.Position { LineNumber = range.StartLine, Column = range.StartColumn },
            EditSource
        );
        await _editor.Focus();
    }

    private async Task LoadMonacoAsync()
    {
        try
        {
            await Monaco.EnsureLoadedAsync();
            _loadFailed = false;
            _scriptsReady = true;
        }
        catch (Exception exception) when (exception is JSException or InvalidOperationException)
        {
            Logger.LogError(exception, "The code editor scripts could not be loaded.");
            _loadFailed = true;
        }

        StateHasChanged();
    }

    private StandaloneEditorConstructionOptions CreateOptions(StandaloneCodeEditor editor)
    {
        return new StandaloneEditorConstructionOptions
        {
            Language = "sql",
            Theme = Theme.MonacoTheme,
            Value = _current,
            AutomaticLayout = true,
            ReadOnly = ReadOnly,
            AriaLabel = L["EditorAriaLabel"],
            Minimap = new EditorMinimapOptions { Enabled = false },
            WordWrap = "on",
            FontSize = 14,
            FontFamily =
                "ui-monospace, 'Cascadia Code', Consolas, 'Liberation Mono', 'DejaVu Sans Mono', monospace",
            TabSize = 4,
            ScrollBeyondLastLine = false,
            Padding = new EditorPaddingOptions { Top = 8, Bottom = 8 },
        };
    }

    private async Task OnEditorInitAsync()
    {
        _initialized = true;
        if (await _editor!.GetValue() != _current)
        {
            await _editor.SetValue(_current);
        }

        await _editor.AddCommand(
            (int)KeyMod.CtrlCmd | (int)KeyCode.Enter,
            new CommandHandler((object[] arguments) => _ = RaiseRunRequestedAsync()),
            null
        );
        if (_pendingMarkers is not null)
        {
            IReadOnlyList<EditorMarker> markers = _pendingMarkers;
            _pendingMarkers = null;
            await SetMarkersAsync(markers);
        }

        StateHasChanged();
        await OnReady.InvokeAsync();
    }

    private async Task RaiseRunRequestedAsync()
    {
        try
        {
            await InvokeAsync(() => OnRunRequested.InvokeAsync());
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Running the query from the editor shortcut failed.");
        }
    }

    private async Task OnContentChangedAsync(ModelContentChangedEvent change)
    {
        string text = await _editor!.GetValue();
        if (text == _current)
        {
            return;
        }

        _current = text;
        await ClearMarkersAsync();
        await ValueChanged.InvokeAsync(text);
    }

    private async Task OnMouseDownAsync(EditorMouseEvent mouse)
    {
        bool modifier = mouse.Event.CtrlKey || mouse.Event.MetaKey;
        if (!modifier || mouse.Target.Position is null || !OnWordCtrlClick.HasDelegate)
        {
            return;
        }

        TextModel model = await _editor!.GetModel();
        BlazorMonaco.Position position = mouse.Target.Position;
        WordAtPosition? word = await model.GetWordAtPosition(position);
        if (word is not null)
        {
            await OnWordCtrlClick.InvokeAsync(
                new WordClickEventArgs(
                    word.Word,
                    position.LineNumber,
                    word.StartColumn,
                    word.EndColumn
                )
            );
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        _ = ApplyThemeAsync();
    }

    private async Task ApplyThemeAsync()
    {
        if (!IsReady)
        {
            return;
        }

        try
        {
            await MonacoGlobal.SetTheme(Js, Theme.MonacoTheme);
        }
        catch (JSException exception)
        {
            Logger.LogWarning(exception, "Applying the editor theme failed.");
        }
    }

    private static MarkerSeverity ToMonacoSeverity(EditorMarkerSeverity severity)
    {
        return severity switch
        {
            EditorMarkerSeverity.Error => MarkerSeverity.Error,
            EditorMarkerSeverity.Warning => MarkerSeverity.Warning,
            EditorMarkerSeverity.Info => MarkerSeverity.Info,
            _ => MarkerSeverity.Hint,
        };
    }
}
