using System.Reflection;
using LearnSql.Data;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Scenarios;

public class ScenarioCatalogTests
{
    private static readonly ScenarioCatalog Catalog = SqliteTestDatabase.ScenarioCatalog;

    public static TheoryData<string> ScenarioIds()
    {
        TheoryData<string> data = new TheoryData<string>();
        foreach (ScenarioDefinition scenario in Catalog.Scenarios)
            data.Add(scenario.Id);
        return data;
    }

    [Fact]
    public void LoadDefault_ContainsTheThreeShippedScenarios()
    {
        Assert.Equal(
            ["starter", "analytics", "playground"],
            Catalog.Scenarios.Select((ScenarioDefinition s) => s.Id)
        );
    }

    [Fact]
    public void Scenarios_AreSortedBySortOrder()
    {
        Assert.Equal(
            Catalog.Scenarios.Select((ScenarioDefinition s) => s.SortOrder).Order(),
            Catalog.Scenarios.Select((ScenarioDefinition s) => s.SortOrder)
        );
    }

    [Fact]
    public void Scenarios_HaveUniqueSortOrdersAndIds()
    {
        Assert.Equal(
            Catalog.Scenarios.Count,
            Catalog.Scenarios.Select((ScenarioDefinition s) => s.SortOrder).Distinct().Count()
        );
        Assert.Equal(
            Catalog.Scenarios.Count,
            Catalog
                .Scenarios.Select((ScenarioDefinition s) => s.Id)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
        );
    }

    [Fact]
    public void Load_DefaultAssemblyTwice_YieldsEqualCatalogs()
    {
        ScenarioCatalog other = ScenarioCatalog.Load(typeof(DataHolder).Assembly);

        Assert.Equal(
            Catalog.Scenarios.Select((ScenarioDefinition s) => s.Id),
            other.Scenarios.Select((ScenarioDefinition s) => s.Id)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_HasTitleDescriptionAndAtLeastOneSchemaResource(string id)
    {
        ScenarioDefinition scenario = Catalog.GetScenario(id);

        Assert.False(string.IsNullOrWhiteSpace(scenario.Title));
        Assert.False(string.IsNullOrWhiteSpace(scenario.Description));
        Assert.NotEmpty(scenario.SchemaResources);
        Assert.NotEmpty(scenario.SeedResources);
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_ResourcesAreListedInNaturalOrder(string id)
    {
        ScenarioDefinition scenario = Catalog.GetScenario(id);

        Assert.Equal(
            scenario.SeedResources.Order(NaturalStringComparer.Instance),
            scenario.SeedResources
        );
        Assert.Equal(
            scenario.SchemaResources.Order(NaturalStringComparer.Instance),
            scenario.SchemaResources
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_ExercisesAreOrderedByTopicDifficultyAndNaturalId(string id)
    {
        IReadOnlyList<ExerciseDefinition> exercises = Catalog.GetScenario(id).Exercises;

        Assert.Equal(
            exercises
                .OrderBy((ExerciseDefinition e) => e.Topic)
                .ThenBy((ExerciseDefinition e) => e.Difficulty)
                .ThenBy((ExerciseDefinition e) => e.Id, NaturalStringComparer.Instance)
                .Select((ExerciseDefinition e) => e.Id),
            exercises.Select((ExerciseDefinition e) => e.Id)
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void Scenario_GuidedScenariosAreOrderedByTopicAndNaturalId(string id)
    {
        IReadOnlyList<GuidedScenarioDefinition> guided = Catalog.GetScenario(id).GuidedScenarios;

        Assert.Equal(
            guided
                .OrderBy((GuidedScenarioDefinition g) => g.Topic)
                .ThenBy((GuidedScenarioDefinition g) => g.Id, NaturalStringComparer.Instance)
                .Select((GuidedScenarioDefinition g) => g.Id),
            guided.Select((GuidedScenarioDefinition g) => g.Id)
        );
    }

    [Fact]
    public void GetScenario_UnknownId_ThrowsUnknownScenarioWithTheKnownIds()
    {
        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            Catalog.GetScenario("does-not-exist")
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnknownScenario, exception.Kind);
        Assert.Equal("does-not-exist", exception.ScenarioId);
        Assert.Contains("does-not-exist", exception.Message);
        Assert.Contains("starter", exception.Message);
        Assert.Contains("playground", exception.Message);
        Assert.Contains("analytics", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetScenario_BlankId_ThrowsUnknownScenario(string id)
    {
        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            Catalog.GetScenario(id)
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnknownScenario, exception.Kind);
    }

    [Fact]
    public void GetScripts_UnknownId_ThrowsUnknownScenario()
    {
        ScenarioCatalogException exception = Assert.Throws<ScenarioCatalogException>(() =>
            Catalog.GetScripts("nope")
        );

        Assert.Equal(ScenarioCatalogErrorKind.UnknownScenario, exception.Kind);
    }

    [Theory]
    [InlineData("starter")]
    [InlineData("STARTER")]
    [InlineData("Starter")]
    public void GetScenario_IsCaseInsensitive(string id)
    {
        Assert.Equal("starter", Catalog.GetScenario(id).Id);
    }

    [Fact]
    public void TryGetScenario_KnownId_ReturnsTheScenario()
    {
        bool found = Catalog.TryGetScenario("playground", out ScenarioDefinition? scenario);

        Assert.True(found);
        Assert.Equal("playground", scenario!.Id);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    [InlineData(" ")]
    public void TryGetScenario_UnknownOrBlankId_ReturnsFalse(string id)
    {
        bool found = Catalog.TryGetScenario(id, out ScenarioDefinition? scenario);

        Assert.False(found);
        Assert.Null(scenario);
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void GetScripts_ReturnsOneScriptPerResourceWithContentInManifestOrder(string id)
    {
        ScenarioDefinition scenario = Catalog.GetScenario(id);

        ScenarioScripts scripts = Catalog.GetScripts(id);

        Assert.Equal(scenario.Id, scripts.ScenarioId);
        Assert.Equal(scenario.SchemaResources, scripts.Schema.Select((SqlScript s) => s.Resource));
        Assert.Equal(scenario.ViewResources, scripts.Views.Select((SqlScript s) => s.Resource));
        Assert.Equal(scenario.SeedResources, scripts.Seed.Select((SqlScript s) => s.Resource));
        Assert.All(
            scripts.InExecutionOrder,
            (SqlScript script) => Assert.False(string.IsNullOrWhiteSpace(script.Content))
        );
    }

    [Theory]
    [MemberData(nameof(ScenarioIds))]
    public void GetScripts_ExecutionOrder_IsSchemaThenViewsThenSeed(string id)
    {
        ScenarioScripts scripts = Catalog.GetScripts(id);

        Assert.Equal(
            scripts
                .Schema.Concat(scripts.Views)
                .Concat(scripts.Seed)
                .Select((SqlScript s) => s.Resource),
            scripts.InExecutionOrder.Select((SqlScript s) => s.Resource)
        );
    }

    [Theory]
    [InlineData(
        "Data/starter/Seed/01.ShopData.sql",
        "LearnSql.Data.Data.starter.Seed.01.ShopData.sql"
    )]
    [InlineData(
        "Data\\starter\\Seed\\01.ShopData.sql",
        "LearnSql.Data.Data.starter.Seed.01.ShopData.sql"
    )]
    [InlineData("Base/Views/TablesView.sql", "LearnSql.Data.Base.Views.TablesView.sql")]
    public void ToResourceName_ConvertsLogicalPathsToEmbeddedResourceNames(
        string logicalPath,
        string expected
    )
    {
        Assert.Equal(expected, ScenarioCatalog.ToResourceName(logicalPath));
    }

    [Fact]
    public void EveryEmbeddedSqlResource_IsReferencedByAScenario()
    {
        HashSet<string> referenced =
        [
            .. Catalog.Scenarios.SelectMany(
                (ScenarioDefinition s) =>
                    s
                        .SchemaResources.Concat(s.ViewResources)
                        .Concat(s.SeedResources)
                        .Select(ScenarioCatalog.ToResourceName)
            ),
        ];
        string[] embedded =
        [
            .. typeof(DataHolder)
                .Assembly.GetManifestResourceNames()
                .Where((string name) => name.EndsWith(".sql", StringComparison.Ordinal)),
        ];

        Assert.NotEmpty(embedded);
        Assert.All(embedded, (string name) => Assert.Contains(name, referenced));
    }

    [Fact]
    public void Load_NullAssembly_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ScenarioCatalog.Load(null!));
    }

    [Fact]
    public void Load_AssemblyWithoutManifests_YieldsAnEmptyCatalog()
    {
        Assembly assembly = new FakeResourceAssembly();

        ScenarioCatalog catalog = ScenarioCatalog.Load(assembly);

        Assert.Empty(catalog.Scenarios);
    }
}
