using FluentValidation;
using MediatR;
using StayHealthy.Application.Caching;
using StayHealthy.Application.Commands;
using StayHealthy.Application.Exceptions;
using StayHealthy.Application.Extensions;
using StayHealthy.Application.Models.Appointment;
using StayHealthy.Application.Models.Availability;
using StayHealthy.Application.Queries;
using StayHealthy.Client.ApiClients;
using StayHealthy.Client.Models.Appointment;

namespace StayHealthy.Application.CommandHandlers;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand>
{
    private readonly ISlotsClient _slotsClient;
    private readonly IMediator _mediator;
    private readonly ICacheProvider _cacheProvider;
    private readonly IValidator<AppointmentRequestModel> _validator;

    public CreateAppointmentCommandHandler(
        ISlotsClient slotsClient,
        IMediator mediator,
        ICacheProvider cacheProvider,
        IValidator<AppointmentRequestModel> validator)
    {
        _slotsClient = slotsClient;
        _mediator = mediator;
        _cacheProvider = cacheProvider;
        _validator = validator;
    }

    public async Task Handle(CreateAppointmentCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command.AppointmentRequest, cancellationToken);

        var patient = command.AppointmentRequest.Patient;
        var appointmentRequest = command.AppointmentRequest;

        if (!await IsSlotAvailableAsync(appointmentRequest.Start, appointmentRequest.End))
        {
            throw new TimeSlotConflictException(appointmentRequest.Start,
                appointmentRequest.End);
        }

        var date = DayOfWeekExtension.GetMondayOfWeek(DateOnly.FromDateTime(command.AppointmentRequest.Start))
            .ToString("yyyyMMdd");

        _cacheProvider.Remove(GetWeeklyAvailabilityCacheKey(date));

        await _slotsClient.MakeAppointmentAsync(
            new AppointmentRequest(appointmentRequest.FacilityId,
                appointmentRequest.Start, appointmentRequest.End, appointmentRequest.Comments,
                new Patient(patient.Name, patient.SecondName,
                    patient.Email, patient.Phone)));
    }

    private async Task<WeeklyAvailabilityResponseModel> GetAvailabilityAsync(DateTime date)
    {
        var query = new GetAvailabilityQuery { Date = DateOnly.FromDateTime(date) };
        var weeklyAvailabilityResponse = await _mediator.Send(query);
        return weeklyAvailabilityResponse;
    }

    private async Task<bool> IsSlotAvailableAsync(
        DateTime appointmentStart,
        DateTime appointmentEnd)
    {
        var availability = await GetAvailabilityAsync(appointmentStart);

        availability.WeekSchedule.TryGetValue(appointmentStart.DayOfWeek, out var daySchedule);

        var isWorkingDay = daySchedule != null;
        var isSlotAvailable = daySchedule != null &&
                              daySchedule.AvailableTimeSlots.Any(s =>
                                  s.Start == appointmentStart && s.End == appointmentEnd);

        return isWorkingDay && isSlotAvailable;
    }

    private string GetWeeklyAvailabilityCacheKey(string date)
    {
        return $"availability-{date}";
    }
}