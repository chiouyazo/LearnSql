namespace LearnSql.Core.Services.Execution;

public sealed record QueryRun(
    string Sql,
    SourceOrigin Origin,
    ExecutionResult Result,
    ExecutionOptions Options
);
