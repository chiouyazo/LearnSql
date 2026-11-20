using LearnSql.Core.Resources;
using LearnSql.Core.Services.LearningPanel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LearnSql.Tests.Core;

public sealed class LearningPanelCatalogTests
{
    private static LearningPanelTab Tab(string id, int order)
    {
        return LearningPanelTab.For<StubComponent, ScenarioStrings>(
            id,
            "Title" + id,
            "icon",
            order
        );
    }

    [Fact]
    public void Tabs_AreOrderedByOrderAndThenById()
    {
        LearningPanelCatalog catalog = new LearningPanelCatalog([
            Tab("b", 20),
            Tab("z", 10),
            Tab("a", 20),
        ]);

        Assert.Equal(["z", "a", "b"], catalog.Tabs.Select((LearningPanelTab t) => t.Id));
    }

    [Fact]
    public void Constructor_DuplicateId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LearningPanelCatalog([Tab("a", 1), Tab("a", 2)])
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_BlankId_Throws(string id)
    {
        Assert.Throws<ArgumentException>(() => new LearningPanelCatalog([Tab(id, 1)]));
    }

    [Fact]
    public void Constructor_BlankTitleKey_Throws()
    {
        LearningPanelTab tab = Tab("a", 1) with { TitleKey = " " };

        Assert.Throws<ArgumentException>(() => new LearningPanelCatalog([tab]));
    }

    [Theory]
    [InlineData("b", 1)]
    [InlineData("a", 0)]
    [InlineData("unknown", 0)]
    [InlineData(null, 0)]
    public void ResolveIndex_ReturnsThePositionAndFallsBackToTheFirstTab(string? id, int expected)
    {
        LearningPanelCatalog catalog = new LearningPanelCatalog([Tab("b", 2), Tab("a", 1)]);

        Assert.Equal(expected, catalog.ResolveIndex(id));
    }

    [Fact]
    public void Catalog_WithoutTabs_IsEmpty()
    {
        LearningPanelCatalog catalog = new LearningPanelCatalog([]);

        Assert.Empty(catalog.Tabs);
        Assert.Equal(0, catalog.ResolveIndex("a"));
    }

    [Fact]
    public void AddLearningPanelTab_RegistersTabsThatTheCatalogOrders()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddLearningPanelTab<StubComponent, ScenarioStrings>(
            "early",
            "TabEarly",
            "icon",
            5
        );
        services.AddScenarioLearningTabs();
        services.AddSingleton<LearningPanelCatalog>();
        using ServiceProvider provider = services.BuildServiceProvider();

        LearningPanelCatalog catalog = provider.GetRequiredService<LearningPanelCatalog>();

        Assert.Equal(["early", "scenario"], catalog.Tabs.Select((LearningPanelTab t) => t.Id));
    }

    private sealed class StubComponent : ComponentBase;
}
