using CliWrap;
using System.Diagnostics.CodeAnalysis;

namespace AnyDaemon.Service;

public class ServiceRunner : BackgroundService
{
    private readonly ILogger<ServiceRunner> logger;
    private readonly IHostApplicationLifetime lifetime;
    private readonly IConfiguration configuration;
    private readonly IServiceConfigurationManager serviceConfigurationManager;
    [AllowNull] private ServiceConfiguration serviceConfig;

    public ServiceRunner(
        ILogger<ServiceRunner> logger,
        IHostApplicationLifetime lifetime,
        IConfiguration configuration,
        IServiceConfigurationManager serviceConfigurationManager)
    {
        this.logger = logger;
        this.lifetime = lifetime;
        this.configuration = configuration;
        this.serviceConfigurationManager = serviceConfigurationManager;
    }

    public override async Task StartAsync(CancellationToken ct)
    {
        var serviceName = configuration[AnyDaemonInstallationHelper.ServiceNameArgument] ??
            throw new Exception("ServiceName is not set");
        serviceConfig = await serviceConfigurationManager.GetConfigurationAsync(serviceName, ct);

        // Start the background service
        await base.StartAsync(ct);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Starting underlying process");

        // Start underlying service
        var builder = Cli.Wrap(serviceConfig.ServiceDescriptor.ExecutablePath)
            .WithWorkingDirectory(serviceConfig.ServiceDescriptor.WorkingDir);

        if (serviceConfig.ServiceDescriptor.Arguments is not null)
            builder = builder.WithArguments(serviceConfig.ServiceDescriptor.Arguments);

        var result = await builder
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync(ct);
        
        // Print meta data
        logger.LogInformation(
            "Underlying process stopped with {exitCode} exit code. Running for {duration}",
            result.ExitCode,
            result.RunTime
        );

        lifetime.StopApplication();
    }
}
