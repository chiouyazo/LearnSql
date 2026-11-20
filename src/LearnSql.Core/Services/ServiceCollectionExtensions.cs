using LearnSql.Core.Services.Drafts;
using LearnSql.Core.Services.Exercises;
using LearnSql.Core.Services.Guided;
using LearnSql.Core.Services.Interop;
using LearnSql.Core.Services.LearningPanel;
using LearnSql.Core.Services.Localization;
using LearnSql.Core.Services.Navigation;
using LearnSql.Core.Services.Results;
using LearnSql.Core.Services.Scenarios;
using LearnSql.Core.Services.Storage;
using LearnSql.Core.Services.Templates;
using LearnSql.Core.Services.Workspace;
using Microsoft.Extensions.DependencyInjection;

namespace LearnSql.Core.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLearnSqlApp(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddTransient<MudLocalizer, AppMudLocalizer>();
        services.AddSandboxDatabase();

        services.AddSingleton<IBrowserStorage, BrowserStorage>();
        services.AddSingleton<IScriptLoader, ScriptLoader>();
        services.AddSingleton<IMonacoLoader, MonacoLoader>();
        services.AddSingleton<IFileDownloader, FileDownloader>();
        services.AddSingleton<IDocumentService, DocumentService>();
        services.AddSingleton<ISplitPanelObserver, SplitPanelObserver>();

        services.AddSingleton<ThemeState>();
        services.AddSingleton<LocalizationState>();
        services.AddSingleton<AppSession>();
        services.AddSingleton<IAppSession>(
            (IServiceProvider provider) => provider.GetRequiredService<AppSession>()
        );
        services.AddSingleton<IScenarioContext>(
            (IServiceProvider provider) => provider.GetRequiredService<AppSession>()
        );
        services.AddSingleton<StartupPreferences>();

        services.AddSingleton<IQueryRunner, QueryRunner>();
        services.AddSingleton<IDraftStore, DraftStore>();
        services.AddSingleton<IQueryTemplateFactory, QueryTemplateFactory>();
        services.AddSingleton<IResultExporter, ResultExporter>();
        services.AddSingleton<IScenarioTextProvider, ScenarioTextProvider>();

        services.AddSingleton<WorkspaceLayoutPreferences>();
        services.AddSingleton<LearningPanelCatalog>();
        services.AddSingleton<WorkspaceEditorBridge>();
        services.AddSingleton<IWorkspaceEditor>(
            (IServiceProvider provider) => provider.GetRequiredService<WorkspaceEditorBridge>()
        );

        return services.AddNavigationEntries().AddLearningPanelTabs();
    }

    private static IServiceCollection AddLearningPanelTabs(this IServiceCollection services)
    {
        services.AddScenarioLearningTabs();
        services.AddGuidedServices();
        services.AddExerciseLearningTabs();
        return services;
    }

    private static IServiceCollection AddNavigationEntries(this IServiceCollection services)
    {
        services.AddSingleton(
            new NavigationEntry("", "NavHome", Icons.Material.Filled.Home, 0, NavLinkMatch.All)
        );
        services.AddSingleton(
            new NavigationEntry("playground", "NavPlayground", Icons.Material.Filled.Code, 10)
        );
        return services;
    }
}
