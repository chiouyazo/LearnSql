using LearnSql.Core.Services.Navigation;
using LearnSql.Core.Services.Scenarios;
using LearnSql.Data.Scenarios;

namespace LearnSql.Tests.Core;

public sealed class ScenarioNavigationItemTests
{
    private sealed class UpperCaseTexts : IScenarioTextProvider
    {
        public bool CoversCurrentLanguage => true;

        public ScenarioDefinition Localize(ScenarioDefinition scenario)
        {
            return scenario;
        }

        public string GetTitle(ScenarioDefinition scenario)
        {
            return scenario.Title.ToUpperInvariant();
        }

        public string GetDescription(ScenarioDefinition scenario)
        {
            return scenario.Description;
        }
    }

    [Fact]
    public void Create_ProducesOneItemPerScenarioInSortOrder()
    {
        ScenarioDefinition[] scenarios =
        [
            ScenarioRoutesTests.Scenario("b", 2),
            ScenarioRoutesTests.Scenario("a", 1),
            ScenarioRoutesTests.Scenario("c", 2),
        ];

        IReadOnlyList<ScenarioNavigationItem> items = ScenarioNavigationItem.Create(
            scenarios,
            new UpperCaseTexts()
        );

        Assert.Equal(["a", "b", "c"], items.Select((ScenarioNavigationItem i) => i.ScenarioId));
        Assert.Equal(
            ["scenarios/a", "scenarios/b", "scenarios/c"],
            items.Select((ScenarioNavigationItem i) => i.Href)
        );
        Assert.Equal("TITEL A", items[0].Title);
    }

    [Fact]
    public void Create_EveryHrefResolvesBackToItsScenario()
    {
        ScenarioDefinition[] scenarios =
        [
            ScenarioRoutesTests.Scenario("starter", 1),
            ScenarioRoutesTests.Scenario("analytics", 2),
        ];

        foreach (
            ScenarioNavigationItem item in ScenarioNavigationItem.Create(
                scenarios,
                new UpperCaseTexts()
            )
        )
        {
            Assert.True(ScenarioRoutes.TryGetScenarioId(item.Href, out string id));
            Assert.Equal(item.ScenarioId, ScenarioRoutes.Resolve(scenarios, id)?.Id);
        }
    }

    [Fact]
    public void IsActive_ComparesWithoutCaseAndRejectsOthers()
    {
        ScenarioNavigationItem item = new ScenarioNavigationItem(
            "starter",
            "scenarios/starter",
            "T"
        );

        Assert.True(item.IsActive("STARTER"));
        Assert.False(item.IsActive("analytics"));
        Assert.False(item.IsActive(null));
    }

    [Fact]
    public void Create_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ScenarioNavigationItem.Create(null!, new UpperCaseTexts())
        );
        Assert.Throws<ArgumentNullException>(() => ScenarioNavigationItem.Create([], null!));
    }
}
