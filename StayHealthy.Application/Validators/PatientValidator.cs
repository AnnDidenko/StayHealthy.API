using FluentValidation;
using StayHealthy.Application.Models.Appointment;

namespace StayHealthy.Application.Validators;

public class PatientValidator : AbstractValidator<PatientModel>
{
    public PatientValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty();

        RuleFor(p => p.SecondName)
            .NotEmpty();

        RuleFor(p => p.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();
        
        RuleFor(p => p.Phone)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Matches(@"^\+[0-9]{1,3}[0-9]{3,14}$");
    }
}