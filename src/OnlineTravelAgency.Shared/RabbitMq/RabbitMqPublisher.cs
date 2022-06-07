using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace OnlineTravelAgency.Shared.RabbitMq;
public class RabbitMqPublisher
{
    private readonly Dictionary<RabbitMqExchange, (IModel, object)> _amqpChannels = new(); // please don't use concurrent dic for this because GetOrAdd(TKey, Func<TKey,TValue>) method is not thread safe.
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


    public void Publish(byte[] message,
                        RabbitMqExchange rabbitMqExchange,
                        string routingKey,
                        bool publisherConfirms,
                        IDictionary<string, object> pollyContextData = null)
    {

        PolicyResult policyResult = _retryPolicy.ExecuteAndCapture((context) =>
        {
            (IModel amqpChannel, object lockObj) = GetAmqpChannel(rabbitMqExchange, publisherConfirms);
            lock (lockObj)
            {
                amqpChannel.BasicPublish(rabbitMqExchange.ExchangeName, routingKey, body: message);
                if (publisherConfirms)
                {
                    amqpChannel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(45)); // maybe you need to change this for every type of message
                }
            }
        }, pollyContextData);

        if (policyResult.Outcome == OutcomeType.Successful)
        {

        }
        else
        {

        }
    }

    public (IModel, object) GetAmqpChannel(RabbitMqExchange rabbitMqExchange, bool publisherConfirms)
    {
        (IModel AmqpChannel, object LockObject) amqpChannelWithLockObject;

        if (_amqpChannels.TryGetValue(rabbitMqExchange, out amqpChannelWithLockObject)) return amqpChannelWithLockObject;
        lock (_lock)
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
