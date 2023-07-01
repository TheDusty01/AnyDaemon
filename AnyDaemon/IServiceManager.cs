using System.Diagnostics.Contracts;
using System.Text;
using System.Text.Json;
using System.Threading;
using CliWrap;
using CliWrap.Buffered;
using FastEnumUtility;

namespace AnyDaemon;


public enum ServiceStartType
{
    [Label("auto")]
    Auto,
    [Label("delayed-auto")]
    Delayed,
    [Label("demand")]
    Demand,
    [Label("disabled")]
    Disabled
}


public class DaemonStatus
{
    //typedef struct _SERVICE_STATUS
    //{
    //    DWORD dwServiceType;
    //    DWORD dwCurrentState;
    //    DWORD dwControlsAccepted;
    //    DWORD dwWin32ExitCode;
    //    DWORD dwServiceSpecificExitCode;
    //    DWORD dwCheckPoint;
    //    DWORD dwWaitHint;
    //}
    //SERVICE_STATUS, *LPSERVICE_STATUS;

    public required ServiceState State { get; set; }
}

public enum ServiceState
{
    // SERVICE_STOPPED
    // 0x00000001
    // The service is not running.
    Stopped = 1,

    // SERVICE_START_PENDING
    // 0x00000002
    // The service is starting.
    StartPending = 2,

    // SERVICE_STOP_PENDING
    // 0x00000003
    // The service is stopping.
    StopPending = 3,

    // SERVICE_RUNNING
    // 0x00000004
    // The service is running.
    Running = 4,

    // SERVICE_CONTINUE_PENDING
    // 0x00000005
    // The service continue is pending.
    ContinuePending = 5,

    // SERVICE_PAUSE_PENDING
    // 0x00000006
    // The service pause is pending.
    PausePending = 6,

    // SERVICE_PAUSED
    // 0x00000007
    // The service is paused.
    Paused = 7,
}

public class DaemonDescriptor
{
    public required string BinPath { get; set; }
    public string? Arguments { get; set; }
    public required string Name { get; set; }
    public ServiceStartType StartType { get; set; } = ServiceStartType.Delayed;

    public required string DisplayName { get; set; }
    public string DisplayNamePrefix { get; set; } = "AnyDaemon: ";
    public string FullDisplayName => $"{DisplayNamePrefix}{DisplayName}";

}

public interface IServiceManager
{
    Task CreateServiceAsync(DaemonDescriptor options, CancellationToken ct = default);
    Task EditServiceAsync(DaemonDescriptor options, CancellationToken ct = default);
    Task DeleteServiceAsync(string serviceName, CancellationToken ct = default);

    // TODO: Maybe move start/stop logic to "AnyDaemon background service" and communicate via
    //  in message pipe or local http server
    Task StartServiceAsync(string serviceName, CancellationToken ct = default);
    Task StopServiceAsync(string serviceName, CancellationToken ct = default);

    Task<DaemonDescriptor> GetServiceInfoAsync(string serviceName, CancellationToken ct = default);
    Task<DaemonStatus> GetServiceStatus(string serviceName, CancellationToken ct = default);
}

public class ServiceManager : IServiceManager
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
    };

private enum ServiceControlExitCode
    {
        Success = 0,
        AccessDenied = 5,
        AlreadyRunning = 1056,
        ServiceDoesNotExist = 1060,
        AlreadyExists = 1073,
    }

    private const string AnyDaemonServiceRunner = "AnyDaemon.Service.exe";

    private static string[] ConvertDescriptorToArgs(string action, DaemonDescriptor options)
    {
        // $"binPath=dotnet {AnyDaemonServiceRunner}",
        var startCommand = $"{AnyDaemonServiceRunner} {options.BinPath}";

        if (options.Arguments is null)
            startCommand += $" {options.Arguments}";

        return new[]
        {
            action,
            options.Name,
            $"binPath={startCommand}",
            $"start={options.StartType.FastToString()}",
            $"displayname={options.FullDisplayName}"
        };
    }

    private static DaemonDescriptor ParseDescriptor(string rawInfo)
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

    private static async Task SaveConfigurationAsync(DaemonDescriptor options, CancellationToken ct)
    {
        var serialized = JsonSerializer.Serialize(options, jsonOptions);
        await File.WriteAllTextAsync(
            AnyDaemonInstallationHelper.GetConfigPath(options.Name),
            serialized,
            ct
        );
    }

    private static void DeleteConfiguration(string serviceName)
    {
        File.Delete(AnyDaemonInstallationHelper.GetConfigPath(serviceName));
    }

    [Pure]
    public async Task CreateServiceAsync(DaemonDescriptor options, CancellationToken ct = default)
    {
        await Cli.Wrap("sc")
            .WithArguments(ConvertDescriptorToArgs("create", options))
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(ct);

        await SaveConfigurationAsync(options, ct);
    }

    public async Task EditServiceAsync(DaemonDescriptor options, CancellationToken ct = default)
    {
        await Cli.Wrap("sc")
            .WithArguments(ConvertDescriptorToArgs("config", options))
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(ct);

        await SaveConfigurationAsync(options, ct);
    }

    public async Task DeleteServiceAsync(string serviceName, CancellationToken ct = default)
    {
        await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "delete",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteAsync(ct);

        DeleteConfiguration(serviceName);
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

    public async Task<DaemonDescriptor> GetServiceInfoAsync(string serviceName, CancellationToken ct = default)
    {
        var result = await Cli.Wrap("sc")
            .WithArguments(new[]
            {
                "qc",
                serviceName
            })
            .WithValidation(CommandResultValidation.ZeroExitCode)
            .ExecuteBufferedAsync(ct);

        return ParseDescriptor(result.StandardOutput);
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
}