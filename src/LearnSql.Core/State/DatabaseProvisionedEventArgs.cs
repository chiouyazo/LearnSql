namespace LearnSql.Core.State;

public sealed class DatabaseProvisionedEventArgs : EventArgs
{
    public DatabaseProvisionedEventArgs(ScenarioDefinition scenario, AppOperation operation)
    {
        Scenario = scenario;
        Operation = operation;
    }

    public ScenarioDefinition Scenario { get; }

    public AppOperation Operation { get; }
}
