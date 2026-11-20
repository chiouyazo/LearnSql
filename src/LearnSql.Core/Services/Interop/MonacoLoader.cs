namespace LearnSql.Core.Services.Interop;

public sealed class MonacoLoader : IMonacoLoader
{
    private const string EditorModule = "vs/editor/editor.main";

    private static readonly string[] ScriptUrls =
    [
        "_content/BlazorMonaco/jsInterop.js",
        "_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js",
    ];

    private static readonly Dictionary<string, string> PackagedLanguages = new Dictionary<
        string,
        string
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["de"] = "de",
    };

    private readonly IScriptLoader _scripts;
    private Task? _loading;

    public MonacoLoader(IScriptLoader scripts)
    {
        _scripts = scripts;
    }

    public Task EnsureLoadedAsync(CancellationToken cancellationToken = default)
    {
        _loading ??= LoadAllAsync();
        return AwaitLoadingAsync(_loading, cancellationToken);
    }

    private async Task AwaitLoadingAsync(Task loading, CancellationToken cancellationToken)
    {
        try
        {
            await loading.WaitAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _loading = null;
            throw;
        }
    }

    private async Task LoadAllAsync()
    {
        foreach (string url in ScriptUrls)
        {
            await _scripts.LoadAsync(url);
        }

        if (
            PackagedLanguages.TryGetValue(
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                out string? language
            )
        )
        {
            await _scripts.ConfigureAmdLanguageAsync(language);
        }

        await _scripts.LoadAmdModuleAsync(EditorModule);
    }
}
