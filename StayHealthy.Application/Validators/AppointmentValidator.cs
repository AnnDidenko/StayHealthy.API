using FluentValidation;
using StayHealthy.Application.Models.Appointment;

namespace StayHealthy.Application.Validators;

public class AppointmentValidator : AbstractValidator<AppointmentRequestModel>
{
    public AppointmentValidator()
    {
        RuleFor(a => a.FacilityId)
            .NotEmpty();

        RuleFor(a => a.Start)
            .NotEmpty();

        RuleFor(a => a.End)
            .NotEmpty();

        RuleFor(a => a.Patient)
            .SetValidator(new PatientValidator());
    }
}