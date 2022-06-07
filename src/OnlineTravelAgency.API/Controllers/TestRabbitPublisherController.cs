using Microsoft.AspNetCore.Mvc;
using OnlineTravelAgency.Shared.DTOs;
using OnlineTravelAgency.Shared.RabbitMq;

namespace OnlineTravelAgency.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestRabbitPublisherController : ControllerBase
{
    private readonly ILogger<TestRabbitPublisherController> _logger;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public TestRabbitPublisherController(ILogger<TestRabbitPublisherController> logger, RabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        this._rabbitMqPublisher = rabbitMqPublisher;
    }


    [HttpPost("[action]")]
    public IActionResult PublishAvailableFlightsRequest(AvailableFlightsRequestDto availableFlightsRequestDto)
    {
        _rabbitMqPublisher.Publish(availableFlightsRequestDto, RabbitMqExchanges.AvailableFlightsRequestExchange,"Mahan",false, new Dictionary<string,object>
        {
            {"MethodName", nameof(PublishAvailableFlightsRequest)}
        });
        return Ok();
    }
}