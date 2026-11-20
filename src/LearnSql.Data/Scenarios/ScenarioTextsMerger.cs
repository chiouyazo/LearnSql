namespace LearnSql.Data.Scenarios;

internal static class ScenarioTextsMerger
{
    public static ScenarioDefinition Apply(ScenarioDefinition scenario, ScenarioTexts texts)
    {
        Dictionary<string, ExerciseTexts> exercises = ById(
            texts.Exercises,
            (ExerciseTexts e) => e.Id
        );
        Dictionary<string, GuidedScenarioTexts> guided = ById(
            texts.GuidedScenarios,
            (GuidedScenarioTexts g) => g.Id
        );
        return scenario with
        {
            Title = texts.Title,
            Description = texts.Description,
            Exercises =
            [
                .. scenario.Exercises.Select(
                    (ExerciseDefinition exercise) =>
                        exercises.TryGetValue(exercise.Id, out ExerciseTexts? match)
                            ? Apply(exercise, match)
                            : exercise
                ),
            ],
            GuidedScenarios =
            [
                .. scenario.GuidedScenarios.Select(
                    (GuidedScenarioDefinition definition) =>
                        guided.TryGetValue(definition.Id, out GuidedScenarioTexts? match)
                            ? Apply(definition, match)
                            : definition
                ),
            ],
        };
    }

    private static Dictionary<string, T> ById<T>(IReadOnlyList<T>? items, Func<T, string> id)
    {
        return (items ?? []).ToDictionary(id, StringComparer.Ordinal);
    }

    private static ExerciseDefinition Apply(ExerciseDefinition exercise, ExerciseTexts texts)
    {
        return exercise with
        {
            Title = texts.Title,
            Description = texts.Description,
            Hints = texts.Hints,
        };
    }

    private static GuidedScenarioDefinition Apply(
        GuidedScenarioDefinition definition,
        GuidedScenarioTexts texts
    )
    {
        Dictionary<string, GuidedStepTexts> steps = ById(texts.Steps, (GuidedStepTexts s) => s.Id);
        return definition with
        {
            Title = texts.Title,
            Description = texts.Description,
            Steps =
            [
                .. definition.Steps.Select(
                    (GuidedStep step) =>
                        steps.TryGetValue(step.Id, out GuidedStepTexts? match)
                            ? Apply(step, match)
                            : step
                ),
            ],
        };
    }

    private static GuidedStep Apply(GuidedStep step, GuidedStepTexts texts)
    {
        return step with
        {
            Title = texts.Title,
            QueryExplanation = texts.QueryExplanation,
            ResultExplanation = texts.ResultExplanation,
            DatabaseEffect = texts.DatabaseEffect,
        };
    }
}
