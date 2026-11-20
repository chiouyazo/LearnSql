using LearnSql.Core.Services.Exercises;

namespace LearnSql.Core.Services.Storage;

public static class StorageKeys
{
    private const string Prefix = "learnsql:v1:";

    public static StorageKey<string> Theme { get; } =
        new StorageKey<string>(Prefix + "theme", StorageJsonContext.Default.String);

    public static StorageKey<string> Language { get; } =
        new StorageKey<string>(Prefix + "language", StorageJsonContext.Default.String);

    public static StorageKey<string> Scenario { get; } =
        new StorageKey<string>(Prefix + "scenario", StorageJsonContext.Default.String);

    public static StorageKey<bool> LearningPanelCollapsed { get; } =
        new StorageKey<bool>(
            Prefix + "workspace:learning-collapsed",
            StorageJsonContext.Default.Boolean
        );

    public static StorageKey<int> LearningPanelWidth { get; } =
        new StorageKey<int>(Prefix + "workspace:learning-width", StorageJsonContext.Default.Int32);

    public static StorageKey<string> LearningPanelTab { get; } =
        new StorageKey<string>(
            Prefix + "workspace:learning-tab",
            StorageJsonContext.Default.String
        );

    public static StorageKey<bool> SchemaPaneCollapsed { get; } =
        new StorageKey<bool>(
            Prefix + "workspace:schema-collapsed",
            StorageJsonContext.Default.Boolean
        );

    public static StorageKey<string> Draft(string scenarioId)
    {
        return new StorageKey<string>(
            Prefix + "draft:" + scenarioId,
            StorageJsonContext.Default.String
        );
    }

    public static StorageKey<ExerciseProgressDocument> Exercises(string scenarioId)
    {
        return new StorageKey<ExerciseProgressDocument>(
            Prefix + "exercises:" + scenarioId,
            StorageJsonContext.Default.ExerciseProgressDocument
        );
    }
}
