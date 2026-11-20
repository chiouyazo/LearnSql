namespace LearnSql.Sql.Execution;

public enum SandboxPhase
{
    PreparingEngine,
    ResettingDatabase,
    CreatingSchema,
    CreatingViews,
    SeedingData,
    ReadingSchema,
    Ready,
}
