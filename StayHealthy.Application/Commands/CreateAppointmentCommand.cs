using MediatR;
using StayHealthy.Application.Models.Appointment;

namespace StayHealthy.Application.Commands;

public class CreateAppointmentCommand : IRequest
{
    public CreateAppointmentCommand(AppointmentRequestModel appointmentRequest)
    {
        AppointmentRequest = appointmentRequest;
    }

    public AppointmentRequestModel AppointmentRequest { get; set; }
    
}