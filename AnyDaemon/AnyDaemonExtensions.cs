using AnyDaemon;

namespace Microsoft.Extensions.DependencyInjection;

public static class AnyDaemonExtensions
{
    public static IServiceCollection AddAdServiceManager(this IServiceCollection services)
    {
        services.AddSingleton<IServiceManager, ServiceManager>();

        return services;
    }

    public static IServiceCollection AddAdConfigurationManager(this IServiceCollection services)
    {
        services.AddSingleton<IServiceConfigurationManager, ServiceConfigurationManager>();

        return services;
    }
}
