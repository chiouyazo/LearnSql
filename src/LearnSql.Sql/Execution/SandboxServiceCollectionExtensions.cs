using LearnSql.Data.Scenarios;
using LearnSql.Sql.Models;
using LearnSql.Sql.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LearnSql.Sql.Execution;

public static class SandboxServiceCollectionExtensions
{
    public static IServiceCollection AddSandboxDatabase(
        this IServiceCollection services,
        SandboxOptions? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? new SandboxOptions());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(_ => ScenarioCatalog.LoadDefault());
        services.TryAddSingleton<IErrorTranslator>(_ =>
            ErrorTranslatorFactory.Create(Language.MsSql)
        );
        services.TryAddSingleton<ICooperativeYield>(provider => new TimeSliceYield(
            provider.GetRequiredService<SandboxOptions>().YieldInterval,
            provider.GetRequiredService<TimeProvider>()
        ));
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IStatementPolicy, SystemObjectPolicy>()
        );
        services.TryAddSingleton<SandboxDatabase>();
        services.TryAddSingleton<ISandboxDatabase>(provider =>
            provider.GetRequiredService<SandboxDatabase>()
        );
        return services;
    }
}
