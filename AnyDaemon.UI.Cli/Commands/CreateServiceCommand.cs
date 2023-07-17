using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace AnyDaemon.UI.Cli.Commands;

[Command(Description = "Creates a new AnyDaemon service")]
public class CreateServiceCommand : ICommand
{
    private readonly IServiceManager serviceManager;

    public CreateServiceCommand(IServiceManager serviceManager)
    {
        this.serviceManager = serviceManager;
    }

    [CommandParameter(0, Description = "")]
    public required string Name { get; init; }

    [CommandParameter(1, Description = "")]
    public required string DisplayName { get; init; }

    [CommandParameter(2, Description = "")]
    public required string ExecutablePath { get; init; }

    [CommandParameter(3, Description = "")]
    public required string WorkingDir { get; init; }

    [CommandParameter(4, IsRequired = false, Description = "")]
    public string? Arguments { get; init; }

    [CommandOption("namePrefix")]
    public string? DisplayNamePrefix { get; init; }

    [CommandOption("startType")]
    public ServiceStartType? StartType { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var ct = console.RegisterCancellationHandler();

        var config = new ServiceConfiguration
        {
            ServiceDescriptor = new DaemonDescriptor
            {
                Name = Name,
                DisplayName = DisplayName,
                ExecutablePath = ExecutablePath,
                WorkingDir = WorkingDir,
                Arguments = Arguments,
            }
        };
        if (DisplayNamePrefix is not null)
            config.ServiceDescriptor.DisplayNamePrefix = DisplayNamePrefix;
        if (StartType is not null)
            config.ServiceDescriptor.StartType = StartType.Value;


        try
        {
            await serviceManager.CreateServiceAsync(config, ct);
        }
        catch (Exception ex)
        {
            console.Output.WriteLine($"Failed to create service: {ex.Message}\n:{ex.StackTrace}");
            return;
        }

        console.Output.WriteLine("Created.");
    }
}