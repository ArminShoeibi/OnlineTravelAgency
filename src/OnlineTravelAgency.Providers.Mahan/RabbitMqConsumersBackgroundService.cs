using OnlineTravelAgency.Providers.Mahan.Consumers;
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
        // Wrap every CreateConsumer consumer with this method, don't put more than one consumer in this method.
        await RabbitMqExtensions.WrapRabbitMqCallsWithRetryForEver(() =>
        {
            _amqpConnection.CreateConsumer(_serviceProvider, new RabbitMqConsumer<AvailableFlightsRequestConsumer>
            {
                PrefetchCount = 10,
                GlobalPrefetchCount = true,
                AutoAcknowledgement = true, // auto-ack because these type of message is cheap and we really don't care about it.
                BindingDetails = new()
                {
                    RoutingKey = "Mahan"
                },
                QueueDetails = RabbitMqQueues.AvailableFlightsRequestQueue,
                ExchangeDetails = RabbitMqExchanges.AvailableFlightsRequestExchange,
            });
        }, stoppingToken, _logger);


        // Without DLX
        await RabbitMqExtensions.WrapRabbitMqCallsWithRetryForEver(() =>
        {
            _amqpConnection.CreateConsumer(_serviceProvider, new RabbitMqConsumer<ReserveFlightConsumer>
            {
                PrefetchCount = 15,
                GlobalPrefetchCount = false, // you can't turn on global prefetch count with quorom queues
                AutoAcknowledgement = false, // quorum queues needs manual ack + publisher confirms
                BindingDetails = new()
                {
                    RoutingKey = "Mahan"
                },
                QueueDetails = RabbitMqQueues.ReserveFlightQueue,
                ExchangeDetails = RabbitMqExchanges.ReserveFlightExchange,
            });
        }, stoppingToken, _logger);


        // With DLX
        await RabbitMqExtensions.WrapRabbitMqCallsWithRetryForEver(() =>
        {
            _amqpConnection.CreateConsumer(_serviceProvider, new RabbitMqConsumer<IssueTicketConsumer>
            {
                PrefetchCount = 15,
                GlobalPrefetchCount = false,
                AutoAcknowledgement = false, // quorum queues needs manual ack + publisher confirms
                BindingDetails = new()
                {
                    RoutingKey = "Mahan"
                },
                QueueDetails = RabbitMqQueues.IssueTicketQueue,
                ExchangeDetails = RabbitMqExchanges.IssueTicketExchange,
                DeadLetterExchangeDetails = RabbitMqExchanges.IssueTicketDeadLetterExchange,
            });
        }, stoppingToken, _logger);
    }
}