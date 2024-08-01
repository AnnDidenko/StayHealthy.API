using StayHealthy.Client.Models.Appointment;
using StayHealthy.Client.Models.Availability;

namespace StayHealthy.Client.ApiClients;

public interface ISlotsClient
{
    Task<WeeklyAvailabilityResponse> GetWeeklyAvailabilityAsync(string date);
    Task MakeAppointmentAsync(AppointmentRequest request);
}