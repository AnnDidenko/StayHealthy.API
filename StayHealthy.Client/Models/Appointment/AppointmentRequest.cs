namespace StayHealthy.Client.Models.Appointment;

public class AppointmentRequest
{
    public AppointmentRequest(Guid facilityId, DateTime start, DateTime end, string? comments, Patient patient)
    {
        FacilityId = facilityId;
        Start = start;
        End = end;
        Comments = comments;
        Patient = patient;
    }
    
    public Guid FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Comments { get; set; }
    public Patient Patient { get; set; }
}