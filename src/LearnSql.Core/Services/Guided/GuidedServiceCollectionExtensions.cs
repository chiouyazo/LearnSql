using Microsoft.Extensions.DependencyInjection;

namespace LearnSql.Core.Services.Guided;

public static class GuidedServiceCollectionExtensions
{
    public static IServiceCollection AddGuidedServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<GuidedCoordinator>();
        return services.AddTransient<GuidedPrompts>();
    }
}
