namespace LearnSql.Core.Services.Exercises;

public sealed record ExerciseCheckResult(
    ExerciseCheckOutcome Outcome,
    ResultComparison? Comparison = null,
    ExecutionError? Error = null,
    int ResultSetCount = 0
)
{
    public bool CountsAsAttempt =>
        Outcome
            is ExerciseCheckOutcome.Correct
                or ExerciseCheckOutcome.Wrong
                or ExerciseCheckOutcome.QueryFailed
                or ExerciseCheckOutcome.NoResultSet;
}
