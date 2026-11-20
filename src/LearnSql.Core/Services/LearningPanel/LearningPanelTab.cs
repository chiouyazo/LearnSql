using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace LearnSql.Core.Services.LearningPanel;

public sealed record LearningPanelTab(
    string Id,
    string TitleKey,
    Type ResourceType,
    string Icon,
    int Order,
    RenderFragment Content
)
{
    public static LearningPanelTab For<TComponent, TResources>(
        string id,
        string titleKey,
        string icon,
        int order
    )
        where TComponent : IComponent
    {
        return new LearningPanelTab(
            id,
            titleKey,
            typeof(TResources),
            icon,
            order,
            (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<TComponent>(0);
                builder.CloseComponent();
            }
        );
    }
}
