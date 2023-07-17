using System.Text.Json;

namespace AnyDaemon;

public interface IServiceConfigurationManager
{
    Task SaveConfigurationAsync(ServiceConfiguration config, CancellationToken ct);
    Task DeleteConfigurationAsync(string serviceName, CancellationToken ct);
    /// <summary>
    /// Throws if configuration cannot be loaded
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<ServiceConfiguration> GetConfigurationAsync(string serviceName, CancellationToken ct);
}

public class ServiceConfigurationManager : IServiceConfigurationManager
{
    public async Task SaveConfigurationAsync(ServiceConfiguration config, CancellationToken ct)
    {
        using var fs = File.Create(AnyDaemonInstallationHelper.GetConfigPath(config.ServiceDescriptor.Name));
        await JsonSerializer.SerializeAsync(fs, config, AnyDaemonConstants.IntendedJsonOptions, ct);
    }

    public Task DeleteConfigurationAsync(string serviceName, CancellationToken ct)
    {
        File.Delete(AnyDaemonInstallationHelper.GetConfigPath(serviceName));

        return Task.CompletedTask;
    }

    public async Task<ServiceConfiguration> GetConfigurationAsync(string serviceName, CancellationToken ct)
    {
        using var fs = File.OpenRead(AnyDaemonInstallationHelper.GetConfigPath(serviceName));
        var config = await JsonSerializer.DeserializeAsync<ServiceConfiguration>(fs, cancellationToken: ct);

        return config!;
    }
}
