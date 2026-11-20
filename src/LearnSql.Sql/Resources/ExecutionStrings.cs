namespace LearnSql.Sql.Resources;

internal static class ExecutionStrings
{
    private static readonly StringTable Table = new StringTable(typeof(ExecutionStrings));

    public static string Cancelled => Table.Get(nameof(Cancelled));

    public static string UnexpectedExecutionError => Table.Get(nameof(UnexpectedExecutionError));

    public static string DatabaseInitializationFailed =>
        Table.Get(nameof(DatabaseInitializationFailed));

    public static string StorageLimitReached => Table.Get(nameof(StorageLimitReached));

    public static string TransactionRolledBack => Table.Get(nameof(TransactionRolledBack));

    public static string ProvisioningCancelled => Table.Get(nameof(ProvisioningCancelled));

    public static string UnexpectedProvisioningError =>
        Table.Get(nameof(UnexpectedProvisioningError));

    public static string ProgressInitializingEngine =>
        Table.Get(nameof(ProgressInitializingEngine));

    public static string ProgressReadingSchema => Table.Get(nameof(ProgressReadingSchema));

    public static string ProgressResettingDatabase => Table.Get(nameof(ProgressResettingDatabase));

    public static string ProgressReady => Table.Get(nameof(ProgressReady));

    public static string TimeLimitExceeded(TimeSpan limit)
    {
        return Table.Format(nameof(TimeLimitExceeded), limit.TotalSeconds);
    }

    public static string ScriptFailed(string? resource, string message)
    {
        return Table.Format(nameof(ScriptFailed), resource, message);
    }

    public static string UnknownScenario(string scenarioId, string available)
    {
        return Table.Format(nameof(UnknownScenario), scenarioId, available);
    }

    public static string InvalidScenario(string scenarioId, string message)
    {
        return Table.Format(nameof(InvalidScenario), scenarioId, message);
    }

    public static string ProgressRunningScript(string resource)
    {
        return Table.Format(nameof(ProgressRunningScript), resource);
    }

    public static string ProtectedInternalObject(string name)
    {
        return Table.Format(nameof(ProtectedInternalObject), name);
    }

    public static string ProtectedInformationSchemaView(string name)
    {
        return Table.Format(nameof(ProtectedInformationSchemaView), name);
    }

    public static string ProtectedVariableTable(string name)
    {
        return Table.Format(nameof(ProtectedVariableTable), name);
    }
}
