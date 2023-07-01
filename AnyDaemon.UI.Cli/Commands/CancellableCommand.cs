using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace AnyDaemon.UI.Cli.Commands;

[Command]
public class CancellableCommand : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        // Make the command cancellation-aware
        var ct = console.RegisterCancellationHandler();

        console.Output.WriteLine("Done.");
    }
}