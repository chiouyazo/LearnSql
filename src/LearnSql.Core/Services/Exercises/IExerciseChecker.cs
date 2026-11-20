namespace LearnSql.Core.Services.Exercises;

public interface IExerciseChecker
{
    bool DatabaseModified { get; }

    Task<ExerciseCheckResult> CheckAsync(
        ExerciseDefinition exercise,
        string sql,
        CancellationToken cancellationToken = default
    );
}
