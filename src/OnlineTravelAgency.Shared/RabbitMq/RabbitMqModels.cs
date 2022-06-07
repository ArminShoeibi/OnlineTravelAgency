using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.Shared.RabbitMq;

public sealed record class RabbitMqExchange
{
    public string ExchangeName { get; init; }
    public string ExchangeType { get; init; }
    public bool Durable { get; init; }
    public bool AutoDelete { get; init; }
    public Dictionary<string, object> ExchangeArguments { get; init; }
}

public sealed record class RabbitMqQueue
{
    public string QueueName { get; init; }
    public bool Durable { get; init; }
    public bool AutoDelete { get; init; }
    public Dictionary<string, object> QueueArguments { get; init; }
}

public sealed record class RabbitMqBinding
{
    public string RoutingKey { get; init; }
    public Dictionary<string, object> BindingArguments { get; init; }
}

public sealed record class RabbitMqConsumer<TConsumer> where TConsumer : AsyncEventingBasicConsumer
{
    public ushort PrefetchCount { get; init; }
    public bool GlobalPrefetchCount { get; init; }
    public RabbitMqExchange ExchangeDetails { get; init; }
    public RabbitMqExchange DeadLetterExchangeDetails { get; init; }
    public RabbitMqQueue QueueDetails { get; init; }
    public RabbitMqBinding BindingDetails { get; init; }
    public bool AutoAcknowledgement { get; init; }
}