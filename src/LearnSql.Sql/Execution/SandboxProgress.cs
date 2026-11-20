namespace LearnSql.Sql.Execution;

public sealed record SandboxProgress(
    SandboxPhase Phase,
    int Step,
    int TotalSteps,
    string Description
)
{
    public double Fraction =>
        TotalSteps <= 0 ? 1.0 : Math.Clamp((double)Step / TotalSteps, 0.0, 1.0);
}
