namespace LearnSql.Sql.Execution;

public enum ExecutionErrorKind
{
    Diagnostics,
    Sql,
    Rejected,
    Timeout,
    Cancelled,
    LimitExceeded,
    Internal,
}
