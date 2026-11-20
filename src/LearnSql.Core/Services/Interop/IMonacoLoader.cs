namespace LearnSql.Core.Services.Interop;

public interface IMonacoLoader
{
    Task EnsureLoadedAsync(CancellationToken cancellationToken = default);
}
