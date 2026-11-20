using System.Diagnostics.CodeAnalysis;

namespace LearnSql.Data.Scenarios;

internal sealed class ScenarioTextsValidator
{
    private readonly string _scenarioId;
    private readonly string _resource;

    private ScenarioTextsValidator(string scenarioId, string resource)
    {
        _scenarioId = scenarioId;
        _resource = resource;
    }

    public static ScenarioTexts Validate(
        ScenarioDefinition scenario,
        ScenarioTexts texts,
        string resource
    )
    {
        ScenarioTextsValidator validator = new ScenarioTextsValidator(scenario.Id, resource);
        return validator.ValidateScenario(scenario, texts);
    }

    private ScenarioTexts ValidateScenario(ScenarioDefinition scenario, ScenarioTexts texts)
    {
        NotBlank(texts.Title, "title");
        NotBlank(texts.Description, "description");

        IReadOnlyList<ExerciseTexts> exercises = texts.Exercises ?? [];
        IReadOnlyList<GuidedScenarioTexts> guided = texts.GuidedScenarios ?? [];
        SameCount("exercises", scenario.Exercises.Count, exercises.Count);
        SameCount("guidedScenarios", scenario.GuidedScenarios.Count, guided.Count);

        for (int index = 0; index < exercises.Count; index++)
        {
            ValidateExercise(scenario.Exercises[index], exercises[index], $"exercises[{index}]");
        }

        for (int index = 0; index < guided.Count; index++)
        {
            ValidateGuided(
                scenario.GuidedScenarios[index],
                guided[index],
                $"guidedScenarios[{index}]"
            );
        }

        return texts with
        {
            Exercises = exercises,
            GuidedScenarios = guided,
        };
    }

    private void ValidateExercise(ExerciseDefinition expected, ExerciseTexts actual, string path)
    {
        SameId(expected.Id, actual.Id, path);
        NotBlank(actual.Title, $"{path}.title");
        NotBlank(actual.Description, $"{path}.description");
        IReadOnlyList<string> hints = RequireList(actual.Hints, $"{path}.hints");
        SameCount($"{path}.hints", expected.Hints.Count, hints.Count);
        for (int index = 0; index < hints.Count; index++)
        {
            NotBlank(hints[index], $"{path}.hints[{index}]");
        }
    }

    private void ValidateGuided(
        GuidedScenarioDefinition expected,
        GuidedScenarioTexts actual,
        string path
    )
    {
        SameId(expected.Id, actual.Id, path);
        NotBlank(actual.Title, $"{path}.title");
        NotBlank(actual.Description, $"{path}.description");
        IReadOnlyList<GuidedStepTexts> steps = RequireList(actual.Steps, $"{path}.steps");
        SameCount($"{path}.steps", expected.Steps.Count, steps.Count);
        for (int index = 0; index < steps.Count; index++)
        {
            ValidateStep(expected.Steps[index], steps[index], $"{path}.steps[{index}]");
        }
    }

    private void ValidateStep(GuidedStep expected, GuidedStepTexts actual, string path)
    {
        SameId(expected.Id, actual.Id, path);
        NotBlank(actual.Title, $"{path}.title");
        NotBlank(actual.ResultExplanation, $"{path}.resultExplanation");
        NotBlank(actual.DatabaseEffect, $"{path}.databaseEffect");
        string queryPath = $"{path}.queryExplanation";
        if (actual.QueryExplanation is null)
        {
            Fail(ScenarioCatalogErrorKind.InvalidTranslation, $"{queryPath} is missing.");
        }

        NotBlank(actual.QueryExplanation.Summary, $"{queryPath}.summary");
        IReadOnlyList<SqlPartExplanation> parts = RequireList(
            actual.QueryExplanation.Parts,
            $"{queryPath}.parts"
        );
        SameCount($"{queryPath}.parts", expected.QueryExplanation.Parts.Count, parts.Count);
        for (int index = 0; index < parts.Count; index++)
        {
            string partPath = $"{queryPath}.parts[{index}]";
            string expectedFragment = expected.QueryExplanation.Parts[index].Fragment;
            if (!string.Equals(expectedFragment, parts[index].Fragment, StringComparison.Ordinal))
            {
                Fail(
                    ScenarioCatalogErrorKind.TranslationMismatch,
                    $"{partPath}.fragment must equal '{expectedFragment}' but is '{parts[index].Fragment}'."
                );
            }

            NotBlank(parts[index].Explanation, $"{partPath}.explanation");
        }
    }

    private IReadOnlyList<T> RequireList<T>(IReadOnlyList<T>? list, string path)
    {
        if (list is null)
        {
            Fail(ScenarioCatalogErrorKind.InvalidTranslation, $"{path} is missing.");
        }

        return list;
    }

    private void NotBlank(string? value, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Fail(ScenarioCatalogErrorKind.InvalidTranslation, $"{path} must not be empty.");
        }
    }

    private void SameCount(string path, int expected, int actual)
    {
        if (expected != actual)
        {
            Fail(
                ScenarioCatalogErrorKind.TranslationMismatch,
                $"{path} has {actual} entries but the base scenario has {expected}."
            );
        }
    }

    private void SameId(string expected, string? actual, string path)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            Fail(
                ScenarioCatalogErrorKind.TranslationMismatch,
                $"{path}.id must be '{expected}' but is '{actual}'."
            );
        }
    }

    [DoesNotReturn]
    private void Fail(ScenarioCatalogErrorKind kind, string message)
    {
        throw new ScenarioCatalogException(
            kind,
            $"The translation '{_resource}' of scenario '{_scenarioId}' is not valid: {message}",
            _scenarioId,
            _resource
        );
    }
}
