using LearnSql.Core.Services.Navigation;
using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Core;

public sealed class ScenarioRoutesTests
{
    [Theory]
    [InlineData("scenarios/starter", "starter")]
    [InlineData("/scenarios/starter", "starter")]
    [InlineData("Scenarios/Analytics/", "Analytics")]
    [InlineData("scenarios/starter?x=1", "starter")]
    [InlineData("scenarios/starter#top", "starter")]
    [InlineData("scenarios/a%20b", "a b")]
    public void TryGetScenarioId_ScenarioRoute_ReturnsTheId(string path, string expected)
    {
        Assert.True(ScenarioRoutes.TryGetScenarioId(path, out string id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("playground")]
    [InlineData("scenarios")]
    [InlineData("scenarios/")]
    [InlineData("scenarios/starter/extra")]
    [InlineData("other/scenarios/starter")]
    public void TryGetScenarioId_OtherPaths_ReturnsFalse(string? path)
    {
        Assert.False(ScenarioRoutes.TryGetScenarioId(path, out string id));
        Assert.Equal(string.Empty, id);
    }

    [Fact]
    public void Path_EscapesTheIdAndRoundTrips()
    {
        string path = ScenarioRoutes.Path("a b/c");

        Assert.Equal("scenarios/a%20b%2Fc", path);
        Assert.True(ScenarioRoutes.TryGetScenarioId(path, out string id));
        Assert.Equal("a b/c", id);
    }

    [Fact]
    public void Path_BlankId_Throws()
    {
        Assert.Throws<ArgumentException>(() => ScenarioRoutes.Path(" "));
    }

    [Fact]
    public void Resolve_IgnoresCaseAndReturnsTheDefinition()
    {
        ScenarioDefinition[] scenarios = [Scenario("starter", 1), Scenario("analytics", 2)];

        Assert.Equal("analytics", ScenarioRoutes.Resolve(scenarios, "ANALYTICS")?.Id);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_UnknownOrBlank_ReturnsNull(string? id)
    {
        Assert.Null(ScenarioRoutes.Resolve([Scenario("starter", 1)], id));
    }

    internal static ScenarioDefinition Scenario(string id, int sortOrder)
    {
        return new ScenarioDefinition
        {
            Id = id,
            SortOrder = sortOrder,
            Title = "Titel " + id,
            Description = "Beschreibung",
            SchemaResources = [],
            ViewResources = [],
            SeedResources = [],
        };
    }
}
