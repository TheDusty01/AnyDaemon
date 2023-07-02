using Microsoft.Extensions.DependencyInjection;
using CliFx;

namespace AnyDaemon.UI.Cli;

public class Program
{
    private static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(commandTypes =>
            {
                var services = new ServiceCollection();

                // Register services
                services.AddAdInstaller();


                // Register commands
                foreach (var commandType in commandTypes)
                    services.AddTransient(commandType);

                return services.BuildServiceProvider();
            })
            .Build()
            .RunAsync();
    }
}
