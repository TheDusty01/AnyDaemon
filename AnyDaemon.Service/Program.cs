using AnyDaemon.Service;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddAdConfigurationManager();

        services.AddHostedService<ServiceRunner>();
    })
    .Build();

host.Run();
