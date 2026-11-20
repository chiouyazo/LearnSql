using System.Globalization;
using System.Text.RegularExpressions;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public partial class ScenarioTranslationDataTests
{
    private static readonly ScenarioCatalog Catalog = ScenarioData.Catalog;

    private static readonly string[] TranslatedLanguages = ["en"];

    private static readonly string[] GermanWordsAllowedInEnglish =
    [
        "Handmühle",
        "Zubehör",
        "Köhler",
        "Österreich",
    ];

    private static readonly string[] EnglishWordsAllowedInGerman = [];

    private static readonly HashSet<string> GermanStopWords = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "und",
        "der",
        "die",
        "das",
        "nicht",
        "wird",
        "werden",
        "zeile",
        "zeilen",
        "ist",
        "sind",
        "mit",
        "eine",
        "einen",
        "oder",
        "wenn",
        "auch",
        "von",
        "dass",
        "alle",
        "jede",
        "jeder",
    };

    private static readonly HashSet<string> EnglishStopWords = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "the",
        "which",
        "are",
        "of",
        "returns",
        "row",
        "rows",
        "this",
        "that",
        "each",
        "have",
        "will",
        "only",
        "you",
        "your",
    };

    [GeneratedRegex("[\u2013\u2014]|\\s--\\s")]
    private static partial Regex DashPattern();

    [GeneratedRegex(@"[äöüÄÖÜß]")]
    private static partial Regex UmlautPattern();

    [GeneratedRegex(@"\b\p{L}+\b")]
    private static partial Regex WordPattern();

    public static TheoryData<string, string> Translations()
    {
        TheoryData<string, string> data = new TheoryData<string, string>();
        foreach (ScenarioDefinition scenario in Catalog.Scenarios)
        {
            foreach (string language in TranslatedLanguages)
                data.Add(scenario.Id, language);
        }

        return data;
    }

    public static TheoryData<string> ScenarioIds() => ScenarioData.ScenarioIds();

    private static ScenarioTexts Translation(string scenarioId, string language)
    {
        Assert.True(
            Catalog.TryGetTranslation(scenarioId, language, out ScenarioTexts? texts),
            $"Scenario '{scenarioId}' has no '{language}' translation (scenario.{language}.json)."
        );
        return texts!;
    }

    private static ScenarioDefinition Localized(string scenarioId, string language)
    {
        return Catalog.Localize(Catalog.GetScenario(scenarioId), new CultureInfo(language));
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_Exists(string scenarioId, string language)
    {
        Assert.NotNull(Translation(scenarioId, language));
        Assert.True(Catalog.HasTexts(scenarioId, new CultureInfo(language)));
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_StructureMatchesTheBaseScenario(string scenarioId, string language)
    {
        ScenarioDefinition scenario = Catalog.GetScenario(scenarioId);
        ScenarioTexts texts = Translation(scenarioId, language);

        Assert.Equal(
            scenario.Exercises.Select((ExerciseDefinition e) => e.Id).Order(),
            texts.Exercises!.Select((ExerciseTexts e) => e.Id).Order()
        );
        Assert.Equal(
            scenario.GuidedScenarios.Select((GuidedScenarioDefinition g) => g.Id).Order(),
            texts.GuidedScenarios!.Select((GuidedScenarioTexts g) => g.Id).Order()
        );
        foreach (ExerciseDefinition exercise in scenario.Exercises)
        {
            ExerciseTexts match = texts.Exercises!.Single((ExerciseTexts e) => e.Id == exercise.Id);
            Assert.Equal(exercise.Hints.Count, match.Hints.Count);
        }

        foreach (GuidedScenarioDefinition guided in scenario.GuidedScenarios)
        {
            GuidedScenarioTexts match = texts.GuidedScenarios!.Single(
                (GuidedScenarioTexts g) => g.Id == guided.Id
            );
            Assert.Equal(
                guided.Steps.Select((GuidedStep s) => s.Id),
                match.Steps.Select((GuidedStepTexts s) => s.Id)
            );
            for (int index = 0; index < guided.Steps.Count; index++)
            {
                Assert.Equal(
                    guided
                        .Steps[index]
                        .QueryExplanation.Parts.Select((SqlPartExplanation p) => p.Fragment),
                    match
                        .Steps[index]
                        .QueryExplanation.Parts.Select((SqlPartExplanation p) => p.Fragment)
                );
            }
        }
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_LeavesNonTextFieldsUntouched(string scenarioId, string language)
    {
        ScenarioDefinition scenario = Catalog.GetScenario(scenarioId);
        ScenarioDefinition localized = Localized(scenarioId, language);

        Assert.Equal(scenario.SchemaResources, localized.SchemaResources);
        Assert.Equal(scenario.SeedResources, localized.SeedResources);
        Assert.Equal(
            scenario.Exercises.Select(
                (ExerciseDefinition e) =>
                    (e.Id, e.Topic, e.Difficulty, e.SolutionSql, e.OrderMatters, e.ExpectedRowCount)
            ),
            localized.Exercises.Select(
                (ExerciseDefinition e) =>
                    (e.Id, e.Topic, e.Difficulty, e.SolutionSql, e.OrderMatters, e.ExpectedRowCount)
            )
        );
        Assert.Equal(
            scenario.GuidedScenarios.SelectMany(
                (GuidedScenarioDefinition g) =>
                    g.Steps.Select(
                        (GuidedStep s) =>
                            (s.Id, s.Sql, s.Kind, s.ExpectedResultRows, s.ExpectedRowsAffected)
                    )
            ),
            localized.GuidedScenarios.SelectMany(
                (GuidedScenarioDefinition g) =>
                    g.Steps.Select(
                        (GuidedStep s) =>
                            (s.Id, s.Sql, s.Kind, s.ExpectedResultRows, s.ExpectedRowsAffected)
                    )
            )
        );
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_AllTextsAreNotEmpty(string scenarioId, string language)
    {
        IReadOnlyList<KeyValuePair<string, string>> prose = ScenarioProse.Of(
            Localized(scenarioId, language)
        );

        Assert.NotEmpty(prose);
        Assert.All(
            prose,
            (KeyValuePair<string, string> entry) =>
                Assert.False(string.IsNullOrWhiteSpace(entry.Value), entry.Key)
        );
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_FragmentsAreLiterallyTheBaseFragments(
        string scenarioId,
        string language
    )
    {
        Assert.Equal(
            ScenarioProse.Fragments(Catalog.GetScenario(scenarioId)),
            ScenarioProse.Fragments(Localized(scenarioId, language))
        );
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_TextsContainNoDashes(string scenarioId, string language)
    {
        Assert.All(
            ScenarioProse.Of(Localized(scenarioId, language)),
            (KeyValuePair<string, string> entry) =>
                Assert.False(DashPattern().IsMatch(entry.Value), entry.Key + ": " + entry.Value)
        );
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void Translation_EnglishFile_ContainsNoGermanWordsOrUmlauts(
        string scenarioId,
        string language
    )
    {
        if (language != "en")
            return;

        Assert.All(
            ScenarioProse.Of(Localized(scenarioId, language)),
            (KeyValuePair<string, string> entry) =>
            {
                string text = StripAllowed(entry.Value, GermanWordsAllowedInEnglish);
                Assert.False(UmlautPattern().IsMatch(text), $"{entry.Key}: {entry.Value}");
                Assert.Empty(
                    StopWordsIn(text, GermanStopWords)
                        .Select((string word) => $"{entry.Key}: '{word}' in {entry.Value}")
                );
            }
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void BaseTexts_ContainNoEnglishStopWords(string scenarioId)
    {
        Assert.All(
            ScenarioProse.Of(Catalog.GetScenario(scenarioId)),
            (KeyValuePair<string, string> entry) =>
            {
                string text = StripAllowed(entry.Value, EnglishWordsAllowedInGerman);
                Assert.Empty(
                    StopWordsIn(text, EnglishStopWords)
                        .Select((string word) => $"{entry.Key}: '{word}' in {entry.Value}")
                );
            }
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void BaseTexts_ContainNoDashes(string scenarioId)
    {
        Assert.All(
            ScenarioProse.Of(Catalog.GetScenario(scenarioId)),
            (KeyValuePair<string, string> entry) =>
                Assert.False(DashPattern().IsMatch(entry.Value), entry.Key + ": " + entry.Value)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Catalog_OffersNoTranslationForUnexpectedLanguages(string scenarioId)
    {
        Assert.Subset(
            new HashSet<string>(TranslatedLanguages),
            new HashSet<string>(Catalog.GetTranslationLanguages(scenarioId))
        );
    }

    private static bool IsUpperCase(string word)
    {
        return word.Length > 1 && word == word.ToUpperInvariant();
    }

    private static string StripAllowed(string text, string[] allowed)
    {
        string result = text;
        foreach (string word in allowed)
            result = result.Replace(word, " ", StringComparison.Ordinal);
        return result;
    }

    private static IEnumerable<string> StopWordsIn(string text, HashSet<string> stopWords)
    {
        return WordPattern()
            .Matches(text)
            .Select((Match match) => match.Value)
            .Where((string word) => !IsUpperCase(word) && stopWords.Contains(word))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
