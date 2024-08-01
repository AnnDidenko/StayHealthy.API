using MediatR;
using Microsoft.AspNetCore.Mvc;
using StayHealthy.Application.Models.Availability;
using StayHealthy.Application.Queries;

namespace StayHealthy.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AvailabilityController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(WeeklyAvailabilityResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAvailabilityAsync([FromQuery] DateOnly date)
    {
        var query = new GetAvailabilityQuery{ Date = date };
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}