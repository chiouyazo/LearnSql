using LearnSql.Core.Services.LearningPanel;
using Microsoft.AspNetCore.Components;

namespace LearnSql.Core.Components.Learning;

public partial class LearningPanel
{
    private IReadOnlyList<LearningPanelTab> _tabs = [];
    private int _active;

    [Inject]
    private LearningPanelCatalog Catalog { get; set; } = default!;

    [Inject]
    private WorkspaceLayoutPreferences Preferences { get; set; } = default!;

    [Inject]
    private IStringLocalizerFactory Localizers { get; set; } = default!;

    [Inject]
    private IStringLocalizer<ScenarioStrings> L { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await Preferences.EnsureLoadedAsync();
        _tabs = Catalog.Tabs;
        _active = Catalog.ResolveIndex(Preferences.ActiveTabId);
    }

    private string TitleOf(LearningPanelTab tab)
    {
        return Localizers.Create(tab.ResourceType)[tab.TitleKey];
    }

    private async Task OnActiveChangedAsync(int index)
    {
        _active = index;
        if (index >= 0 && index < _tabs.Count)
        {
            await Preferences.SetActiveTabAsync(_tabs[index].Id);
        }
    }
}
