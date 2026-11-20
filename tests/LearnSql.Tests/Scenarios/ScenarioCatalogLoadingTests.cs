using System.Text.Json;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public class ScenarioCatalogLoadingTests
{
    private static ScenarioCatalogException LoadFailure(FakeResourceAssembly assembly)
    {
        return Assert.Throws<ScenarioCatalogException>(() => ScenarioCatalog.Load(assembly));
    }

    private static FakeResourceAssembly AssemblyWithRawManifest(string id, string json)
    {
        return new FakeResourceAssembly().Add(ManifestJson.ManifestName(id), json);
    }

    [Fact]
    public void Load_MinimalValidManifest_ProducesTheScenario()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(ManifestJson.Scenario("one"));

        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        ScenarioDefinition scenario = Assert.Single(catalog.Scenarios);
        Assert.Equal("one", scenario.Id);
        Assert.Equal(["Data/one/Schema/01.sql"], scenario.SchemaResources);
        Assert.Empty(scenario.Exercises);
        Assert.Empty(scenario.GuidedScenarios);
    }

    [Fact]
    public void Load_SortsByOrderValueThenByNaturalId()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(
            ManifestJson.Scenario("s10", 1),
            ManifestJson.Scenario("s2", 1),
            ManifestJson.Scenario("late", 5),
            ManifestJson.Scenario("early", 0)
        );

        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        Assert.Equal(
            ["early", "s2", "s10", "late"],
            catalog.Scenarios.Select((ScenarioDefinition s) => s.Id)
        );
    }

    [Fact]
    public void Load_NormalizesExerciseAndGuidedOrder()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(
            ManifestJson.Scenario(
                "one",
                exercises:
                [
                    ManifestJson.Exercise("ex-10", "Select", "Beginner"),
                    ManifestJson.Exercise("ex-2", "Select", "Beginner"),
                    ManifestJson.Exercise("hard", "Select", "Advanced"),
                    ManifestJson.Exercise("join", "Joins", "Beginner"),
                    ManifestJson.Exercise("filter", "Filtering", "Intermediate"),
                ],
                guided:
                [
                    ManifestJson.Guided("g-10", "Joins"),
                    ManifestJson.Guided("g-2", "Joins"),
                    ManifestJson.Guided("g-first", "Select"),
                ]
            )
        );

        ScenarioDefinition scenario = ScenarioCatalog.Load(assembly).GetScenario("one");

        Assert.Equal(
            ["ex-2", "ex-10", "hard", "filter", "join"],
            scenario.Exercises.Select((ExerciseDefinition e) => e.Id)
        );
        Assert.Equal(
            ["g-first", "g-2", "g-10"],
            scenario.GuidedScenarios.Select((GuidedScenarioDefinition g) => g.Id)
        );
    }

    [Fact]
    public void Load_DuplicateScenarioIdInTwoManifests_ThrowsDuplicateScenarioId()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(ManifestJson.Scenario("dup"));
        assembly.Add(
            "LearnSql.Data.Data.other.scenario.json",
            ManifestJson.ToJson(ManifestJson.Scenario("DUP"))
        );

        ScenarioCatalogException exception = LoadFailure(assembly);

        Assert.Equal(ScenarioCatalogErrorKind.DuplicateScenarioId, exception.Kind);
        Assert.Contains("dup", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(exception.Resource);
    }

    [Fact]
    public void Load_ReferencedResourceMissing_ThrowsMissingResourceWithScenarioAndPath()
    {
        FakeResourceAssembly assembly = new FakeResourceAssembly().Add(
            ManifestJson.ManifestName("one"),
            ManifestJson.ToJson(ManifestJson.Scenario("one"))
        );

        ScenarioCatalogException exception = LoadFailure(assembly);

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
        Assert.Equal("one", exception.ScenarioId);
        Assert.Equal("Data/one/Schema/01.sql", exception.Resource);
        Assert.Contains("LearnSql.Data.Data.one.Schema.01.sql", exception.Message);
    }

    [Fact]
    public void Load_MissingViewOrSeedResource_IsDetectedToo()
    {
        FakeResourceAssembly assembly = new FakeResourceAssembly()
            .Add(
                ManifestJson.ManifestName("one"),
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        views: ["Base/Views/V.sql"],
                        seed: ["Data/one/Seed/01.sql"]
                    )
                )
            )
            .Add("LearnSql.Data.Data.one.Schema.01.sql", "SELECT 1;")
            .Add("LearnSql.Data.Base.Views.V.sql", "SELECT 1;");

        ScenarioCatalogException exception = LoadFailure(assembly);

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
        Assert.Equal("Data/one/Seed/01.sql", exception.Resource);
    }

    [Fact]
    public void Load_ManifestStreamCannotBeOpened_ThrowsMissingResource()
    {
        FakeResourceAssembly assembly = new FakeResourceAssembly().AddStream(
            ManifestJson.ManifestName("one"),
            () => null
        );

        ScenarioCatalogException exception = LoadFailure(assembly);

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
        Assert.Equal(ManifestJson.ManifestName("one"), exception.Resource);
    }

    [Fact]
    public void Load_ManifestStreamFailsWhileReading_ThrowsUnreadableResource()
    {
        FakeResourceAssembly assembly = new FakeResourceAssembly().AddStream(
            ManifestJson.ManifestName("one"),
            () => new ThrowingStream()
        );

        ScenarioCatalogException exception = LoadFailure(assembly);

        Assert.Equal(ScenarioCatalogErrorKind.UnreadableResource, exception.Kind);
        Assert.IsType<IOException>(exception.InnerException);
    }

    [Theory]
    [InlineData("not json at all")]
    [InlineData("{")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public void Load_MalformedJson_ThrowsInvalidManifestWithTheJsonException(string json)
    {
        ScenarioCatalogException exception = LoadFailure(AssemblyWithRawManifest("one", json));

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.IsAssignableFrom<JsonException>(exception.InnerException);
        Assert.Equal(ManifestJson.ManifestName("one"), exception.Resource);
    }

    [Fact]
    public void Load_NullJson_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(AssemblyWithRawManifest("one", "null"));

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("empty", exception.Message);
    }

    [Fact]
    public void Load_UnknownProperty_ThrowsInvalidManifest()
    {
        Dictionary<string, object?> scenario = ManifestJson.Scenario("one");
        scenario["unexpected"] = 1;

        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest("one", ManifestJson.ToJson(scenario))
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("sortOrder")]
    [InlineData("title")]
    [InlineData("description")]
    [InlineData("schemaResources")]
    [InlineData("viewResources")]
    [InlineData("seedResources")]
    public void Load_MissingRequiredProperty_ThrowsInvalidManifest(string property)
    {
        Dictionary<string, object?> scenario = ManifestJson.Scenario("one");
        scenario.Remove(property);

        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest("one", ManifestJson.ToJson(scenario))
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Fact]
    public void Load_UnknownEnumName_ThrowsInvalidManifest()
    {
        Dictionary<string, object?> exercise = ManifestJson.Exercise("e1");
        exercise["topic"] = "Astrology";

        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(ManifestJson.Scenario("one", exercises: [exercise]))
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_BlankScenarioId_ThrowsInvalidManifest(string id)
    {
        Dictionary<string, object?> scenario = ManifestJson.Scenario("one");
        scenario["id"] = id;

        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest("one", ManifestJson.ToJson(scenario))
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("Id must not be empty", exception.Message);
    }

    [Fact]
    public void Load_BlankTitle_ThrowsInvalidManifest()
    {
        Dictionary<string, object?> scenario = ManifestJson.Scenario("one");
        scenario["title"] = " ";

        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest("one", ManifestJson.ToJson(scenario))
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("title", exception.Message);
    }

    [Fact]
    public void Load_NoSchemaResources_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(ManifestJson.Scenario("one", schema: []))
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("schema resource", exception.Message);
    }

    [Fact]
    public void Load_DuplicateExerciseId_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        exercises: [ManifestJson.Exercise("e1"), ManifestJson.Exercise("e1")]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("e1", exception.Message);
    }

    [Fact]
    public void Load_ExerciseWithoutHints_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        exercises: [ManifestJson.Exercise("e1", hints: [])]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("hint", exception.Message);
    }

    [Fact]
    public void Load_ExerciseWithBlankSolution_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        exercises: [ManifestJson.Exercise("e1", solution: " ")]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Fact]
    public void Load_DuplicateGuidedId_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        guided: [ManifestJson.Guided("g"), ManifestJson.Guided("g")]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Fact]
    public void Load_GuidedScenarioWithoutSteps_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario("one", guided: [ManifestJson.Guided("g", steps: [])])
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
        Assert.Contains("at least one step", exception.Message);
    }

    [Fact]
    public void Load_DuplicateStepId_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        guided:
                        [
                            ManifestJson.Guided(
                                "g",
                                steps: [ManifestJson.Step("s"), ManifestJson.Step("s")]
                            ),
                        ]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Fact]
    public void Load_StepWithBlankSql_ThrowsInvalidManifest()
    {
        ScenarioCatalogException exception = LoadFailure(
            AssemblyWithRawManifest(
                "one",
                ManifestJson.ToJson(
                    ManifestJson.Scenario(
                        "one",
                        guided:
                        [
                            ManifestJson.Guided("g", steps: [ManifestJson.Step("s", sql: "  ")]),
                        ]
                    )
                )
            )
        );

        Assert.Equal(ScenarioCatalogErrorKind.InvalidManifest, exception.Kind);
    }

    [Fact]
    public void Load_SameExerciseIdInDifferentScenarios_IsAllowed()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(
            ManifestJson.Scenario("one", exercises: [ManifestJson.Exercise("e1")]),
            ManifestJson.Scenario("two", 2, exercises: [ManifestJson.Exercise("e1")])
        );

        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        Assert.Equal(2, catalog.Scenarios.Count);
    }

    [Fact]
    public void GetScripts_ResourceReadFails_ThrowsUnreadableResource()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(ManifestJson.Scenario("one"));
        assembly.AddStream("LearnSql.Data.Data.one.Schema.01.sql", () => new ThrowingStream());
        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            catalog.GetScripts("one")
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnreadableResource, exception.Kind);
        Assert.Equal("one", exception.ScenarioId);
        Assert.Equal("Data/one/Schema/01.sql", exception.Resource);
    }

    [Fact]
    public void GetScripts_ResourceVanishesAfterLoading_ThrowsMissingResource()
    {
        bool available = true;
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(ManifestJson.Scenario("one"));
        assembly.AddStream(
            "LearnSql.Data.Data.one.Schema.01.sql",
            () => available ? new MemoryStream([49]) : null
        );
        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);
        available = false;

        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            catalog.GetScripts("one")
        );

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
    }

    [Fact]
    public void GetScripts_ReadsTheResourceTextAsUtf8()
    {
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(ManifestJson.Scenario("one"));
        assembly.Add("LearnSql.Data.Data.one.Schema.01.sql", "SELECT 'Kräftig';");
        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        ScenarioScripts scripts = catalog.GetScripts("one");

        Assert.Equal("SELECT 'Kräftig';", Assert.Single(scripts.Schema).Content);
    }

    [Fact]
    public void ErrorKinds_AreStableNumericValues()
    {
        Assert.Equal(0, (int)ScenarioCatalogErrorKind.UnknownScenario);
        Assert.Equal(1, (int)ScenarioCatalogErrorKind.InvalidManifest);
        Assert.Equal(2, (int)ScenarioCatalogErrorKind.DuplicateScenarioId);
        Assert.Equal(3, (int)ScenarioCatalogErrorKind.MissingResource);
        Assert.Equal(4, (int)ScenarioCatalogErrorKind.UnreadableResource);
    }

    [Fact]
    public void Exception_ExposesAllDetails()
    {
        InvalidOperationException inner = new InvalidOperationException("inner");

        ScenarioCatalogException exception = new ScenarioCatalogException(
            ScenarioCatalogErrorKind.MissingResource,
            "message",
            "scenario",
            "resource",
            inner
        );

        Assert.Equal(ScenarioCatalogErrorKind.MissingResource, exception.Kind);
        Assert.Equal("message", exception.Message);
        Assert.Equal("scenario", exception.ScenarioId);
        Assert.Equal("resource", exception.Resource);
        Assert.Same(inner, exception.InnerException);
    }
}
