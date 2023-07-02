using CliWrap;
using CliWrap.Buffered;
using FastEnumUtility;

namespace AnyDaemon;

public interface IServiceManager
{
    Task CreateServiceAsync(ServiceConfiguration config, CancellationToken ct = default);
    Task EditServiceAsync(ServiceConfiguration config, CancellationToken ct = default);
    Task DeleteServiceAsync(string serviceName, CancellationToken ct = default);

    // TODO: Maybe move start/stop logic to "AnyDaemon background service" and communicate via
    //  in message pipe or local http server
    Task StartServiceAsync(string serviceName, CancellationToken ct = default);
    Task StopServiceAsync(string serviceName, CancellationToken ct = default);

    Task<ServiceConfiguration> GetServiceInfoAsync(string serviceName, CancellationToken ct = default);
    Task<DaemonStatus> GetServiceStatus(string serviceName, CancellationToken ct = default);
}

public class ServiceManager : IServiceManager
{
    private enum ServiceControlExitCode
    {
        Success = 0,
        AccessDenied = 5,
        AlreadyRunning = 1056,
        ServiceDoesNotExist = 1060,
        AlreadyExists = 1073,
    }

    private readonly IServiceConfigurationManager configurationManager;

    public ServiceManager(IServiceConfigurationManager configurationManager)
    {
        this.configurationManager = configurationManager;
    }

    public async Task CreateServiceAsync(ServiceConfiguration config, CancellationToken ct = default)
    {
        try
        {
            await Cli.Wrap("sc")
                .WithArguments(ConvertDescriptorToArgs("create", config.ServiceDescriptor))
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync(ct);

            // Only execute if the ServiceControl command did not fail
            await configurationManager.SaveConfigurationAsync(config, ct);
        }
        catch
        {
            throw;
        }
    }

    public async Task EditServiceAsync(ServiceConfiguration config, CancellationToken ct = default)
    {
        try
        {
            await Cli.Wrap("sc")
                .WithArguments(ConvertDescriptorToArgs("config", config.ServiceDescriptor))
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync(ct);

            // Only execute if the ServiceControl command did not fail
            await configurationManager.SaveConfigurationAsync(config, ct);
        }
        catch
        {
            throw;
        }
    }

    public async Task DeleteServiceAsync(string serviceName, CancellationToken ct = default)
    {
        try
        {
            await Cli.Wrap("sc")
                .WithArguments(new[]
                {
                    "delete",
                    serviceName
                })
                .WithValidation(CommandResultValidation.ZeroExitCode)
                .ExecuteAsync(ct);

            // Only execute if the ServiceControl command did not fail
            await configurationManager.DeleteConfigurationAsync(serviceName, ct);
        }
        catch
        {
            throw;
        }
    }

    public async Task StartServiceAsync(string serviceName, CancellationToken ct = default)
    {
        await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "start",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(ct);
    }


    public async Task StopServiceAsync(string serviceName, CancellationToken ct = default)
    {
        await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "stop",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(ct);
    }

    public async Task<ServiceConfiguration> GetServiceInfoAsync(string serviceName, CancellationToken ct = default)
    {
        var result = await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "qc",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteBufferedAsync(ct);

        return ParseConfiguration(result.StandardOutput);
    }

    public async Task<DaemonStatus> GetServiceStatus(string serviceName, CancellationToken ct = default)
    {
        var result = await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "query",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteBufferedAsync(ct);

        return ParseStatus(result.StandardOutput);
    }

    private static string[] ConvertDescriptorToArgs(string action, DaemonDescriptor options)
    {
        // $"binPath=dotnet {AnyDaemonServiceRunner}",
        var startCommand = $"{AnyDaemonInstallationHelper.ServiceRunnerExecutableName} {AnyDaemonInstallationHelper.ServiceNameArgument}={options.Name}";

        // Put the arguments into the json config file
        //if (options.Arguments is null)
        //    startCommand += $" {options.Arguments}";

        return new[]
        {
            action,
            options.Name,
            $"binPath={startCommand}",
            $"start={options.StartType.FastToString()}",
            $"displayname={options.FullDisplayName}"
        };
    }

    private static ServiceConfiguration ParseConfiguration(string rawInfo)
    {
        // [SC] QueryServiceConfig ERFOLG
        //
        // SERVICE_NAME: gupdate
        //         TYPE               : 10  WIN32_OWN_PROCESS
        //         START_TYPE         : 2   AUTO_START  (DELAYED)
        //         ERROR_CONTROL      : 1   NORMAL
        //         BINARY_PATH_NAME   : "C:\Program Files (x86)\Google\Update\GoogleUpdate.exe" /svc
        //         LOAD_ORDER_GROUP   :
        //         TAG                : 0
        //         DISPLAY_NAME       : Google Update-Dienst (gupdate)
        //         DEPENDENCIES       : RPCSS
        //         SERVICE_START_NAME : LocalSystem

        return default;
    }

    private static DaemonStatus ParseStatus(string rawStatus)
    {
        // SERVICE_NAME: gupdate
        //         TYPE               : 10  WIN32_OWN_PROCESS
        //         STATE              : 1  STOPPED
        //         WIN32_EXIT_CODE    : 0  (0x0)
        //         SERVICE_EXIT_CODE  : 0  (0x0)
        //         CHECKPOINT         : 0x0
        //         WAIT_HINT          : 0x0

        return default;
    }
}