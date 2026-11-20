namespace LearnSql.Core.Services.Exercises;

public interface IExerciseProgressService
{
    event EventHandler? Changed;

    Task<ExerciseProgressBook> GetBookAsync(ScenarioDefinition scenario);

    Task RecordAttemptAsync(
        ScenarioDefinition scenario,
        string exerciseId,
        bool solved,
        string? sql
    );

    Task<int> RevealNextHintAsync(ScenarioDefinition scenario, ExerciseDefinition exercise);

    Task MarkSolutionViewedAsync(ScenarioDefinition scenario, string exerciseId);

    Task ResetAsync(ScenarioDefinition scenario);
}
