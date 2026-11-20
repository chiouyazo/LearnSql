namespace LearnSql.Core.Services.Interop;

public interface IScriptLoader
{
    Task LoadAsync(string relativeUrl, CancellationToken cancellationToken = default);

    Task ConfigureAmdLanguageAsync(string language, CancellationToken cancellationToken = default);

    Task LoadAmdModuleAsync(string moduleId, CancellationToken cancellationToken = default);
}
