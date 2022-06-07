using RabbitMQ.Client;

namespace OnlineTravelAgency.Shared.RabbitMq;
public static class RabbitMqQueues
{
    public static RabbitMqQueue AvailableFlightsRequestQueue = new()
    {
        AutoDelete = false,
        Durable = false, // handling RabbitMQ Cluster, we don't need Mirroring or Quorum queues for available flights
        QueueName = "OnlineTravelAgency.Queues.AvailableFlightsRequest",
        QueueArguments = new Dictionary<string, object>()
        {
            {Headers.XQueueType, "classic"}, // HA
            {Headers.XMessageTTL, 90_000} , // Since RabbitMQ 3.10 with quorum
        }
    };
}

public static class RabbitMqExchanges
{
    public static RabbitMqExchange AvailableFlightsRequestExchange = new()
    {
        ExchangeName = "OnlineTravelAgency.Exchanges.AvailableFlightsRequest",
        AutoDelete = false,
        Durable = true, // keep it durable, we have no worries about clustering in Exchanges.
        ExchangeType = ExchangeType.Direct,
    };
}