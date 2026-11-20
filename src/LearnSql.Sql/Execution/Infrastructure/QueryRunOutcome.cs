namespace LearnSql.Sql.Execution;

internal sealed record QueryRunOutcome(ExecutionResult Result, bool SchemaMayHaveChanged);
