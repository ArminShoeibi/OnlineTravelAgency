using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.Providers.Mahan;

internal class AvailableFlightsRequestConsumer : AsyncEventingBasicConsumer
{
    public AvailableFlightsRequestConsumer(IModel model) : base(model)
    {
    }

    public override Task HandleBasicDeliver(string consumerTag,
                                            ulong deliveryTag,
                                            bool redelivered,
                                            string exchange,
                                            string routingKey,
                                            IBasicProperties properties,
                                            ReadOnlyMemory<byte> body)
    {
        return base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
    }
}
