using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Support;

public static class ScenarioProse
{
    public static IReadOnlyList<KeyValuePair<string, string>> Of(ScenarioDefinition scenario)
    {
        List<KeyValuePair<string, string>> prose = [];
        Add(prose, "title", scenario.Title);
        Add(prose, "description", scenario.Description);
        foreach (ExerciseDefinition exercise in scenario.Exercises)
        {
            Add(prose, $"{exercise.Id}.title", exercise.Title);
            Add(prose, $"{exercise.Id}.description", exercise.Description);
            for (int index = 0; index < exercise.Hints.Count; index++)
                Add(prose, $"{exercise.Id}.hints[{index}]", exercise.Hints[index]);
        }

        foreach (GuidedScenarioDefinition guided in scenario.GuidedScenarios)
        {
            Add(prose, $"{guided.Id}.title", guided.Title);
            Add(prose, $"{guided.Id}.description", guided.Description);
            foreach (GuidedStep step in guided.Steps)
            {
                Add(prose, $"{step.Id}.title", step.Title);
                Add(prose, $"{step.Id}.summary", step.QueryExplanation.Summary);
                for (int index = 0; index < step.QueryExplanation.Parts.Count; index++)
                {
                    Add(
                        prose,
                        $"{step.Id}.parts[{index}]",
                        step.QueryExplanation.Parts[index].Explanation
                    );
                }

                Add(prose, $"{step.Id}.result", step.ResultExplanation);
                Add(prose, $"{step.Id}.effect", step.DatabaseEffect);
            }
        }

        return prose;
    }

    public static IReadOnlyList<string> Fragments(ScenarioDefinition scenario)
    {
        return
        [
            .. scenario.GuidedScenarios.SelectMany(
                (GuidedScenarioDefinition guided) =>
                    guided.Steps.SelectMany(
                        (GuidedStep step) =>
                            step.QueryExplanation.Parts.Select(
                                (SqlPartExplanation part) => part.Fragment
                            )
                    )
            ),
        ];
    }

    private static void Add(List<KeyValuePair<string, string>> prose, string path, string text)
    {
        prose.Add(new KeyValuePair<string, string>(path, text));
    }
}
