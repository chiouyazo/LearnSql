namespace LearnSql.Core.Services.Drafts;

public interface IDraftStore
{
    Task<string?> LoadAsync(string scenarioId, CancellationToken cancellationToken = default);

    void Save(string scenarioId, string text);

    Task FlushAsync();
}
