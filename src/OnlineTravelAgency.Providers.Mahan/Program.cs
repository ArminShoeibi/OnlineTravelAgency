using OnlineTravelAgency.Providers.Mahan;
using OnlineTravelAgency.Shared.RabbitMq;

IHost host = Host.CreateDefaultBuilder(args)
    .UseDefaultServiceProvider(serviceProviderOptions =>
    {
        serviceProviderOptions.ValidateScopes = true;
        serviceProviderOptions.ValidateOnBuild = true;
    })
    .ConfigureServices(services =>
    {
        services.AddRabbitMqAmqpConnection("Mahan");
        services.AddHostedService<RabbitMqConsumersBackgroundService>();
        services.AddSingleton<RabbitMqPublisher>();
    })
    .Build();

await host.RunAsync();
