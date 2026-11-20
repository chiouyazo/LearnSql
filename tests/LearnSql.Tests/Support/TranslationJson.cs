using System.Text.Json;

namespace LearnSql.Tests.Support;

public static class TranslationJson
{
    public static Dictionary<string, object?> For(Dictionary<string, object?> scenario)
    {
        Dictionary<string, object?> texts = new Dictionary<string, object?>
        {
            ["title"] = "English " + scenario["title"],
            ["description"] = "English " + scenario["description"],
        };
        if (scenario.TryGetValue("exercises", out object? exercises))
        {
            texts["exercises"] = ((List<Dictionary<string, object?>>)exercises!)
                .Select(ExerciseTexts)
                .ToList();
        }

        if (scenario.TryGetValue("guidedScenarios", out object? guided))
        {
            texts["guidedScenarios"] = ((List<Dictionary<string, object?>>)guided!)
                .Select(GuidedTexts)
                .ToList();
        }

        return texts;
    }

    public static List<Dictionary<string, object?>> Exercises(Dictionary<string, object?> texts)
    {
        return (List<Dictionary<string, object?>>)texts["exercises"]!;
    }

    public static List<Dictionary<string, object?>> Guided(Dictionary<string, object?> texts)
    {
        return (List<Dictionary<string, object?>>)texts["guidedScenarios"]!;
    }

    public static List<Dictionary<string, object?>> Steps(Dictionary<string, object?> guided)
    {
        return (List<Dictionary<string, object?>>)guided["steps"]!;
    }

    public static Dictionary<string, object?> Query(Dictionary<string, object?> step)
    {
        return (Dictionary<string, object?>)step["queryExplanation"]!;
    }

    public static List<Dictionary<string, object?>> Parts(Dictionary<string, object?> step)
    {
        return (List<Dictionary<string, object?>>)Query(step)["parts"]!;
    }

    public static string TranslationName(string id, string language)
    {
        return $"LearnSql.Data.Data.{id}.scenario.{language}.json";
    }

    public static FakeResourceAssembly WithTranslation(
        FakeResourceAssembly assembly,
        string id,
        Dictionary<string, object?> texts,
        string language = "en"
    )
    {
        return assembly.Add(TranslationName(id, language), JsonSerializer.Serialize(texts));
    }

    private static Dictionary<string, object?> ExerciseTexts(Dictionary<string, object?> exercise)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = exercise["id"],
            ["title"] = "English " + exercise["title"],
            ["description"] = "English " + exercise["description"],
            ["hints"] = ((string[])exercise["hints"]!)
                .Select((string h) => "English " + h)
                .ToList(),
        };
    }

    private static Dictionary<string, object?> GuidedTexts(Dictionary<string, object?> guided)
    {
        return new Dictionary<string, object?>
        {
            ["id"] = guided["id"],
            ["title"] = "English " + guided["title"],
            ["description"] = "English " + guided["description"],
            ["steps"] = ((List<Dictionary<string, object?>>)guided["steps"]!)
                .Select(StepTexts)
                .ToList(),
        };
    }

    private static Dictionary<string, object?> StepTexts(Dictionary<string, object?> step)
    {
        Dictionary<string, object?> query = (Dictionary<string, object?>)step["queryExplanation"]!;
        return new Dictionary<string, object?>
        {
            ["id"] = step["id"],
            ["title"] = "English " + step["title"],
            ["queryExplanation"] = new Dictionary<string, object?>
            {
                ["summary"] = "English " + query["summary"],
                ["parts"] = ((List<Dictionary<string, object?>>)query["parts"]!)
                    .Select(
                        (Dictionary<string, object?> part) =>
                            new Dictionary<string, object?>
                            {
                                ["fragment"] = part["fragment"],
                                ["explanation"] = "English " + part["explanation"],
                            }
                    )
                    .ToList(),
            },
            ["resultExplanation"] = "English " + step["resultExplanation"],
            ["databaseEffect"] = "English " + step["databaseEffect"],
        };
    }
}
