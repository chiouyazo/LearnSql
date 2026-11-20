using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Support;

public static class ScenarioData
{
    public static ScenarioCatalog Catalog => SqliteTestDatabase.ScenarioCatalog;

    public static TheoryData<string> ScenarioIds()
    {
        TheoryData<string> data = new TheoryData<string>();
        foreach (ScenarioDefinition scenario in Catalog.Scenarios)
            data.Add(scenario.Id);
        return data;
    }

    public static TheoryData<string, string> ExerciseKeys()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        foreach (ScenarioDefinition scenario in Catalog.Scenarios)
        {
            foreach (ExerciseDefinition exercise in scenario.Exercises)
                data.Add(scenario.Id, exercise.Id);
        }

        return data;
    }

    public static TheoryData<string, string> GuidedKeys()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        foreach (ScenarioDefinition scenario in Catalog.Scenarios)
        {
            foreach (GuidedScenarioDefinition guided in scenario.GuidedScenarios)
                data.Add(scenario.Id, guided.Id);
        }

        return data;
    }

    public static ExerciseDefinition Exercise(string scenarioId, string exerciseId)
    {
        return Catalog
            .GetScenario(scenarioId)
            .Exercises.Single((ExerciseDefinition e) => e.Id == exerciseId);
    }

    public static GuidedScenarioDefinition Guided(string scenarioId, string guidedId)
    {
        return Catalog
            .GetScenario(scenarioId)
            .GuidedScenarios.Single((GuidedScenarioDefinition g) => g.Id == guidedId);
    }
}
