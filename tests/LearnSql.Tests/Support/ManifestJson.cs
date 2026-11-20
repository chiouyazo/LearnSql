using System.Text.Json;
using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Support;

public static class ManifestJson
{
    public static Dictionary<string, object?> Scenario(
        string id,
        int sortOrder = 1,
        string[]? schema = null,
        string[]? views = null,
        string[]? seed = null,
        IEnumerable<Dictionary<string, object?>>? exercises = null,
        IEnumerable<Dictionary<string, object?>>? guided = null
    )
    {
        Dictionary<string, object?> scenario = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["sortOrder"] = sortOrder,
            ["title"] = "Title " + id,
            ["description"] = "Description " + id,
            ["schemaResources"] = schema ?? [$"Data/{id}/Schema/01.sql"],
            ["viewResources"] = views ?? [],
            ["seedResources"] = seed ?? [],
        };
        if (exercises is not null)
            scenario["exercises"] = exercises.ToList();
        if (guided is not null)
            scenario["guidedScenarios"] = guided.ToList();
        return scenario;
    }

    public static Dictionary<string, object?> Exercise(
        string id,
        string topic = "Select",
        string difficulty = "Beginner",
        string solution = "SELECT 1",
        string[]? hints = null
    )
    {
        return new Dictionary<string, object?>
        {
            ["id"] = id,
            ["title"] = "Title " + id,
            ["description"] = "Description " + id,
            ["topic"] = topic,
            ["difficulty"] = difficulty,
            ["hints"] = hints ?? ["hint"],
            ["solutionSql"] = solution,
            ["orderMatters"] = false,
            ["expectedRowCount"] = 1,
        };
    }

    public static Dictionary<string, object?> Guided(
        string id,
        string topic = "Select",
        IEnumerable<Dictionary<string, object?>>? steps = null
    )
    {
        return new Dictionary<string, object?>
        {
            ["id"] = id,
            ["title"] = "Title " + id,
            ["description"] = "Description " + id,
            ["topic"] = topic,
            ["requiresFreshDatabase"] = false,
            ["steps"] = (steps ?? [Step("step-1")]).ToList(),
        };
    }

    public static Dictionary<string, object?> Step(string id, string sql = "SELECT 1")
    {
        return new Dictionary<string, object?>
        {
            ["id"] = id,
            ["title"] = "Title " + id,
            ["sql"] = sql,
            ["kind"] = "ReadOnly",
            ["queryExplanation"] = new Dictionary<string, object?>
            {
                ["summary"] = "Summary",
                ["parts"] = new List<Dictionary<string, object?>>
                {
                    new Dictionary<string, object?>
                    {
                        ["fragment"] = "SELECT",
                        ["explanation"] = "Selects.",
                    },
                },
            },
            ["resultExplanation"] = "Result",
            ["databaseEffect"] = "None",
        };
    }

    public static string ToJson(Dictionary<string, object?> scenario)
    {
        return JsonSerializer.Serialize(scenario);
    }

    public static string ManifestName(string id)
    {
        return $"LearnSql.Data.Data.{id}.scenario.json";
    }

    public static string ResourceName(string id, string file)
    {
        return $"LearnSql.Data.Data.{id}.Schema.{file}";
    }

    public static FakeResourceAssembly AssemblyWith(params Dictionary<string, object?>[] scenarios)
    {
        FakeResourceAssembly assembly = new FakeResourceAssembly();
        foreach (Dictionary<string, object?> scenario in scenarios)
        {
            string id = (string)scenario["id"]!;
            assembly.Add(ManifestName(id), ToJson(scenario));
            foreach (string resource in ResourcesOf(scenario))
                assembly.Add(ScenarioCatalog.ToResourceName(resource), "SELECT 1;");
        }

        return assembly;
    }

    private static IEnumerable<string> ResourcesOf(Dictionary<string, object?> scenario)
    {
        return ((string[])scenario["schemaResources"]!)
            .Concat((string[])scenario["viewResources"]!)
            .Concat((string[])scenario["seedResources"]!);
    }
}
