namespace LearnSql.Sql.Execution;

public sealed record SandboxStatus(
    bool IsInitialized,
    string? ScenarioId,
    SandboxActivity Activity,
    bool InTransaction
)
{
    public static SandboxStatus Uninitialized { get; } =
        new SandboxStatus(false, null, SandboxActivity.Idle, false);

    public bool IsBusy => Activity != SandboxActivity.Idle;
}
