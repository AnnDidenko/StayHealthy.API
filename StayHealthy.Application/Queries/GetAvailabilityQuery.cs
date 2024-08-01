using MediatR;
using StayHealthy.Application.Models.Availability;

namespace StayHealthy.Application.Queries;

public class GetAvailabilityQuery : IRequest<WeeklyAvailabilityResponseModel>
{
    public DateOnly Date { get; set; }
}