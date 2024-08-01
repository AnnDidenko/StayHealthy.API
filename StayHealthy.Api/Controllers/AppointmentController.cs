using MediatR;
using Microsoft.AspNetCore.Mvc;
using StayHealthy.Application.Commands;
using StayHealthy.Application.Models.Appointment;

namespace StayHealthy.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/appointment")]
public class AppointmentController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public AppointmentController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MakeAppointmentAsync([FromBody] AppointmentRequestModel appointmentRequest)
    {
        var command = new CreateAppointmentCommand(appointmentRequest);
        await _mediator.Send(command);
        return Ok();
    }
}