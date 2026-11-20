namespace LearnSql.Core.Services.LearningPanel;

public sealed class LearningPanelCatalog
{
    public LearningPanelCatalog(IEnumerable<LearningPanelTab> tabs)
    {
        ArgumentNullException.ThrowIfNull(tabs);

        List<LearningPanelTab> ordered =
        [
            .. tabs.OrderBy((LearningPanelTab tab) => tab.Order)
                .ThenBy((LearningPanelTab tab) => tab.Id, StringComparer.Ordinal),
        ];
        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (LearningPanelTab tab in ordered)
        {
            if (string.IsNullOrWhiteSpace(tab.Id) || string.IsNullOrWhiteSpace(tab.TitleKey))
            {
                throw new ArgumentException("A learning panel tab needs an id and a title key.");
            }

            if (!ids.Add(tab.Id))
            {
                throw new ArgumentException(
                    $"The learning panel tab '{tab.Id}' is registered twice."
                );
            }
        }

        Tabs = ordered;
    }

    public IReadOnlyList<LearningPanelTab> Tabs { get; }

    public int ResolveIndex(string? id)
    {
        if (id is null)
        {
            return 0;
        }

        for (int index = 0; index < Tabs.Count; index++)
        {
            if (Tabs[index].Id == id)
            {
                return index;
            }
        }

        return 0;
    }
}
