using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using AnyDaemon.Service;
using System.Runtime.Versioning;
using AnyDaemon;
using System.ServiceProcess;

[SupportedOSPlatform("windows")]
internal class Program
{
    private static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((host, services) =>
            {
                LoggerProviderOptions.RegisterProviderOptions<
                    EventLogSettings, EventLogLoggerProvider>(services);

                services.AddAdConfigurationManager();
                services.AddHostedService<ServiceRunner>();
            })
            .ConfigureLogging((host, logging) =>
            {
                logging.AddConfiguration(host.Configuration.GetSection("Logging"));

                var serviceName = host.Configuration[AnyDaemonInstallationHelper.ServiceNameArgument] ??
                    throw new Exception("ServiceName is not set");

                logging.AddEventLog(options =>
                {
                    var sc = new ServiceController(serviceName);
                    options.SourceName = sc.DisplayName;

                    sc.Close();
                });
            })
            .Build();

        host.Run();
    }
}