namespace LearnSql.Core.State;

public interface IAppSession : IScenarioContext
{
    AppPhase Phase { get; }

    AppOperation? Operation { get; }

    SandboxProgress? Progress { get; }

    AppFailure? Failure { get; }

    event EventHandler? StateChanged;

    Task RestoreScenarioAsync();

    void SelectInitialScenario(string scenarioId);

    Task EnsureStartedAsync();

    Task RetryAsync();

    Task SwitchScenarioAsync(string scenarioId);

    Task ResetDatabaseAsync();

    void NotifySchemaChanged();
}
