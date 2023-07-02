using System.Diagnostics.CodeAnalysis;

namespace AnyDaemon.Service;

public class ServiceRunner : BackgroundService
{
    private readonly ILogger<ServiceRunner> logger;
    private readonly IConfiguration configuration;
    private readonly IServiceConfigurationManager serviceConfigurationManager;
    [AllowNull] private ServiceConfiguration serviceConfig;

    public ServiceRunner(ILogger<ServiceRunner> logger,
        IConfiguration configuration,
        IServiceConfigurationManager serviceConfigurationManager)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.serviceConfigurationManager = serviceConfigurationManager;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var serviceName = configuration[AnyDaemonInstallationHelper.ServiceNameArgument] ??
            throw new Exception("ServiceName is not set");
        serviceConfig = await serviceConfigurationManager.GetConfigurationAsync(serviceName, cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        // TODO: Start underlying service
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
