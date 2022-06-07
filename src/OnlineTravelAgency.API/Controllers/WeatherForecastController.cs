using Microsoft.AspNetCore.Mvc;
using OnlineTravelAgency.Shared.DTOs;
using OnlineTravelAgency.Shared.RabbitMq;

namespace OnlineTravelAgency.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestRabbitPublisher : ControllerBase
{
    private readonly ILogger<TestRabbitPublisher> _logger;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public TestRabbitPublisher(ILogger<TestRabbitPublisher> logger, RabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        this._rabbitMqPublisher = rabbitMqPublisher;
    }


    [HttpPost("[action]")]
    public IActionResult PublishAvailableFlightsRequest(AvailableFlightsRequestDto availableFlightsRequestDto)
    {
        Jsonseri
        _rabbitMqPublisher.Publish()
    }
}