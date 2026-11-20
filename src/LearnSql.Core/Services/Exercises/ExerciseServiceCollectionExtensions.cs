using LearnSql.Core.Components.Learning.Exercises;
using LearnSql.Core.Services.LearningPanel;
using LearnSql.Core.Services.Workspace;
using Microsoft.Extensions.DependencyInjection;

namespace LearnSql.Core.Services.Exercises;

public static class ExerciseServiceCollectionExtensions
{
    public static IServiceCollection AddExerciseLearningTabs(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IWorkspaceEditorText>(
            (IServiceProvider provider) => provider.GetRequiredService<WorkspaceEditorBridge>()
        );
        services.AddSingleton<IExerciseProgressService, ExerciseProgressService>();
        services.AddSingleton<IExerciseChecker, ExerciseChecker>();
        return services.AddLearningPanelTab<ExercisesTab, ExerciseStrings>(
            "exercises",
            "TabExercises",
            Icons.Material.Filled.Assignment,
            30
        );
    }
}
