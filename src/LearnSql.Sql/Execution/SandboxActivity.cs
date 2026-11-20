namespace LearnSql.Sql.Execution;

public enum SandboxActivity
{
    Idle,
    Initializing,
    Provisioning,
    Executing,
    ReadingSchema,
}
