namespace LearnSql.Core.State;

public interface IScenarioContext
{
    ScenarioDefinition Scenario { get; }

    IReadOnlyList<ScenarioDefinition> Scenarios { get; }

    bool IsReady { get; }

    event EventHandler<DatabaseProvisionedEventArgs>? DatabaseProvisioned;

    event EventHandler? SchemaChanged;

    Task<DatabaseSchema> GetSchemaAsync(CancellationToken cancellationToken = default);
}
