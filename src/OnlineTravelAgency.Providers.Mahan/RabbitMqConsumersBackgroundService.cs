using OnlineTravelAgency.Shared.RabbitMq;
using RabbitMQ.Client;

namespace OnlineTravelAgency.Providers.Mahan;

internal class RabbitMqConsumersBackgroundService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumersBackgroundService> _logger;
    private readonly IConnection _amqpConnection;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqConsumersBackgroundService(ILogger<RabbitMqConsumersBackgroundService> logger, IConnection amqpConnection, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _amqpConnection = amqpConnection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await RabbitMqExtensions.WrapRabbitMqCallsWithRetryForEver(() =>
        {
            _amqpConnection.CreateConsumer(_serviceProvider, new RabbitMqConsumer<ReserveFlightConsumer>
            {

            });
        }, stoppingToken, _logger);
    }
}