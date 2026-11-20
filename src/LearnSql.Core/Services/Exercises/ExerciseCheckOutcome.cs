namespace LearnSql.Core.Services.Exercises;

public enum ExerciseCheckOutcome
{
    Correct,
    Wrong,
    QueryFailed,
    NoResultSet,
    DatabaseModified,
    Cancelled,
    EmptyInput,
    NotReady,
    Busy,
    ReferenceFailed,
}
