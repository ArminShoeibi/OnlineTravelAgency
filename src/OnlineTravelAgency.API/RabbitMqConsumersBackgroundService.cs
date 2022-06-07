using OnlineTravelAgency.API.Consumers;
using OnlineTravelAgency.Shared.RabbitMq;
using RabbitMQ.Client;

namespace OnlineTravelAgency.API;

public class RabbitMqConsumersBackgroundService : BackgroundService
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
        // With DLX
        await RabbitMqExtensions.WrapRabbitMqCallsWithRetryForEver(() =>
        {
            _amqpConnection.CreateConsumer(_serviceProvider, new RabbitMqConsumer<IssueTicketDeadLetterConsumer>
            {
                PrefetchCount = 15,
                GlobalPrefetchCount = true,
                AutoAcknowledgement = false,
                BindingDetails = new()
                {
                    RoutingKey = "",
                },
                QueueDetails = RabbitMqQueues.IssueTicketDeadLetterQueue,
                ExchangeDetails = RabbitMqExchanges.IssueTicketDeadLetterExchange,
            });
        }, stoppingToken, _logger);
    }
}
