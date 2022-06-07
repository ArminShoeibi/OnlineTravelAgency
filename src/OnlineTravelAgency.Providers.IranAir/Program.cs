using OnlineTravelAgency.Providers.IranAir;
using OnlineTravelAgency.Shared.RabbitMq;

IHost host = Host.CreateDefaultBuilder(args)
    .UseDefaultServiceProvider(serviceProviderOptions =>
    {
        serviceProviderOptions.ValidateScopes = true;
        serviceProviderOptions.ValidateOnBuild = true;
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddRabbitMqAmqpConnection("IranAir");
    })
    .Build();

await host.RunAsync();
