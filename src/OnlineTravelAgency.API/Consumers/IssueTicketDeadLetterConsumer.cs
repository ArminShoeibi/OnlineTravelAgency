using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OnlineTravelAgency.API.Consumers;

internal class IssueTicketDeadLetterConsumer : AsyncEventingBasicConsumer
{
    public IssueTicketDeadLetterConsumer(IModel model) : base(model)
    {
    }
}
