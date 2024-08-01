namespace StayHealthy.Application.Models.Appointment;

public class AppointmentRequestModel
{
    public Guid FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Comments { get; set; }
    public required PatientModel Patient { get; set; }
}