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

    public static RabbitMqQueue ReserveFlightQueue = new()
    {
        AutoDelete = false,
        Durable = true,
        QueueName = "OnlineTravelAgency.Queues.ReserveFlightQueue",
        QueueArguments = new Dictionary<string, object>()
        {
            {Headers.XQueueType, "quorum"}, // HA
            {Headers.XMessageTTL, 240_000} , // Since RabbitMQ 3.10 with quorum
            {"x-delivery-limit", 1},// https://dradoaica.blogspot.com/2020/03/poison-message-handling-rabbitmq.html and needs **Idempotency** if you don't have Idempotency put 0 for the value
            {"x-queue-leader-locator", "balanced"},
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

    public static RabbitMqExchange ReserveFlightExchange = new()
    {
        ExchangeName = "OnlineTravelAgency.Exchanges.ReserveFlightExchange",
        AutoDelete = false,
        Durable = true,
        ExchangeType = ExchangeType.Direct,
    };
}