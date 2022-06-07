using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace OnlineTravelAgency.Shared.RabbitMq;
public class RabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RetryPolicy _retryPolicy;
    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
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


    public void Publish(byte[] message, string exchangeName, string routingKey, bool publisherConfirms)
    {

    }
}
