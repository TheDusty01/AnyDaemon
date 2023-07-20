using AnyDaemon;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class AnyDaemonExtensions
{
    public static IServiceCollection AddAdServiceManager(this IServiceCollection services)
    {
        // Dependencies
        services.AddAdConfigurationManager();

        services.TryAddSingleton<IServiceManager, ServiceManager>();

        return services;
    }

    public static IServiceCollection AddAdConfigurationManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IServiceConfigurationManager, ServiceConfigurationManager>();

        return services;
    }
}
