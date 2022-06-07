using OnlineTravelAgency.Shared.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace OnlineTravelAgency.Providers.Mahan.Consumers;

internal class AvailableFlightsRequestConsumer : AsyncEventingBasicConsumer
{
    private readonly ILogger<AvailableFlightsRequestConsumer> _logger;

    public AvailableFlightsRequestConsumer(IModel model, ILogger<AvailableFlightsRequestConsumer> logger) : base(model)
    {
        _logger = logger;
    }

    public override Task HandleBasicDeliver(string consumerTag,
                                            ulong deliveryTag,
                                            bool redelivered,
                                            string exchange,
                                            string routingKey,
                                            IBasicProperties properties,
                                            ReadOnlyMemory<byte> body)
    {
        var avRequest = JsonSerializer.Deserialize<AvailableFlightsRequestDto>(body.Span);
        _logger.LogInformation("Available flights request: {AvailableFlightsRequestDto}", avRequest); // We can do this because it is a record class
        return Task.CompletedTask;
    }
}
