using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using System.Diagnostics;

namespace AnyDaemon.UI.Cli.Commands;

[Command("create", Description = "Creates a new AnyDaemon service")]
public class CreateServiceCommand : ICommand
{
    private readonly IServiceManager serviceManager;

    public CreateServiceCommand(IServiceManager serviceManager)
    {
        this.serviceManager = serviceManager;
    }

    [CommandParameter(0, Description = "Unique service name")]
    public required string Name { get; init; }

    [CommandParameter(1, Description = "Display name of the service")]
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

        var prefix = DisplayNamePrefix is null ? $"AnyDaemon" : DisplayNamePrefix;
        var serviceDescriptor = new DaemonDescriptor
        {
            Name = Name,
            DisplayName = $"{prefix}: {DisplayName}",
            ExecutablePath = ExecutablePath,
            WorkingDir = WorkingDir,
            Arguments = Arguments,
        };
        if (StartType is not null)
            serviceDescriptor.StartType = StartType.Value;

        try
        {
            await serviceManager.CreateServiceAsync(new ServiceConfiguration
            {
                ServiceDescriptor = serviceDescriptor
            }, ct);
        }
        catch (Exception ex)
        {
            console.Output.WriteLine($"Failed to create service: {ex.Message}");
            return;
        }

        console.Output.WriteLine("Created.");
    }
}