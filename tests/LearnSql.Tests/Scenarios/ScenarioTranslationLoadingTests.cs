using System.Globalization;
using System.Text.Json;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public class ScenarioTranslationLoadingTests
{
    private static readonly CultureInfo English = new CultureInfo("en");
    private static readonly CultureInfo German = new CultureInfo("de");

    private static Dictionary<string, object?> Base()
    {
        return ManifestJson.Scenario(
            "one",
            exercises:
            [
                ManifestJson.Exercise("ex-1", "Select", "Beginner", hints: ["a", "b"]),
                ManifestJson.Exercise("ex-2", "Filtering", "Beginner", hints: ["c"]),
            ],
            guided:
            [
                ManifestJson.Guided(
                    "g-1",
                    steps: [ManifestJson.Step("g-1-01"), ManifestJson.Step("g-1-02")]
                ),
            ]
        );
    }

    private static ScenarioCatalogException Failure(
        Action<Dictionary<string, object?>> mutate,
        Dictionary<string, object?>? scenario = null
    )
    {
        Dictionary<string, object?> source = scenario ?? Base();
        Dictionary<string, object?> texts = TranslationJson.For(source);
        mutate(texts);
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(source);
        TranslationJson.WithTranslation(assembly, "one", texts);
        return Assert.Throws<ScenarioCatalogException>(() => ScenarioCatalog.Load(assembly));
    }

    private static ScenarioCatalog LoadTranslated(Dictionary<string, object?>? scenario = null)
    {
        Dictionary<string, object?> source = scenario ?? Base();
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(source);
        TranslationJson.WithTranslation(assembly, "one", TranslationJson.For(source));
        return ScenarioCatalog.Load(assembly);
    }

    [Fact]
    public void Load_WithoutTranslationFile_HasNoTranslationLanguages()
    {
        ScenarioCatalog catalog = ScenarioCatalog.Load(ManifestJson.AssemblyWith(Base()));

        Assert.Empty(catalog.GetTranslationLanguages("one"));
        Assert.False(catalog.TryGetTranslation("one", "en", out ScenarioTexts? texts));
        Assert.Null(texts);
    }

    [Fact]
    public void Localize_WithoutTranslation_ReturnsTheBaseDefinition()
    {
        ScenarioCatalog catalog = ScenarioCatalog.Load(ManifestJson.AssemblyWith(Base()));
        ScenarioDefinition scenario = catalog.GetScenario("one");

        Assert.Same(scenario, catalog.Localize(scenario, English));
    }

    [Fact]
    public void HasTexts_WithoutTranslation_IsTrueOnlyForTheBaseLanguage()
    {
        ScenarioCatalog catalog = ScenarioCatalog.Load(ManifestJson.AssemblyWith(Base()));

        Assert.True(catalog.HasTexts("one", German));
        Assert.True(catalog.HasTexts("one", new CultureInfo("de-AT")));
        Assert.False(catalog.HasTexts("one", English));
    }

    [Fact]
    public void Load_WithTranslation_ListsTheLanguage()
    {
        ScenarioCatalog catalog = LoadTranslated();

        Assert.Equal(["en"], catalog.GetTranslationLanguages("one"));
        Assert.True(catalog.TryGetTranslation("ONE", "en", out ScenarioTexts? texts));
        Assert.Equal("English Title one", texts!.Title);
        Assert.True(catalog.HasTexts("one", English));
        Assert.False(catalog.HasTexts("one", new CultureInfo("fr")));
    }

    [Fact]
    public void Localize_English_ReplacesTextsAndKeepsEverythingElse()
    {
        ScenarioCatalog catalog = LoadTranslated();
        ScenarioDefinition scenario = catalog.GetScenario("one");

        ScenarioDefinition localized = catalog.Localize(scenario, English);

        Assert.Equal("English Title one", localized.Title);
        Assert.Equal("English Description one", localized.Description);
        Assert.Equal(scenario.SchemaResources, localized.SchemaResources);
        Assert.Equal(scenario.Id, localized.Id);
        Assert.Equal(
            scenario.Exercises.Select((ExerciseDefinition e) => e.Id),
            localized.Exercises.Select((ExerciseDefinition e) => e.Id)
        );
        ExerciseDefinition exercise = localized.Exercises.Single(
            (ExerciseDefinition e) => e.Id == "ex-1"
        );
        Assert.Equal("English Title ex-1", exercise.Title);
        Assert.Equal("English Description ex-1", exercise.Description);
        Assert.Equal(["English a", "English b"], exercise.Hints);
        Assert.Equal("SELECT 1", exercise.SolutionSql);
        Assert.Equal(ExerciseTopic.Select, exercise.Topic);
        GuidedScenarioDefinition guided = Assert.Single(localized.GuidedScenarios);
        Assert.Equal("English Title g-1", guided.Title);
        GuidedStep step = guided.Steps[1];
        Assert.Equal("g-1-02", step.Id);
        Assert.Equal("English Title g-1-02", step.Title);
        Assert.Equal("SELECT 1", step.Sql);
        Assert.Equal("English Summary", step.QueryExplanation.Summary);
        Assert.Equal("SELECT", step.QueryExplanation.Parts[0].Fragment);
        Assert.Equal("English Selects.", step.QueryExplanation.Parts[0].Explanation);
        Assert.Equal("English Result", step.ResultExplanation);
        Assert.Equal("English None", step.DatabaseEffect);
    }

    [Fact]
    public void Localize_DoesNotModifyTheBaseDefinition()
    {
        ScenarioCatalog catalog = LoadTranslated();
        ScenarioDefinition scenario = catalog.GetScenario("one");

        catalog.Localize(scenario, English);

        Assert.Equal("Title one", scenario.Title);
        Assert.Equal("Title ex-1", scenario.Exercises[0].Title);
    }

    [Theory]
    [InlineData("en-GB")]
    [InlineData("en-US")]
    public void Localize_RegionalEnglish_ResolvesToTheEnglishTranslation(string name)
    {
        ScenarioCatalog catalog = LoadTranslated();

        ScenarioDefinition localized = catalog.Localize(
            catalog.GetScenario("one"),
            new CultureInfo(name)
        );

        Assert.Equal("English Title one", localized.Title);
    }

    [Theory]
    [InlineData("de")]
    [InlineData("de-AT")]
    [InlineData("fr")]
    [InlineData("")]
    public void Localize_BaseOrUnknownLanguage_FallsBackToTheBaseTexts(string name)
    {
        ScenarioCatalog catalog = LoadTranslated();
        ScenarioDefinition scenario = catalog.GetScenario("one");

        Assert.Same(scenario, catalog.Localize(scenario, new CultureInfo(name)));
    }

    [Fact]
    public void Localize_ScenarioUnknownToTheCatalog_ReturnsItUnchanged()
    {
        ScenarioCatalog catalog = LoadTranslated();
        ScenarioDefinition foreign = LearnSql.Tests.Core.ScenarioRoutesTests.Scenario("foreign", 1);

        Assert.Same(foreign, catalog.Localize(foreign, English));
    }

    [Fact]
    public void Localize_DefinitionWithOtherExerciseIds_KeepsUnmatchedItemsUntranslated()
    {
        ScenarioCatalog catalog = LoadTranslated();
        ScenarioDefinition scenario = catalog.GetScenario("one");
        ScenarioDefinition altered = scenario with
        {
            Exercises =
            [
                scenario.Exercises[0] with
                {
                    Id = "unknown",
                },
                .. scenario.Exercises.Skip(1),
            ],
        };

        ScenarioDefinition localized = catalog.Localize(altered, English);

        Assert.Equal("Title ex-1", localized.Exercises[0].Title);
    }

    [Fact]
    public void Load_TranslationWithoutLearningContentOnBaseWithout_IsAccepted()
    {
        Dictionary<string, object?> source = ManifestJson.Scenario("one");

        ScenarioCatalog catalog = LoadTranslated(source);

        Assert.Equal(
            "English Title one",
            catalog.Localize(catalog.GetScenario("one"), English).Title
        );
    }

    [Fact]
    public void Load_TranslationOmitsLearningContentWhileBaseHasSome_Fails()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
            {
                texts.Remove("exercises");
                texts.Remove("guidedScenarios");
            }
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_TranslationFollowsFileOrderNotTheNormalizedOrder()
    {
        Dictionary<string, object?> source = ManifestJson.Scenario(
            "one",
            exercises:
            [
                ManifestJson.Exercise("zz", "Select", "Advanced"),
                ManifestJson.Exercise("aa", "Select", "Beginner"),
            ]
        );

        ScenarioCatalog catalog = LoadTranslated(source);
        ScenarioDefinition localized = catalog.Localize(catalog.GetScenario("one"), English);

        Assert.Equal(["aa", "zz"], localized.Exercises.Select((ExerciseDefinition e) => e.Id));
        Assert.Equal("English Title aa", localized.Exercises[0].Title);
    }

    [Fact]
    public void Load_ErrorCarriesScenarioIdAndResource()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => texts["title"] = " "
        );

        Assert.Equal("one", exception.ScenarioId);
        Assert.Equal(TranslationJson.TranslationName("one", "en"), exception.Resource);
    }

    [Fact]
    public void Load_MalformedJson_ThrowsInvalidTranslation()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.Add(TranslationJson.TranslationName("one", "en"), "{ not json");

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
    }

    [Fact]
    public void Load_NullDocument_ThrowsInvalidTranslation()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.Add(TranslationJson.TranslationName("one", "en"), "null");

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_UnknownField_ThrowsInvalidTranslation()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Exercises(texts)[0]["solutionSql"] = "SELECT 1"
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_MissingRequiredField_ThrowsInvalidTranslation()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => texts.Remove("description")
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    public static TheoryData<string> BlankTextEdits()
    {
        return new TheoryData<string>
        {
            "title",
            "description",
            "exerciseTitle",
            "exerciseDescription",
            "hint",
            "guidedTitle",
            "guidedDescription",
            "stepTitle",
            "summary",
            "explanation",
            "result",
            "effect",
        };
    }

    [Theory]
    [MemberData(nameof(BlankTextEdits))]
    public void Load_BlankText_ThrowsInvalidTranslation(string field)
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
            {
                Dictionary<string, object?> exercise = TranslationJson.Exercises(texts)[0];
                Dictionary<string, object?> guided = TranslationJson.Guided(texts)[0];
                Dictionary<string, object?> step = TranslationJson.Steps(guided)[0];
                switch (field)
                {
                    case "title":
                        texts["title"] = "";
                        break;
                    case "description":
                        texts["description"] = "  ";
                        break;
                    case "exerciseTitle":
                        exercise["title"] = "";
                        break;
                    case "exerciseDescription":
                        exercise["description"] = "";
                        break;
                    case "hint":
                        ((List<string>)exercise["hints"]!)[1] = " ";
                        break;
                    case "guidedTitle":
                        guided["title"] = "";
                        break;
                    case "guidedDescription":
                        guided["description"] = "";
                        break;
                    case "stepTitle":
                        step["title"] = "";
                        break;
                    case "summary":
                        TranslationJson.Query(step)["summary"] = "";
                        break;
                    case "explanation":
                        TranslationJson.Parts(step)[0]["explanation"] = "";
                        break;
                    case "result":
                        step["resultExplanation"] = "";
                        break;
                    default:
                        step["databaseEffect"] = "";
                        break;
                }
            }
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_NullHintList_ThrowsInvalidTranslation()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Exercises(texts)[0]["hints"] = null
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_NullQueryExplanation_ThrowsInvalidTranslation()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Steps(TranslationJson.Guided(texts)[0])[0]["queryExplanation"] =
                    null
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_TranslationInTheBaseLanguage_ThrowsInvalidTranslation()
    {
        Dictionary<string, object?> source = Base();
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(source);
        TranslationJson.WithTranslation(assembly, "one", TranslationJson.For(source), "de");

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidTranslation, exception.Kind);
    }

    [Fact]
    public void Load_ExerciseCountDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => TranslationJson.Exercises(texts).RemoveAt(1)
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_ExerciseIdsDiffer_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Exercises(texts)[0]["id"] = "other"
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_ExerciseOrderDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => TranslationJson.Exercises(texts).Reverse()
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_HintCountDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                ((List<string>)TranslationJson.Exercises(texts)[0]["hints"]!).Add("extra")
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_GuidedCountDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => TranslationJson.Guided(texts).Clear()
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_GuidedIdDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) => TranslationJson.Guided(texts)[0]["id"] = "other"
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_StepCountDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Steps(TranslationJson.Guided(texts)[0]).RemoveAt(1)
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_StepIdDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Steps(TranslationJson.Guided(texts)[0])[1]["id"] = "other"
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_PartCountDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson
                    .Parts(TranslationJson.Steps(TranslationJson.Guided(texts)[0])[0])
                    .Clear()
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_FragmentDiffers_ThrowsTranslationMismatch()
    {
        ScenarioCatalogException exception = Failure(
            (Dictionary<string, object?> texts) =>
                TranslationJson.Parts(TranslationJson.Steps(TranslationJson.Guided(texts)[0])[0])[
                    0
                ]["fragment"] = "select"
        );

        Assert.Equal(ScenarioCatalogErrorKind.TranslationMismatch, exception.Kind);
    }

    [Fact]
    public void Load_TranslationWithoutBaseManifest_ThrowsOrphanTranslation()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.Add(TranslationJson.TranslationName("ghost", "en"), "{}");

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.OrphanTranslation, exception.Kind);
        Assert.Equal(TranslationJson.TranslationName("ghost", "en"), exception.Resource);
    }

    [Fact]
    public void Load_TranslationStreamFailsWhileReading_ThrowsUnreadableResource()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.AddStream(
            TranslationJson.TranslationName("one", "en"),
            () => new ThrowingStream()
        );

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnreadableResource, exception.Kind);
    }

    [Fact]
    public void Load_TranslationStreamCannotBeOpened_ThrowsMissingResource()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.AddStream(TranslationJson.TranslationName("one", "en"), () => null);

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            ScenarioCatalog.Load(assembly)
        );

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
    }

    [Theory]
    [InlineData("LearnSql.Data.Data.one.scenario.json.bak")]
    [InlineData("LearnSql.Data.Data.one.scenario.english.json")]
    [InlineData("LearnSql.Data.Data.one.scenario.E1.json")]
    [InlineData("LearnSql.Data.Data.one.notes.en.json")]
    public void Load_UnrelatedResourceNames_AreNotTreatedAsTranslations(string name)
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(Base());
        assembly.Add(name, "{ not json");

        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        Assert.Empty(catalog.GetTranslationLanguages("one"));
    }
}
