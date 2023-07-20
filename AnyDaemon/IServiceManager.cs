using CliWrap;
using CliWrap.Buffered;
using FastEnumUtility;
using System.Collections;

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

public class ServiceControlException : Exception
{
    public int ExitCode { get; }

    public ServiceControlException(string output, int exitCode)
        : base($"sc.exe failed with exit code {exitCode}.\n{output}")
    {
        ExitCode = exitCode;
    }
}

public class AnyDaemonNotInstalledException : Exception
{
    public AnyDaemonNotInstalledException()
        : base("AnyDaemon is not installed")
    {
    }
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
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        await ExecuteServiceControl("create", ConvertDescriptorToArgs(config.ServiceDescriptor), ct);

        // Only execute if the ServiceControl command did not fail
        await configurationManager.SaveConfigurationAsync(config, ct);
    }

    public async Task EditServiceAsync(ServiceConfiguration config, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        await ExecuteServiceControl("config", ConvertDescriptorToArgs(config.ServiceDescriptor), ct);

        // Only execute if the ServiceControl command did not fail
        await configurationManager.SaveConfigurationAsync(config, ct);
    }

    public async Task DeleteServiceAsync(string serviceName, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        await ExecuteServiceControl("delete", new[] { serviceName }, ct);

        // Only execute if the ServiceControl command did not fail
        await configurationManager.DeleteConfigurationAsync(serviceName, ct);
    }

    public async Task StartServiceAsync(string serviceName, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        await ExecuteServiceControl("start", new[] { serviceName }, ct);
    }

    public async Task StopServiceAsync(string serviceName, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        await ExecuteServiceControl("stop", new[] { serviceName }, ct);
    }

    public async Task<ServiceConfiguration> GetServiceInfoAsync(string serviceName, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        var result = await ExecuteServiceControl("qc", new[] { serviceName }, ct);

        return ParseConfiguration(result.StandardOutput);
    }

    public async Task<DaemonStatus> GetServiceStatus(string serviceName, CancellationToken ct = default)
    {
        AnyDaemonInstallationHelper.ThrowIfNotInstalled();

        var result = await ExecuteServiceControl("query", new[] { serviceName }, ct);

        return ParseStatus(result.StandardOutput);
    }

    private static async Task<BufferedCommandResult> ExecuteServiceControl(string action, string[] args, CancellationToken ct)
    {
        //var fullArgs = args.Prepend(action);
        var fullArgs = new string[args.Length + 1];
        fullArgs[0] = action;
        Array.Copy(args, 0, fullArgs, 1, args.Length);

        var result = await Cli.Wrap("sc")
            .WithArguments(fullArgs)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync(ct);

        if (result.ExitCode != 0)
        {
            throw new ServiceControlException(result.StandardOutput, result.ExitCode);
        }

        return result;
    }

    private static string[] ConvertDescriptorToArgs(DaemonDescriptor options)
    {
        // $"binPath=dotnet {AnyDaemonServiceRunner}",
        var startCommand = $"{AnyDaemonInstallationHelper.GetInstallationPath()} {AnyDaemonInstallationHelper.ServiceNameArgument}={options.Name}";

        // Put the arguments into the json config file
        //if (options.Arguments is null)
        //    startCommand += $" {options.Arguments}";

        return new[]
        {
            options.Name,
            $"binPath={startCommand}",
            $"start={options.StartType.GetLabel()}",
            $"displayname={options.DisplayName}"
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