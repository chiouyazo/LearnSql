using LearnSql.Core.Components.Learning;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LearnSql.Core.Services.LearningPanel;

public static class LearningPanelServiceCollectionExtensions
{
    public static IServiceCollection AddLearningPanelTab<TComponent, TResources>(
        this IServiceCollection services,
        string id,
        string titleKey,
        string icon,
        int order
    )
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton(
            LearningPanelTab.For<TComponent, TResources>(id, titleKey, icon, order)
        );
    }

    public static IServiceCollection AddScenarioLearningTabs(this IServiceCollection services)
    {
        return services.AddLearningPanelTab<ScenarioInfoTab, ScenarioStrings>(
            "scenario",
            "TabScenario",
            Icons.Material.Filled.Info,
            10
        );
    }
}
