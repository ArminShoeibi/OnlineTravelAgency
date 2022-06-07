using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.Providers.Mahan;
internal class ReserveFlightConsumer : AsyncEventingBasicConsumer
{
    private readonly ILogger<ReserveFlightConsumer> logger;
    public ReserveFlightConsumer(IModel model, ILogger<ReserveFlightConsumer> logger) : base(model)
    {
        this.logger = logger;
    }
}
