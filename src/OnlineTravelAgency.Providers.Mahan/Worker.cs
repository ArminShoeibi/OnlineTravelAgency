using RabbitMQ.Client;

namespace OnlineTravelAgency.Providers.Mahan
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConnection _amqpConnection;

        public Worker(ILogger<Worker> logger, IConnection amqpConnection)
        {
            _logger = logger;
            _amqpConnection = amqpConnection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("IsOpen: {IsOpen}", _amqpConnection.IsOpen);
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}