using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        Console.WriteLine("Starting..");

        while (!ct.IsCancellationRequested)
        {
            Console.WriteLine($"Current utc time is {DateTime.UtcNow}");
            try
            {
                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Abort requested");
            }

        }

        Console.WriteLine("Finished!");
    }
}