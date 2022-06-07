using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace OnlineTravelAgency.Shared.RabbitMq;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMqAmqpConnection(this IServiceCollection serviceCollection, string clientProvidedName)
    {
        ArgumentNullException.ThrowIfNull(clientProvidedName);

        serviceCollection.AddSingleton((serviceProvider) =>
        {
            var amqpConnectionFactory = new ConnectionFactory()
            {
                ConsumerDispatchConcurrency = 1,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                UserName = ConnectionFactory.DefaultUser,
                Password = ConnectionFactory.DefaultPass,
                VirtualHost = ConnectionFactory.DefaultVHost,
                HostName = "localhost",
                Port = 5672,
                ClientProvidedName = clientProvidedName,
                RequestedHeartbeat = TimeSpan.FromSeconds(20),
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            var logger = serviceProvider.GetRequiredService<ILogger<ConnectionFactory>>();
            while (true)
            {
                try
                {
                    return amqpConnectionFactory.CreateConnection();
                }
                catch (BrokerUnreachableException ex)
                {
                    logger.LogCritical(ex, "An exception occurred when creating an AMQP Connection");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "An exception occurred ");
                }
                Thread.Sleep(2000);
            }
        });

        return serviceCollection;
    }

    public static string CreateConsumer<TConsumer>(this IConnection amqpConnection,
                                                   IServiceProvider serviceProvider,
                                                   RabbitMqConsumer<TConsumer> rabbitMqConsumer)
       where TConsumer : AsyncEventingBasicConsumer
    {
        IModel amqpChannel = amqpConnection.CreateModel();
        //set prefetchSize to zero, meaning "no specific limit"
        amqpChannel.BasicQos(0, rabbitMqConsumer.PrefetchCount, rabbitMqConsumer.GlobalPrefetchCount);

        amqpChannel.ExchangeDeclare(rabbitMqConsumer.ExchangeDetails);

        if (rabbitMqConsumer.DeadLetterExchangeDetails is not null)
        {
            amqpChannel.ExchangeDeclare(rabbitMqConsumer.DeadLetterExchangeDetails);
            rabbitMqConsumer.QueueDetails.QueueArguments.Add(Headers.XDeadLetterExchange, rabbitMqConsumer.DeadLetterExchangeDetails.ExchangeName);
        }

        string queueName = rabbitMqConsumer.QueueDetails.QueueName;
        if (string.IsNullOrWhiteSpace(rabbitMqConsumer.BindingDetails.RoutingKey) is false)
        {
            queueName = $"{rabbitMqConsumer.QueueDetails.QueueName}.{rabbitMqConsumer.BindingDetails.RoutingKey}";
        }
        amqpChannel.QueueDeclare(queueName,
                                 durable: rabbitMqConsumer.QueueDetails.Durable,
                                 exclusive: false,
                                 autoDelete: rabbitMqConsumer.QueueDetails.AutoDelete,
                                 arguments: rabbitMqConsumer.QueueDetails.QueueArguments);

        amqpChannel.QueueBind(queueName,
                              rabbitMqConsumer.ExchangeDetails.ExchangeName,
                              rabbitMqConsumer.BindingDetails.RoutingKey,
                              rabbitMqConsumer.BindingDetails.BindingArguments);

        TConsumer consumer = ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider, amqpChannel);
        return amqpChannel.BasicConsume(queueName, rabbitMqConsumer.AutoAcknowledgement, consumer);
    }

    public static void ExchangeDeclare(this IModel amqpChannel, RabbitMqExchange rabbitMqExchange)
    {
        amqpChannel.ExchangeDeclare(exchange: rabbitMqExchange.ExchangeName,
                                    type: rabbitMqExchange.ExchangeType,
                                    durable: rabbitMqExchange.Durable,
                                    autoDelete: rabbitMqExchange.AutoDelete,
                                    arguments: rabbitMqExchange.ExchangeArguments);
    }

    public static async Task WrapRabbitMqCallsWithRetryForEver(Action action, CancellationToken cancellationToken, ILogger logger)
    {
        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                action();
                break;
            }
            catch (BrokerUnreachableException ex)
            {
                logger.LogCritical(ex, "Unable to open an AMQP connection for RabbitMQ");
            }
            catch (AlreadyClosedException ex)
            {
                logger.LogCritical(ex, "Connection was opened but couldn't open a new AMQP channel for RabbitMQ");
            }
            catch (OperationInterruptedException ex)
            {
                logger.LogCritical(ex, "We have some problems in configure section (declaring queues)");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "RabbitMQ has some connection issues");
            }
            await Task.Delay(3000);
        }
    }
}
