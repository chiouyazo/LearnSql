using LearnSql.Sql.Execution;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Execution;

public sealed class ScenarioSandboxFixture : IAsyncLifetime
{
    private readonly Dictionary<string, SandboxDatabase> _sandboxes = new Dictionary<
        string,
        SandboxDatabase
    >(StringComparer.Ordinal);

    public async Task<SandboxDatabase> GetAsync(string scenarioId)
    {
        if (_sandboxes.TryGetValue(scenarioId, out SandboxDatabase? existing))
        {
            return existing;
        }

        SandboxDatabase created = await SandboxFactory.CreateProvisionedAsync(scenarioId);
        _sandboxes[scenarioId] = created;
        return created;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (SandboxDatabase sandbox in _sandboxes.Values)
        {
            await sandbox.DisposeAsync();
        }
    }
}
