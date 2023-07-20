using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AnyDaemon.UI.Cli.Commands;

[Command("delete", Description = "Deletes an AnyDaemon service")]
public class DeleteServiceCommand : ICommand
{
    private readonly IServiceManager serviceManager;

    public DeleteServiceCommand(IServiceManager serviceManager)
    {
        this.serviceManager = serviceManager;
    }

    [CommandParameter(0, Description = "Unique service name")]
    public required string Name { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var ct = console.RegisterCancellationHandler();
        
        try
        {
            await serviceManager.DeleteServiceAsync(Name, ct);
        }
        catch (Exception ex)
        {
            console.Output.WriteLine($"Failed to delete service: {ex.Message}\n:{ex.StackTrace}");
            return;
        }

        console.Output.WriteLine("Deleted.");
    }
}
