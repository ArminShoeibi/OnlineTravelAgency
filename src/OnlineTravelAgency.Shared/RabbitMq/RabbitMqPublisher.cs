using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OnlineTravelAgency.Shared.RabbitMq;
public class RabbitMqPublisher
{
    // please don't use concurrent dic for this because GetOrAdd(TKey, Func<TKey,TValue>) method is not thread safe.
    private readonly Dictionary<RabbitMqExchange, (IModel, object)> _amqpChannels = new();

    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IConnection _amqpConnection;
    private readonly RetryPolicy _retryPolicy;
    private readonly object _lock = new object();
    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IConnection amqpConnection)
    {
        _logger = logger;
        this._amqpConnection = amqpConnection;
        _retryPolicy = Policy.Handle<Exception>()
                         .WaitAndRetry(5, waitDuration => TimeSpan.FromSeconds(8), onRetry: (exception, waitDuration, retryCount, context) =>
                         {
                             _logger.LogError(
                                 exception,
                                 "An exception occurred when Publishing Result. WaitDuration: {WaitDuration} - RetryCount: {RetryCount} - Context: {@Context} ",
                                 waitDuration,
                                 retryCount,
                                 context);
                         });
    }

    public void Publish<T>(T message,
                        RabbitMqExchange rabbitMqExchange,
                        string routingKey,
                        bool publisherConfirms,
                        IDictionary<string, object> pollyContextData)
    {
        PolicyResult policyResult = _retryPolicy.ExecuteAndCapture((context) =>
        {
            (IModel amqpChannel, object lockObj) = GetAmqpChannel(rabbitMqExchange, publisherConfirms);
            var messageAsJson = JsonSerializer.Serialize(message);
            byte[] messageAsBytes = Encoding.UTF8.GetBytes(messageAsJson);
            lock (lockObj)
            {
                amqpChannel.BasicPublish(rabbitMqExchange.ExchangeName, routingKey, body: messageAsBytes);
                if (publisherConfirms)
                {
                    amqpChannel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(45)); // maybe you need to change this for every type of message
                }
            }
        }, pollyContextData);

        if (policyResult.Outcome == OutcomeType.Successful)
        {
            _logger.LogInformation("A message with type of: {Type} published to Exchange: {Exchange} with RoutingKey: {RoutingKey}", typeof(T), rabbitMqExchange.ExchangeName, routingKey);
        }
        else
        {
            _logger.LogError(policyResult.FinalException, "An exception occurred. Context: {Context}", policyResult.Context);
            throw policyResult.FinalException;
        }
    }

    public (IModel, object) GetAmqpChannel(RabbitMqExchange rabbitMqExchange, bool publisherConfirms)
    {
        (IModel AmqpChannel, object LockObject) amqpChannelWithLockObject;

        if (_amqpChannels.TryGetValue(rabbitMqExchange, out amqpChannelWithLockObject)) return amqpChannelWithLockObject;
        lock (_lock) // Execute once per Exchange
        {
            if (_amqpChannels.TryGetValue(rabbitMqExchange, out amqpChannelWithLockObject)) return amqpChannelWithLockObject; // double-validation (Mandatory)

            amqpChannelWithLockObject.AmqpChannel = _amqpConnection.CreateModel();
            if (publisherConfirms)
            {
                amqpChannelWithLockObject.AmqpChannel.ConfirmSelect();
            }
            amqpChannelWithLockObject.LockObject = new object();
            return amqpChannelWithLockObject;
        }
    }
}
