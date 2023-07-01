using AnyDaemon;

namespace Microsoft.Extensions.DependencyInjection;

public static class AnyDaemonExtensions
{
    public static IServiceCollection AddAnyDaemonInstaller(this IServiceCollection services)
    {
        services.AddSingleton<IServiceManager, ServiceManager>();

        return services;
    }
}
