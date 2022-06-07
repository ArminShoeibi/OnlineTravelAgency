using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.Providers.Mahan.Consumers;
internal class IssueTicketConsumer : AsyncEventingBasicConsumer
{
    public IssueTicketConsumer(IModel model) : base(model)
    {
    }
}
