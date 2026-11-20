using System.Globalization;
using LearnSql.Core.Services.Scenarios;
using LearnSql.Data.Scenarios;
using LearnSql.Tests.Support;

namespace LearnSql.Tests.Core;

public sealed class ScenarioTextProviderTests
{
    private static ScenarioCatalog Catalog(params string[] translatedIds)
    {
        Dictionary<string, object?>[] scenarios =
        [
            ManifestJson.Scenario("one", 1),
            ManifestJson.Scenario("two", 2),
        ];
        FakeResourceAssembly assembly = ManifestJson.AssemblyWith(scenarios);
        foreach (Dictionary<string, object?> scenario in scenarios)
        {
            string id = (string)scenario["id"]!;
            if (translatedIds.Contains(id))
            {
                TranslationJson.WithTranslation(assembly, id, TranslationJson.For(scenario));
            }
        }

        return ScenarioCatalog.Load(assembly);
    }

    private static void UnderCulture(string name, Action action)
    {
        CultureInfo previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(name);
        try
        {
            action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void GetTitle_English_ReturnsTheTranslatedTitle()
    {
        ScenarioCatalog catalog = Catalog("one", "two");
        ScenarioTextProvider provider = new ScenarioTextProvider(catalog);

        UnderCulture(
            "en",
            () => Assert.Equal("English Title one", provider.GetTitle(catalog.GetScenario("one")))
        );
    }

    [Fact]
    public void GetDescription_English_ReturnsTheTranslatedDescription()
    {
        ScenarioCatalog catalog = Catalog("one", "two");
        ScenarioTextProvider provider = new ScenarioTextProvider(catalog);

        UnderCulture(
            "en",
            () =>
                Assert.Equal(
                    "English Description one",
                    provider.GetDescription(catalog.GetScenario("one"))
                )
        );
    }

    [Fact]
    public void GetTitle_German_ReturnsTheBaseTitle()
    {
        ScenarioCatalog catalog = Catalog("one", "two");
        ScenarioTextProvider provider = new ScenarioTextProvider(catalog);

        UnderCulture(
            "de",
            () => Assert.Equal("Title one", provider.GetTitle(catalog.GetScenario("one")))
        );
    }

    [Fact]
    public void GetTitle_EnglishWithoutTranslation_FallsBackToTheBaseTitle()
    {
        ScenarioCatalog catalog = Catalog("one");
        ScenarioTextProvider provider = new ScenarioTextProvider(catalog);

        UnderCulture(
            "en",
            () => Assert.Equal("Title two", provider.GetTitle(catalog.GetScenario("two")))
        );
    }

    [Fact]
    public void Localize_FollowsTheCurrentUiCulture()
    {
        ScenarioCatalog catalog = Catalog("one", "two");
        ScenarioTextProvider provider = new ScenarioTextProvider(catalog);
        ScenarioDefinition scenario = catalog.GetScenario("one");

        UnderCulture("en", () => Assert.NotSame(scenario, provider.Localize(scenario)));
        UnderCulture("de", () => Assert.Same(scenario, provider.Localize(scenario)));
    }

    [Theory]
    [InlineData("de", true)]
    [InlineData("en", true)]
    [InlineData("fr", false)]
    public void CoversCurrentLanguage_AllScenariosTranslated_IsTrueForBaseAndTranslatedLanguages(
        string culture,
        bool expected
    )
    {
        ScenarioTextProvider provider = new ScenarioTextProvider(Catalog("one", "two"));

        UnderCulture(culture, () => Assert.Equal(expected, provider.CoversCurrentLanguage));
    }

    [Fact]
    public void CoversCurrentLanguage_OneScenarioUntranslated_IsFalseForEnglishButTrueForGerman()
    {
        ScenarioTextProvider provider = new ScenarioTextProvider(Catalog("one"));

        UnderCulture("en", () => Assert.False(provider.CoversCurrentLanguage));
        UnderCulture("de", () => Assert.True(provider.CoversCurrentLanguage));
    }
}
