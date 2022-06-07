using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.Providers.Mahan.Consumers;
internal class IssueTicketConsumer : AsyncEventingBasicConsumer
{
    private readonly ILogger<IssueTicketConsumer> _logger;

    public IssueTicketConsumer(IModel model, ILogger<IssueTicketConsumer> logger) : base(model)
    {
        this._logger = logger;
    }
}
