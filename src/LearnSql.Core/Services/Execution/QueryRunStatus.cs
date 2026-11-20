namespace LearnSql.Core.Services.Execution;

public enum QueryRunStatus
{
    Completed,
    EmptyInput,
    AlreadyRunning,
    NotReady,
}
